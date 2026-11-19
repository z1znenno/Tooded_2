using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Tooded_2
{
    /// <summary>Toote kaart poe aknas: pilt, nimi, kategooria, hind, laoseis ja nupp "Lisa korvi".</summary>
    [DesignerCategory("")] // et Visual Studio ei prooviks seda disaineris avada
    public class ToodeKaart : Panel
    {
        /// <summary>
        /// Kui tead täpselt, kus pildid asuvad, kirjuta kaust siia (nt @"C:\Projektid\Tooded_2\Pildid").
        /// Kui jätad null, otsitakse pilti automaatselt käivituskaustast ja projekti kaustast.
        /// </summary>
        public static string? PiltideKaust = null;

        // Kaustanimed, mida otsitakse käivituskaustas ja selle ülemkaustades (bin\Debug\... -> projekti kaust)
        private static readonly string[] KaustaNimed = { "Pildid", "Images", "Pictures", "Resources", "img" };
        private static readonly string[] Laiendid = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        private const int PildiLaius = 182, PildiKorgus = 150;
        // Ühised kohatäited (iga keele jaoks oma) – neid ei vabastata kunagi (kasutavad kõik kaardid)
        private static readonly Dictionary<(KeeleKood, bool), Image> kohataited = new();

        /// <summary>Mis seisus kaardi pilt on (sellest sõltuvad kohatäide ja vihje tekst).</summary>
        private enum PildiOlek { Olemas, Laadimine, Puudub, LaadimineEbaonnestus }

        private PildiOlek pildiOlek;
        private int saadaval;

        private readonly PictureBox pilt;
        private readonly ToolTip vihje = new();
        private readonly Label nimiLabel;
        private readonly Label kategooriaLabel;
        private readonly Label hindLabel;
        private readonly Label laosLabel;
        private readonly NumericUpDown kogusNum;
        private readonly TeemaNupp lisaBtn;
        private bool hover;

        public PoodToode Toode { get; }

        /// <summary>Käivitub, kui vajutatakse "Lisa korvi". Argument on valitud kogus.</summary>
        public event EventHandler<int>? LisaKorvi;

        public ToodeKaart(PoodToode toode)
        {
            Toode = toode;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(206, 340);
            Margin = new Padding(0, 0, 14, 14);
            BackColor = Color.White;

            // Internetipilt: kui see on juba mälus, näitame kohe; muidu laeme taustal
            bool onUrl = PildiLaadija.OnUrl(toode.Pilt);
            Image? laetud = onUrl ? PildiLaadija.VotaMalust(toode.Pilt) : LaePilt(toode.Pilt);
            bool laadiTaustal = onUrl && laetud == null;
            pildiOlek = laetud != null ? PildiOlek.Olemas
                : laadiTaustal ? PildiOlek.Laadimine
                : PildiOlek.Puudub;

            // Kui pilti pole, paneb RakendaKeel siia õiges keeles kohatäite
            pilt = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(PildiLaius, PildiKorgus),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Image = laetud
            };

            // Kaks rida nime jaoks, nagu kaup24 toodete loendis
            nimiLabel = new Label
            {
                Location = new Point(12, 172),
                Size = new Size(182, 40),
                Font = new Font(Teema.FondiNimi, 10F),
                ForeColor = Teema.Tekst,
                AutoEllipsis = true,
                Text = toode.Nimetus
            };

            kategooriaLabel = new Label
            {
                Location = new Point(12, 212),
                Size = new Size(182, 18),
                Font = Teema.Vaike,
                ForeColor = Teema.TekstHall,
                AutoEllipsis = true
            };

            hindLabel = new Label
            {
                Location = new Point(10, 232),
                Size = new Size(184, 34),
                Font = new Font(Teema.FondiNimi, 18F, FontStyle.Bold),
                ForeColor = Teema.Tekst,
                Text = $"{toode.Hind:0.00} €"
            };

            laosLabel = new Label
            {
                Location = new Point(12, 266),
                Size = new Size(182, 18),
                Font = new Font(Teema.FondiNimi, 8.5F, FontStyle.Bold)
            };

            kogusNum = new NumericUpDown
            {
                Location = new Point(12, 297),
                Size = new Size(52, 25),
                Font = new Font(Teema.FondiNimi, 10F),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center
            };

            lisaBtn = new TeemaNupp
            {
                Location = new Point(70, 292),
                Size = new Size(124, 36)
            };
            lisaBtn.Click += (_, _) => LisaKorvi?.Invoke(this, (int)kogusNum.Value);

            Controls.AddRange(new Control[] { pilt, nimiLabel, kategooriaLabel, hindLabel, laosLabel, kogusNum, lisaBtn });

            vihje.SetToolTip(nimiLabel, toode.Nimetus);

            SeoHover(this);
            foreach (Control c in Controls) SeoHover(c);

            SeaSaadaval(toode.Kogus);

            // Tekstid praeguses keeles; uuenevad ise, kui keel vahetub
            Keel.Jalgi(this, RakendaKeel);

            if (laadiTaustal)
                LaeUrlPilt(toode.Pilt!);
        }

        /// <summary>Uuendab, mitu tükki saab veel korvi lisada.</summary>
        public void SeaSaadaval(int saadaval)
        {
            saadaval = Math.Max(0, saadaval);
            this.saadaval = saadaval;

            kogusNum.Minimum = 0;
            kogusNum.Maximum = saadaval;
            kogusNum.Minimum = saadaval > 0 ? 1 : 0;
            kogusNum.Value = kogusNum.Minimum;

            bool saab = saadaval > 0;
            kogusNum.Enabled = saab;
            lisaBtn.Enabled = saab;

            UuendaLaoSilt();
        }

        // ---------- Keel ----------

        /// <summary>Kaardi tekstid praeguses keeles (toote nimi ja kategooria tulevad andmebaasist ja jäävad samaks).</summary>
        private void RakendaKeel()
        {
            kategooriaLabel.Text = string.IsNullOrWhiteSpace(Toode.Kategooria)
                ? Keel.T("Kategooriata", "Без категории", "No category")
                : Toode.Kategooria;
            lisaBtn.Text = Keel.T("Lisa korvi", "В корзину", "Add to cart");
            UuendaLaoSilt();
            UuendaPildiTekstid();
        }

        private void UuendaLaoSilt()
        {
            bool saab = saadaval > 0;

            if (Toode.Kogus <= 0)
                laosLabel.Text = Keel.T("✕ Otsas", "✕ Нет в наличии", "✕ Out of stock");
            else if (saab)
                laosLabel.Text = Keel.T($"✓ Laos {saadaval} tk", $"✓ В наличии: {saadaval} шт.", $"✓ In stock: {saadaval} pcs");
            else
                laosLabel.Text = Keel.T("Kõik on juba korvis", "Всё уже в корзине", "All already in cart");

            laosLabel.ForeColor = saab ? Teema.Edu : Teema.Viga;
        }

        /// <summary>Kohatäide ja vihje, kui pilti (veel) ei ole.</summary>
        private void UuendaPildiTekstid()
        {
            switch (pildiOlek)
            {
                case PildiOlek.Laadimine:
                    pilt.Image = Kohataide(true);
                    vihje.SetToolTip(pilt, null);
                    break;

                // Kui pilti ei leitud, näitab hiire all, mis väärtus andmebaasis on (aitab vigu leida)
                case PildiOlek.Puudub:
                    pilt.Image = Kohataide(false);
                    vihje.SetToolTip(pilt, string.IsNullOrWhiteSpace(Toode.Pilt)
                        ? Keel.T("Andmebaasis pole pilti määratud",
                                 "В базе данных изображение не указано",
                                 "No image is set in the database")
                        : Keel.T($"Pilti ei leitud: \"{Toode.Pilt}\"\nKäivituskaust: {Application.StartupPath}",
                                 $"Изображение не найдено: \"{Toode.Pilt}\"\nПапка запуска: {Application.StartupPath}",
                                 $"Image not found: \"{Toode.Pilt}\"\nStartup folder: {Application.StartupPath}"));
                    break;

                case PildiOlek.LaadimineEbaonnestus:
                    pilt.Image = Kohataide(false);
                    vihje.SetToolTip(pilt, Keel.T($"Pilti ei õnnestunud laadida:\n{Toode.Pilt}",
                                                  $"Не удалось загрузить изображение:\n{Toode.Pilt}",
                                                  $"Failed to load the image:\n{Toode.Pilt}"));
                    break;
            }
        }

        /// <summary>Ühine kohatäide praeguses keeles (luuakse esimesel kasutamisel).</summary>
        private static Image Kohataide(bool laadimine)
        {
            var voti = (Keel.Praegune, laadimine);
            if (!kohataited.TryGetValue(voti, out var kohataide))
            {
                kohataide = LooKohataide(laadimine
                    ? Keel.T("Laadin pilti...", "Загрузка изображения...", "Loading image...")
                    : Keel.T("Pilt puudub", "Нет изображения", "No image"));
                kohataited[voti] = kohataide;
            }
            return kohataide;
        }

        private static bool OnKohataide(Image? kontrollitav) =>
            kontrollitav != null && kohataited.ContainsValue(kontrollitav);

        // ---------- Välimus ----------

        private void SeoHover(Control c)
        {
            c.MouseEnter += (_, _) => SeaHover(true);
            c.MouseLeave += (_, _) => SeaHover(ClientRectangle.Contains(PointToClient(Cursor.Position)));
        }

        private void SeaHover(bool uus)
        {
            if (hover == uus) return;
            hover = uus;
            Invalidate();
        }

        // Valge ümar kaart; hiire all tumedam ääris ja oranž joon all (kaup24 "hover" efekt)
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Teema.VanemaTaust(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var tee = Teema.Umar(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 10);
            g.FillPath(Brushes.White, tee);
            using var pliiats = new Pen(hover ? Teema.PiirTume : Teema.Piir, 1f);
            g.DrawPath(pliiats, tee);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!hover) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var joon = new Pen(Teema.Aktsent, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            e.Graphics.DrawLine(joon, 14, 2, Width - 14, 2);
        }


        // ---------- Pildid ----------

        /// <summary>
        /// Laeb pildi internetist taustal (vahemälu kaudu), et pood avaneks kohe.
        /// await jätkab UI lõimel, seega võib pildi otse PictureBox-i panna.
        /// </summary>
        private async void LaeUrlPilt(string url)
        {
            Bitmap? bmp = null;
            try
            {
                bmp = await PildiLaadija.LaeUrlistAsync(url);
            }
            catch
            {
                bmp = null;
            }

            // Kaart võis vahepeal sulgeda (nt uus otsing või aken suleti)
            if (IsDisposed || Disposing || pilt.IsDisposed)
            {
                bmp?.Dispose();
                return;
            }

            if (InvokeRequired)
            {
                // Igaks juhuks, kui jätkati teisel lõimel
                try { BeginInvoke(() => SeaUrlPilt(bmp)); }
                catch { bmp?.Dispose(); }
                return;
            }

            SeaUrlPilt(bmp);
        }

        private void SeaUrlPilt(Bitmap? bmp)
        {
            if (IsDisposed || Disposing || pilt.IsDisposed)
            {
                bmp?.Dispose();
                return;
            }

            var vana = pilt.Image;
            if (bmp != null)
            {
                pildiOlek = PildiOlek.Olemas;
                pilt.Image = bmp;
                vihje.SetToolTip(pilt, null);
            }
            else
            {
                pildiOlek = PildiOlek.LaadimineEbaonnestus;
                UuendaPildiTekstid();
            }

            if (vana != null && !OnKohataide(vana) && vana != bmp)
                vana.Dispose();
        }

        private static Image? LaePilt(string? fail)
        {
            if (string.IsNullOrWhiteSpace(fail)) return null;

            string? tee = LeiaPildiFail(fail);
            if (tee == null) return null;

            // Kopeerime pildi mällu, et fail ei jääks lukku
            return PildiLaadija.LaeFailist(tee);
        }

        /// <summary>Otsib pildifaili mitmest kohast ja tagastab esimese olemasoleva tee.</summary>
        private static string? LeiaPildiFail(string fail)
        {
            fail = fail.Trim().Trim('"');

            // 1) Täielik tee, mis on olemas
            if (Path.IsPathRooted(fail) && File.Exists(fail)) return fail;

            // Kui on suhteline tee (nt "Pildid\piim.jpg"), proovime seda tervena,
            // ja lisaks ainult failinime (kui pilt on hiljem teise kausta tõstetud)
            string suhteline = Path.IsPathRooted(fail) ? Path.GetFileName(fail) : fail;
            string ainultNimi = Path.GetFileName(fail);
            var nimed = new List<string> { suhteline };
            if (ainultNimi != suhteline) nimed.Add(ainultNimi);

            // Kui laiendit pole, proovime levinud laiendeid
            if (!Path.HasExtension(ainultNimi))
            {
                foreach (var n in nimed.ToList())
                    foreach (var l in Laiendid)
                        nimed.Add(n + l);
            }

            // 2) Kaustad, kust otsida
            var kaustad = new List<string>();
            if (!string.IsNullOrWhiteSpace(PiltideKaust)) kaustad.Add(PiltideKaust);

            // Käivituskaust (bin\Debug\net8.0-windows) ja kuni 5 ülemkausta (jõuab projekti kaustani)
            var dir = new DirectoryInfo(Application.StartupPath);
            for (int i = 0; i < 5 && dir != null; i++, dir = dir.Parent)
            {
                kaustad.Add(dir.FullName);
                foreach (var k in KaustaNimed)
                    kaustad.Add(Path.Combine(dir.FullName, k));
            }

            foreach (var kaust in kaustad)
                foreach (var nimi in nimed)
                {
                    string tee = Path.Combine(kaust, nimi);
                    if (File.Exists(tee)) return tee;
                }

            return null;
        }

        private static Image LooKohataide(string tekst)
        {
            var bmp = new Bitmap(PildiLaius, PildiKorgus);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Teema.Taust);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var f = new Font("Segoe UI", 9F, FontStyle.Italic);
            using var pintsel = new SolidBrush(Color.Gray);
            using var formaat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(tekst, f, pintsel, new RectangleF(0, 0, PildiLaius, PildiKorgus), formaat);
            return bmp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var vana = pilt.Image;
                if (vana != null && !OnKohataide(vana))
                {
                    pilt.Image = null;
                    vana.Dispose();
                }
                vihje.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}