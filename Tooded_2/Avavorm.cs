using Microsoft.EntityFrameworkCore;

namespace Tooded_2
{
    public partial class Avavorm : Form
    {
        Plaat poodPlaat, tooPlaat, adminPlaat;
        TeemaNupp logiValjaBtn, kontoBtn;
        Label tervitusLabel, rahaLabel;
        Label bannerTekst, bannerAlam, pealkiri;

        // Plaadi suurus (vene ja inglise kirjeldused on eesti omast pikemad)
        private const int PlaadiLaius = 250, PlaadiKorgus = 210;

        private readonly AppDbContext _context = new AppDbContext();

        public Avavorm()
        {
            InitializeComponent();
            Teema.Vorm(this, new Size(960, 460));
            StartPosition = FormStartPosition.CenterScreen;

            // ---------- Päis: logo | tervitus, keelevalik, raha, logi välja ----------
            var pais = Teema.LooPais(this);

            logiValjaBtn = Teema.Nupp("", NupuStiil.Pais);
            logiValjaBtn.Size = new Size(110, 36);
            logiValjaBtn.Location = new Point(pais.Width - logiValjaBtn.Width - 16, 14);
            logiValjaBtn.Click += LogiValja_Click;
            pais.Controls.Add(logiValjaBtn);

            kontoBtn = Teema.Nupp("", NupuStiil.Pais);
            kontoBtn.Size = new Size(120, 36);
            kontoBtn.Location = new Point(logiValjaBtn.Left - kontoBtn.Width - 4, 14);
            kontoBtn.Click += Konto_Click;
            pais.Controls.Add(kontoBtn);

            rahaLabel = new PillSilt
            {
                Size = new Size(120, 32),
                Location = new Point(kontoBtn.Left - 132, 16)
            };
            pais.Controls.Add(rahaLabel);

            var keeleValik = new KeeleValik();
            keeleValik.Location = new Point(rahaLabel.Left - keeleValik.Width - 12, 16);
            pais.Controls.Add(keeleValik);

            // Tervitus täidab logo ja keelevaliku vahele jääva ruumi
            const int tervitusVasak = 216;
            tervitusLabel = new Label
            {
                Font = Teema.Tavaline,
                ForeColor = Color.FromArgb(210, 220, 235),
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true,
                Size = new Size(keeleValik.Left - 12 - tervitusVasak, 32),
                Location = new Point(tervitusVasak, 16)
            };
            pais.Controls.Add(tervitusLabel);
            Controls.Add(pais);

            // ---------- Reklaamiriba (nagu kaup24 kampaaniabänner) ----------
            var banner = new KaartPaneel
            {
                BackColor = Teema.AktsentHele,
                PiirVarv = Color.FromArgb(255, 214, 186),
                Location = new Point(30, 84),
                Size = new Size(ClientSize.Width - 60, 64)
            };
            bannerTekst = Teema.Silt("", new Font(Teema.FondiNimi, 13F, FontStyle.Bold), Teema.AktsentHover);
            bannerTekst.Location = new Point(20, 10);
            bannerAlam = Teema.Silt("", Teema.Tavaline, Teema.Tekst);
            bannerAlam.Location = new Point(22, 36);
            banner.Controls.Add(bannerTekst);
            banner.Controls.Add(bannerAlam);
            Controls.Add(banner);

            pealkiri = Teema.Silt("", Teema.Pealkiri);
            pealkiri.Location = new Point(28, 166);
            Controls.Add(pealkiri);

            // ---------- Plaadid ----------
            poodPlaat = LooPlaat("🛒", Pood_Click);
            tooPlaat = LooPlaat("💼", Too_Click);
            adminPlaat = LooPlaat("⚙", AdminPanel_Click);

            // Ostja näeb ainult poodi (ja tööd, et poe jaoks raha teenida).
            // Admin pääseb igale poole.
            adminPlaat.Visible = Session.IsAdmin;
            var plaadid = Session.IsAdmin
                ? new[] { poodPlaat, tooPlaat, adminPlaat }
                : new[] { poodPlaat, tooPlaat };

            const int vahe = 20;
            int x = (ClientSize.Width - (plaadid.Length * PlaadiLaius + (plaadid.Length - 1) * vahe)) / 2;
            foreach (var p in plaadid)
            {
                p.Location = new Point(x, 204);
                x += PlaadiLaius + vahe;
            }

            // Raha värskendamine, kui tullakse tagasi teisest aknast
            Activated += (s, e) => UuendaRaha();

            // Tekstid valitud keeles (ja uuesti iga kord, kui keelt vahetatakse)
            Keel.Jalgi(this, RakendaKeel);
        }

        private Plaat LooPlaat(string ikoon, EventHandler klikk)
        {
            var plaat = new Plaat
            {
                Ikoon = ikoon,
                Size = new Size(PlaadiLaius, PlaadiKorgus)
            };
            plaat.Click += klikk;
            Controls.Add(plaat);
            return plaat;
        }

        // ---------- Keel ----------

        /// <summary>Kõik vormi staatilised tekstid praeguses keeles.</summary>
        private void RakendaKeel()
        {
            // Kasutajanimi võib konto aknas muutuda – pealkiri kirjutatakse siin uuesti
            Text = $"{TsekiKoostaja.PoeNimi} – {Session.Username} ({Session.Role})";
            kontoBtn.Text = Keel.T("👤 Konto", "👤 Аккаунт", "👤 Account");
            logiValjaBtn.Text = Keel.T("Logi välja", "Выйти", "Log out");
            tervitusLabel.Text = Keel.T(
                $"Tere, {Session.Username}!",
                $"Привет, {Session.Username}!",
                $"Hello, {Session.Username}!");

            bannerTekst.Text = Keel.T("🔥 TOP diilid iga päev", "🔥 ТОП-предложения каждый день", "🔥 TOP deals every day");
            bannerAlam.Text = Keel.T(
                "Teeni raha lehel „Töö“ ja osta poest parimate hindadega.",
                "Зарабатывай деньги в разделе «Работа» и покупай в магазине по лучшим ценам.",
                "Earn money on the “Work” page and shop at the best prices.");
            pealkiri.Text = Keel.T("Mida soovid täna teha?", "Чем хочешь заняться сегодня?", "What would you like to do today?");

            SeaPlaat(poodPlaat,
                Keel.T("Pood", "Магазин", "Shop"),
                Keel.T("Sirvi tooteid ja lisa need ostukorvi",
                       "Просматривай товары и добавляй их в корзину",
                       "Browse products and add them to your cart"));
            SeaPlaat(tooPlaat,
                Keel.T("Töö", "Работа", "Work"),
                Keel.T("Klõpsa, teeni raha ja palka abilisi",
                       "Кликай, зарабатывай деньги и нанимай помощников",
                       "Click, earn money and hire helpers"));
            SeaPlaat(adminPlaat,
                Keel.T("Admin paneel", "Админ-панель", "Admin panel"),
                Keel.T("Halda tooteid, hindu ja kategooriaid",
                       "Управляй товарами, ценами и категориями",
                       "Manage products, prices and categories"));
        }

        private static void SeaPlaat(Plaat plaat, string tekst, string kirjeldus)
        {
            plaat.Text = tekst;
            plaat.Kirjeldus = kirjeldus;
            plaat.Invalidate(); // Kirjeldus on tavaline omadus, mis ise ümber ei joonista
        }

        private void UuendaRaha()
        {
            try
            {
                rahaLabel.Text = $"{Session.Raha():0.00} €";
            }
            catch
            {
                rahaLabel.Text = "? €";
            }
        }

        // ---------- Nupud ----------

        private void AdminPanel_Click(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show(Keel.T("Ligipääs keelatud.", "Доступ запрещён.", "Access denied."));
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

            var ostud = pood.Ostud;
            if (OstuTehing.TeeOst(this, ostud, pood.Kokku, out string email))
                _ = OstuTehing.SaadaTsekk(this, ostud, email); // taustal, aken ei jää ootama
            UuendaRaha();
        }

        private void Konto_Click(object? sender, EventArgs e)
        {
            using var konto = new Konto();
            konto.ShowDialog(this);

            if (konto.Kustutatud)
            {
                // Kontot enam pole – välja logimine ja tagasi sisselogimise aknasse
                Session.Logout();
                Application.Restart();
                return;
            }

            RakendaKeel(); // kasutajanimi võis muutuda (tervitus ja akna pealkiri)
        }

        private void LogiValja_Click(object? sender, EventArgs e)
        {
            Session.Logout();
            Application.Restart();
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
