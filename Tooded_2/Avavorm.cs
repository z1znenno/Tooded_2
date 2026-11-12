using Microsoft.EntityFrameworkCore;

namespace Tooded_2
{
    public partial class Avavorm : Form
    {
        Button AdminPanel, Pood, Too, LogiValja;
        Label tervitusLabel, rahaLabel;

        public static Font font = new Font("Segoe Script", 9F, FontStyle.Bold);

        private readonly AppDbContext _context = new AppDbContext();

        public Avavorm()
        {
            InitializeComponent();
            ClientSize = new Size(330, 300);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Text = $"Avavorm – {Session.Username} ({Session.Role})";
            StartPosition = FormStartPosition.CenterScreen;
            Font = font;

            tervitusLabel = new Label
            {
                Text = $"Tere, {Session.Username}!",
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 10),
                Size = new Size(310, 25)
            };
            Controls.Add(tervitusLabel);

            rahaLabel = new Label
            {
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 125, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 35),
                Size = new Size(310, 30)
            };
            Controls.Add(rahaLabel);

            int y = 80;
            Pood = LooNupp("🛒 Pood", ref y, Pood_Click);
            Too = LooNupp("💼 Töö (teeni raha)", ref y, Too_Click);
            AdminPanel = LooNupp("⚙ Admin paneel", ref y, AdminPanel_Click);
            LogiValja = LooNupp("Logi välja", ref y, LogiValja_Click);

            // Ostja näeb ainult poodi (ja tööd, et poe jaoks raha teenida).
            // Admin pääseb igale poole.
            AdminPanel.Visible = Session.IsAdmin;
            if (!Session.IsAdmin)
                LogiValja.Top = AdminPanel.Top;

            // Raha värskendamine, kui tullakse tagasi teisest aknast
            Activated += (s, e) => UuendaRaha();
        }

        private Button LooNupp(string tekst, ref int y, EventHandler klikk)
        {
            var nupp = new Button
            {
                Text = tekst,
                Font = font,
                Width = 220,
                Height = 38,
                Location = new Point(55, y)
            };
            nupp.Click += klikk;
            Controls.Add(nupp);
            y += 45;
            return nupp;
        }

        private void UuendaRaha()
        {
            try
            {
                rahaLabel.Text = $"💰 {Session.Raha():0.00} €";
            }
            catch
            {
                rahaLabel.Text = "💰 ? €";
            }
        }

        // ---------- Nupud ----------

        private void AdminPanel_Click(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show("Ligipääs keelatud.");
                return;
            }
            new AdminPanel().Show();
        }

        private void Too_Click(object? sender, EventArgs e)
        {
            using var too = new Too();
            too.ShowDialog(this);
            UuendaRaha();
        }

        private void Pood_Click(object? sender, EventArgs e)
        {
            using var pood = new Pood(OtsiPoest);
            if (pood.ShowDialog(this) != DialogResult.OK) return;

            TeeOst(pood.Ostud, pood.Kokku, pood.Email);
            UuendaRaha();
        }

        private void LogiValja_Click(object? sender, EventArgs e)
        {
            Session.Logout();
            Application.Restart();
        }

        // ---------- Ostu sooritamine ----------

        private void TeeOst(List<OstuRida> ostud, decimal kokku, string email)
        {
            using var db = new AppDbContext();
            using var tx = db.Database.BeginTransaction();
            try
            {
                var kasutaja = Session.LaeKasutaja(db)
                               ?? throw new InvalidOperationException("Kasutajat ei leitud.");

                if (kasutaja.Raha < kokku)
                {
                    MessageBox.Show("Sul ei ole piisavalt raha.", "Pood");
                    return;
                }

                // Vähendame laoseisu
                foreach (var rida in ostud)
                {
                    var toode = db.Toodetabel.FirstOrDefault(t => t.Id == rida.ToodeId);
                    if (toode == null || toode.Kogus < rida.Kogus)
                    {
                        MessageBox.Show($"Toodet \"{rida.Nimetus}\" ei ole laos piisavalt.", "Pood");
                        return; // tehing tühistatakse automaatselt
                    }
                    toode.Kogus -= rida.Kogus;
                }

                kasutaja.Raha -= kokku;

                db.SaveChanges();
                tx.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ost ebaõnnestus: " + (ex.InnerException?.Message ?? ex.Message),
                    "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Ost õnnestus! Makstud {kokku:0.00} €.", "Pood",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            _ = SaadaTsekk(ostud, email);
        }

        private async Task SaadaTsekk(List<OstuRida> ostud, string email)
        {
            try
            {
                var aeg = DateTime.Now;
                string tsekiNr = aeg.ToString("yyyyMMddHHmmss");
                string html = TsekiKoostaja.LooHtml(ostud, tsekiNr, aeg, Session.Username ?? "");
                string teema = $"{TsekiKoostaja.PoeNimi} – tšekk nr {tsekiNr}";

                // Kui kasutajal on oma rakenduse parool, saadame tema e-postilt,
                // muidu poe üldiselt aadressilt.
                string saatja = "", parool = "";
                using (var db = new AppDbContext())
                {
                    var kasutaja = Session.LaeKasutaja(db);
                    if (kasutaja != null && !string.IsNullOrEmpty(kasutaja.EpostiParool))
                    {
                        saatja = kasutaja.Email;
                        parool = ParooliKaitse.Dekrupteeri(kasutaja.EpostiParool);
                    }
                }

                if (parool.Length > 0)
                    await EpostiSaatja.SaadaAsync(email, teema, html, saatja, parool);
                else
                    await EpostiSaatja.SaadaAsync(email, teema, html);

                MessageBox.Show($"Tšekk saadeti aadressile {email}.", "E-post",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tšeki saatmine ebaõnnestus: " + ex.Message, "E-post",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------- Toodete otsing poe jaoks ----------

        private List<PoodToode> OtsiPoest(string otsing)
        {
            var paring = _context.Toodetabel.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(otsing))
            {
                paring = paring.Where(t =>
                    t.Toodenimetus.Contains(otsing) ||
                    t.Kategooria.nimetus.Contains(otsing));
            }

            return paring
                .OrderBy(t => t.Toodenimetus)
                .Select(t => new PoodToode
                {
                    Id = t.Id,
                    Nimetus = t.Toodenimetus,
                    Kogus = (int)t.Kogus,
                    Hind = (decimal)t.Hind,
                    Kategooria = t.Kategooria.nimetus,
                    Pilt = t.Pilt
                })
                .ToList();
        }
    }
}
