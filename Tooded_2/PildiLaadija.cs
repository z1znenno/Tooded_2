using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Tooded_2
{
    /// <summary>
    /// Laeb tootepilte nii kohalikest failidest kui ka internetist (http/https link).
    /// Internetist laetud pildid salvestatakse kettale (%LOCALAPPDATA%\Tooded_2\pildid)
    /// ja mällu, et poe uuesti avamine oleks kiire ja töötaks ka ilma internetita.
    /// Kõik meetodid tagastavad vea korral null ega viska erindit.
    /// Tagastatud Bitmap on alati uus koopia – kutsuja võib (ja peab) selle ise vabastama.
    /// </summary>
    public static class PildiLaadija
    {
        /// <summary>Kaust, kuhu allalaetud pildid salvestatakse.</summary>
        public static readonly string VahemaluKaust = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tooded_2", "pildid");

        // Üks HttpClient kogu programmi jaoks. User-Agent on vajalik, sest nt upload.wikimedia.org
        // keeldub päringutest, millel see puudub.
        private static readonly HttpClient Klient = LooKlient();

        // Mälus hoitakse pildi baite (mitte Bitmap-i), et igale kutsujale saaks anda oma koopia.
        // Task-i hoidmine tähendab ka, et sama linki ei laeta korraga mitu korda.
        private static readonly ConcurrentDictionary<string, Task<byte[]?>> Malu = new();

        private const long MaxSuurus = 15 * 1024 * 1024; // 15 MB

        // Liiga palju korraga = HTTP 429. Piirame samaaegsed allalaadimised ja proovime uuesti.
        private const int MaxKorraga = 2;
        private const int Katseid = 3;
        private static readonly TimeSpan MaxOoteaeg = TimeSpan.FromSeconds(5);
        private static readonly SemaphoreSlim Jarjekord = new(MaxKorraga, MaxKorraga);

        private static HttpClient LooKlient()
        {
            var k = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            k.DefaultRequestHeaders.UserAgent.ParseAdd("Tooded_2/1.0 (desktop demo)");
            k.DefaultRequestHeaders.Accept.ParseAdd("image/*");
            return k;
        }

        /// <summary>Kas tekst on http:// või https:// link?</summary>
        public static bool OnUrl(string? tekst)
        {
            if (string.IsNullOrWhiteSpace(tekst)) return false;
            return Uri.TryCreate(tekst.Trim(), UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>Laeb pildi failist nii, et fail ei jää lukku. Vea korral null.</summary>
        public static Bitmap? LaeFailist(string? tee)
        {
            if (string.IsNullOrWhiteSpace(tee) || !File.Exists(tee)) return null;
            try
            {
                return LooBitmap(File.ReadAllBytes(tee));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Kui pilt on juba mälus olemas, tagastab kohe selle koopia (ilma ootamata).
        /// Muidu null – siis kasuta <see cref="LaeUrlistAsync"/>.
        /// </summary>
        public static Bitmap? VotaMalust(string? url)
        {
            if (!OnUrl(url)) return null;
            if (Malu.TryGetValue(url!.Trim(), out var t) && t.IsCompletedSuccessfully && t.Result != null)
                return LooBitmap(t.Result);
            return null;
        }

        /// <summary>
        /// Laeb pildi lingilt: enne mälust, siis kettalt, viimasena internetist.
        /// Vea korral (pole internetti, vale link, pole pilt) tagastab null.
        /// </summary>
        public static async Task<Bitmap?> LaeUrlistAsync(string? url)
        {
            if (!OnUrl(url)) return null;
            url = url!.Trim();

            try
            {
                var ulesanne = Malu.GetOrAdd(url, u => Task.Run(() => LaeBaidid(u)));
                byte[]? baidid = await ulesanne.ConfigureAwait(false);

                if (baidid == null)
                {
                    // Ebaõnnestunud laadimist ei jäta meelde, et järgmine kord saaks uuesti proovida
                    Malu.TryRemove(new KeyValuePair<string, Task<byte[]?>>(url, ulesanne));
                    return null;
                }
                return LooBitmap(baidid);
            }
            catch
            {
                return null;
            }
        }

        // ---------- Sisemised abimeetodid ----------

        private static async Task<byte[]?> LaeBaidid(string url)
        {
            string fail = Path.Combine(VahemaluKaust, FailiNimi(url));

            // 1) Kettalt (töötab ka ilma internetita)
            try
            {
                if (File.Exists(fail))
                {
                    byte[] olemas = await File.ReadAllBytesAsync(fail).ConfigureAwait(false);
                    if (OnPilt(olemas)) return olemas;
                    File.Delete(fail); // katkine fail – laeme uuesti
                }
            }
            catch
            {
                // ketta viga – proovime internetist
            }

            // 2) Internetist
            byte[]? baidid = await LaeAlla(url).ConfigureAwait(false);
            if (baidid == null) return null;

            // 3) Salvestame kettale (ajutine fail + ümbernimetamine, et pooleli faili ei jääks)
            try
            {
                Directory.CreateDirectory(VahemaluKaust);
                string ajutine = fail + "." + Guid.NewGuid().ToString("N") + ".tmp";
                await File.WriteAllBytesAsync(ajutine, baidid).ConfigureAwait(false);
                File.Move(ajutine, fail, true);
            }
            catch
            {
                // Kui kettale ei saa kirjutada, töötab pilt ikka mälust
            }

            return baidid;
        }

        /// <summary>
        /// Laeb pildi internetist. Korraga käib kuni <see cref="MaxKorraga"/> allalaadimist,
        /// sest nt Wikimedia vastab liiga paljudele päringutele HTTP 429-ga.
        /// 429/503 korral proovitakse uuesti (kokku <see cref="Katseid"/> korda),
        /// oodates Retry-After järgi või järjest kauem (kuni <see cref="MaxOoteaeg"/>).
        /// Pärast 429-t pannakse paus KÕIGILE allalaadimistele ja edasised päringud
        /// tehakse hõredamalt – muidu proovivad kõik pildid korraga uuesti ja saavad jälle 429.
        /// </summary>
        private static async Task<byte[]?> LaeAlla(string url)
        {
            for (int katse = 1; katse <= Katseid; katse++)
            {
                TimeSpan? oota = null;

                await Jarjekord.WaitAsync().ConfigureAwait(false);
                try
                {
                    await OotaJarjekorda().ConfigureAwait(false);

                    using var vastus = await Klient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                                                   .ConfigureAwait(false);

                    if (vastus.StatusCode == HttpStatusCode.TooManyRequests ||
                        vastus.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        oota = OoteAeg(vastus, katse);
                        Aeglusta(oota.Value);
                        if (katse == Katseid) return null;
                    }
                    else
                    {
                        if (!vastus.IsSuccessStatusCode) return null;
                        if (vastus.Content.Headers.ContentLength > MaxSuurus) return null;

                        byte[] baidid = await vastus.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        if (baidid.Length == 0 || baidid.Length > MaxSuurus || !OnPilt(baidid)) return null;
                        return baidid;
                    }
                }
                catch
                {
                    return null; // pole internetti, aegumine, vale aadress jne
                }
                finally
                {
                    Jarjekord.Release();
                }

                // Ootame väljaspool järjekorda, et teised pildid saaksid vahepeal laadida
                await Task.Delay(oota.Value).ConfigureAwait(false);
            }
            return null;
        }

        // ---------- Ühine tempo (kõigi allalaadimiste peale) ----------

        private static readonly object TempoLukk = new();
        private static DateTime jargmineAlgus = DateTime.MinValue; // UTC, varem ei alustata ühtegi päringut
        private static TimeSpan vahe = TimeSpan.Zero;              // päringute vahe pärast 429-t

        // Broneerib järgmise vaba algusaja ja ootab selleni
        private static Task OotaJarjekorda()
        {
            TimeSpan oota;
            lock (TempoLukk)
            {
                var nyyd = DateTime.UtcNow;
                var algus = jargmineAlgus > nyyd ? jargmineAlgus : nyyd;
                jargmineAlgus = algus + vahe;
                oota = algus - nyyd;
            }
            return oota > TimeSpan.Zero ? Task.Delay(oota) : Task.CompletedTask;
        }

        // Server ütles "liiga palju": paus kõigile ja edaspidi suurem vahe päringute vahel
        private static void Aeglusta(TimeSpan paus)
        {
            lock (TempoLukk)
            {
                var kuni = DateTime.UtcNow + paus;
                if (kuni > jargmineAlgus) jargmineAlgus = kuni;

                var uus = vahe + TimeSpan.FromSeconds(1); // 1 s, 2 s, 3 s ... (kuni MaxOoteaeg)
                vahe = uus > MaxOoteaeg ? MaxOoteaeg : uus;
            }
        }

        // Retry-After päisest (sekundid või kuupäev); kui puudub, siis 1 s, 2 s, 4 s ...
        private static TimeSpan OoteAeg(HttpResponseMessage vastus, int katse)
        {
            TimeSpan aeg = TimeSpan.FromSeconds(Math.Pow(2, katse - 1));
            var ra = vastus.Headers.RetryAfter;
            if (ra?.Delta is TimeSpan d) aeg = d;
            else if (ra?.Date is DateTimeOffset kp) aeg = kp - DateTimeOffset.UtcNow;

            if (aeg < TimeSpan.FromMilliseconds(200)) aeg = TimeSpan.FromMilliseconds(200);
            return aeg > MaxOoteaeg ? MaxOoteaeg : aeg;
        }

        // Failinimi lingi SHA-256 räsist, nt "3f2a...c1.pilt"
        private static string FailiNimi(string url)
        {
            byte[] rasi = SHA256.HashData(Encoding.UTF8.GetBytes(url));
            return Convert.ToHexString(rasi).ToLowerInvariant() + ".pilt";
        }

        private static bool OnPilt(byte[] baidid)
        {
            using var bmp = LooBitmap(baidid);
            return bmp != null;
        }

        // Teeb baitidest uue Bitmap-i, mis ei sõltu voost ega failist
        private static Bitmap? LooBitmap(byte[] baidid)
        {
            try
            {
                using var voog = new MemoryStream(baidid, false);
                using var algne = Image.FromStream(voog);
                return new Bitmap(algne);
            }
            catch
            {
                return null;
            }
        }
    }
}
