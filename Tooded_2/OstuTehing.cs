namespace Tooded_2
{
    /// <summary>
    /// Ostu sooritamine: raha mahaarvamine, laoseisu vähendamine ning tšeki ja PDF-arve saatmine.
    /// Ühine pealehe (Avavorm) ja administraatori paneeli (AdminPanel) jaoks.
    /// </summary>
    public static class OstuTehing
    {
        // Teateakende pealkirjad
        private static string PoeTiitel() => Keel.T("Pood", "Магазин", "Shop");
        private static string EpostiTiitel() => Keel.T("E-post", "Эл. почта", "E-mail");

        /// <summary>
        /// Sooritab ostu ühe andmebaasi tehinguna: kas kõik muudatused (raha ja laoseis)
        /// salvestatakse koos või ei salvestata ühtegi. Näitab kasutajale tulemuse teate.
        /// </summary>
        /// <param name="email">Kasutaja registreeritud e-post, kuhu tšekk ja arve saata.</param>
        /// <returns>true, kui ost õnnestus.</returns>
        public static bool TeeOst(IWin32Window omanik, List<OstuRida> ostud, decimal kokku, out string email)
        {
            email = "";
            try
            {
                using var db = new AppDbContext();
                using var tx = db.Database.BeginTransaction();

                var kasutaja = Session.LaeKasutaja(db)
                               ?? throw new InvalidOperationException(
                                   Keel.T("Kasutajat ei leitud.", "Пользователь не найден.", "User not found."));
                email = (kasutaja.Email ?? "").Trim();

                // Kontrollime uuesti andmebaasi värskete andmetega – poe aknas nähtud raha võis vahepeal muutuda
                if (kasutaja.Raha < kokku)
                {
                    MessageBox.Show(omanik,
                        Keel.T("Teil ei ole piisavalt raha.", "У вас недостаточно денег.", "You don't have enough money."),
                        PoeTiitel(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Vähendame laoseisu
                foreach (var rida in ostud)
                {
                    var toode = db.Toodetabel.FirstOrDefault(t => t.Id == rida.ToodeId);
                    if (toode == null || toode.Kogus < rida.Kogus)
                    {
                        MessageBox.Show(omanik, Keel.T(
                                $"Toodet \"{rida.Nimetus}\" ei ole laos piisavalt. Ost tühistati.",
                                $"Товара «{rida.Nimetus}» недостаточно на складе. Покупка отменена.",
                                $"There is not enough of \"{rida.Nimetus}\" in stock. The purchase was cancelled."),
                            PoeTiitel(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false; // Commit jääb tegemata – tehing tühistatakse automaatselt
                    }
                    toode.Kogus -= rida.Kogus;
                }

                kasutaja.Raha -= kokku;

                db.SaveChanges();
                tx.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(omanik, Keel.T("Ost ebaõnnestus: ", "Покупка не удалась: ", "Purchase failed: ")
                        + (ex.InnerException?.Message ?? ex.Message),
                    Keel.T("Viga", "Ошибка", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MessageBox.Show(omanik, Keel.T(
                    $"Ost õnnestus! Makstud {kokku:0.00} €.",
                    $"Покупка прошла успешно! Оплачено {kokku:0.00} €.",
                    $"Purchase successful! Paid {kokku:0.00} €."), PoeTiitel(),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        /// <summary>
        /// Saadab ühe kirjaga HTML-tšeki (kirja sisu) ja PDF-arve (manus) poe postkastist.
        /// Ost on selleks ajaks juba kinnitatud – saatmise viga ostu ei tühista.
        /// Erindeid välja ei viska, seega võib kutsuda ka ootamata: <c>_ = SaadaTsekk(...)</c>.
        /// </summary>
        public static async Task SaadaTsekk(Form omanik, List<OstuRida> ostud, string email)
        {
            // Aadressi pole või see on vigane – poe aken juba ütles, et tšekki ei saadeta
            if (!EpostiSaatja.OnKorrektne(email)) return;

            try
            {
                var aeg = DateTime.Now;
                string tsekiNr = aeg.ToString("yyyyMMddHHmmss");
                string klient = Session.Username ?? "";
                string html = TsekiKoostaja.LooHtml(ostud, tsekiNr, aeg, klient);
                string teema = Keel.T(
                    $"{TsekiKoostaja.PoeNimi} – tšekk ja arve nr {tsekiNr}",
                    $"{TsekiKoostaja.PoeNimi} – чек и счёт № {tsekiNr}",
                    $"{TsekiKoostaja.PoeNimi} – receipt and invoice no. {tsekiNr}");

                // PDF-i koostamine on arvutusmahukas – teeme seda taustal, et aken ei hanguks
                byte[] pdf = await Task.Run(() => ArveKoostaja.LooPdf(ostud, tsekiNr, aeg, klient, email));

                await EpostiSaatja.SaadaAsync(email, teema, html, pdf, $"arve_{tsekiNr}.pdf");

                if (omanik.IsDisposed) return;
                MessageBox.Show(omanik, Keel.T(
                        $"Tšekk ja PDF-arve saadeti aadressile {email}.",
                        $"Чек и PDF-счёт отправлены на адрес {email}.",
                        $"The receipt and PDF invoice were sent to {email}."), EpostiTiitel(),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (omanik.IsDisposed) return;
                MessageBox.Show(omanik, Keel.T(
                        "Ost on tehtud, kuid tšeki ja arve saatmine ebaõnnestus: ",
                        "Покупка совершена, но отправить чек и счёт не удалось: ",
                        "The purchase is complete, but sending the receipt and invoice failed: ")
                    + ex.Message, EpostiTiitel(),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
