using System.ComponentModel;

namespace Tooded_2
{
    public partial class Pood : Form
    {
        // Funktsioon, mis otsib tooteid andmebaasist (antakse Form2-st)
        private readonly Func<string, List<PoodToode>> otsiTooted;

        private readonly BindingList<OstuRida> korv = new();
        private readonly Dictionary<int, ToodeKaart> kaardid = new();

        /// <summary>Mida osteti (loe pärast DialogResult.OK).</summary>
        public List<OstuRida> Ostud => korv.ToList();

        /// <summary>Lõppsumma.</summary>
        public decimal Kokku => korv.Sum(r => r.Summa);

        public Pood(Func<string, List<PoodToode>> otsiTooted)
        {
            InitializeComponent();
            this.otsiTooted = otsiTooted;
            korvGrid.DataSource = korv;

            rahakott = Session.Raha();

            // Tšekk ja PDF-arve saadetakse alati registreerimisel antud e-postile
            using (var db = new AppDbContext())
                kasutajaEmail = (Session.LaeKasutaja(db)?.Email ?? "").Trim();

            // Admin Panel nupp ainult administraatorile
            Administrator.Visible = Session.IsAdmin;
            Administrator.Click += Administrator_Click;

            Kujunda();
            UuendaKokku();
        }

        // Kasutaja raha (teenitud klikkermängus "Töö")
        private readonly decimal rahakott;

        // Kasutaja registreeritud e-post (laetakse üks kord akna avamisel)
        private readonly string kasutajaEmail;

        /// <summary>Kas kontol on korrektne e-posti aadress, kuhu tšekk ja arve saata.</summary>
        private bool EpostOlemas => EpostiSaatja.OnKorrektne(kasutajaEmail);

        private void Administrator_Click(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin) return;
            new AdminPanel().Show();
        }

        // ---------- Toodete laadimine ----------

        private void Form3_Load(object? sender, EventArgs e) => LaeTooted("");

        private void LaeTooted(string otsing)
        {
            List<PoodToode> tooted;
            try
            {
                Cursor = Cursors.WaitCursor;
                tooted = otsiTooted(otsing);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Keel.T("Viga toodete laadimisel:", "Ошибка при загрузке товаров:", "Error loading products:")
                    + "\n" + ex.Message, PoeTekst(),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            tootedPanel.SuspendLayout();

            // Vanad kaardid ära (Dispose vabastab ka pildid)
            foreach (Control c in tootedPanel.Controls.Cast<Control>().ToList())
                c.Dispose();
            tootedPanel.Controls.Clear();
            kaardid.Clear();

            foreach (var toode in tooted)
            {
                var kaart = new ToodeKaart(toode);
                kaart.SeaSaadaval(Saadaval(toode));
                kaart.LisaKorvi += Kaart_LisaKorvi;
                tootedPanel.Controls.Add(kaart);
                kaardid[toode.Id] = kaart;
            }

            tootedPanel.ResumeLayout();

            viimaneOtsing = otsing;
            leitudArv = tooted.Count;
            UuendaLeitud();

            if (string.IsNullOrEmpty(otsing) && !kategooriadLoodud)
                LooKategooriad(tooted);
            MargiKategooria(otsing);
        }

        // Viimase otsingu tulemus – et pealkirja saaks keele vahetumisel uuesti kirjutada
        private string viimaneOtsing = "";
        private int? leitudArv;

        /// <summary>Toodete ala pealkiri: mida otsiti ja mitu toodet leiti.</summary>
        private void UuendaLeitud()
        {
            if (leitudArv is not int arv)
            {
                leitudLabel.Text = "";
                return;
            }

            string pealkiri = string.IsNullOrEmpty(viimaneOtsing)
                ? KoikTootedTekst()
                : Keel.T($"Otsing: „{viimaneOtsing}“", $"Поиск: «{viimaneOtsing}»", $"Search: “{viimaneOtsing}”");

            leitudLabel.Text = arv == 0
                ? pealkiri + "  ·  " + Keel.T("midagi ei leitud", "ничего не найдено", "nothing found")
                : pealkiri + "  ·  " + Keel.T($"{arv} toodet", $"{arv} {VeneTooteid(arv)}",
                    arv == 1 ? "1 product" : $"{arv} products");
        }

        /// <summary>Vene keeles sõltub sõna kuju arvust: 1 товар, 2 товара, 5 товаров.</summary>
        private static string VeneTooteid(int arv)
        {
            int viimane = arv % 10, kaks = arv % 100;
            if (viimane == 1 && kaks != 11) return "товар";
            if (viimane >= 2 && viimane <= 4 && (kaks < 12 || kaks > 14)) return "товара";
            return "товаров";
        }

        private static string PoeTekst() => Keel.T("Pood", "Магазин", "Shop");

        private static string KoikTootedTekst() => Keel.T("Kõik tooted", "Все товары", "All products");

        // ---------- Kujundus (kaup24 stiilis) ----------

        private Label rahakottLabel = null!;
        private FlowLayoutPanel kategooriadPanel = null!;
        private bool kategooriadLoodud;

        // Juhtelemendid, mille tekst sõltub keelest (vt RakendaKeel)
        private TeemaNupp otsiBtn = null!;
        private Label kokkuPealkiri = null!;
        private ToolTip vihje = null!;
        private Panel alumine = null!; // ostukorvi alumine osa (kõrgus sõltub e-posti info pikkusest)
        private TeemaNupp? koikNupp; // "Kõik tooted" kategooriate ribal

        private void Kujunda()
        {
            Teema.Vorm(this, new Size(1200, 720), muudetav: true);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(1000, 640);
            Controls.Clear();

            // ----- Päis: logo, otsingukast, otsi, admin, keelevalik, rahakott -----
            // Parem pool on ankurdatud paremale; otsingukast venib nende vahel koos aknaga
            var pais = Teema.LooPais(this, korgus: 72);

            rahakottLabel = new PillSilt
            {
                Text = $"{rahakott:0.00} €",
                Size = new Size(120, 32),
                Location = new Point(pais.Width - 136, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            vihje = new ToolTip(components);
            pais.Controls.Add(rahakottLabel);

            var keeleValik = new KeeleValik
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            keeleValik.Location = new Point(rahakottLabel.Left - 12 - keeleValik.Width, 20);
            pais.Controls.Add(keeleValik);

            Administrator.Stiil = NupuStiil.Pais;
            Administrator.SetBounds(keeleValik.Left - 122, 16, 110, 40);
            Administrator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pais.Controls.Add(Administrator);

            otsiBtn = Teema.Nupp("");
            otsiBtn.SetBounds(Administrator.Left - 112, 14, 100, 44);
            otsiBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            otsiBtn.Click += (s, e) =>
            {
                otsiTimer.Stop();
                LaeTooted(otsiEntry.Text.Trim());
            };
            pais.Controls.Add(otsiBtn);

            var otsiKast = new KaartPaneel
            {
                Location = new Point(240, 14),
                Size = new Size(otsiBtn.Left - 8 - 240, 44),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Raadius = 8,
                PiirVarv = Color.Empty,
                Padding = new Padding(0)
            };
            otsiEntry.BorderStyle = BorderStyle.None;
            otsiEntry.Font = new Font(Teema.FondiNimi, 11.5F);
            otsiEntry.ForeColor = Teema.Tekst;
            otsiEntry.BackColor = Color.White;
            otsiEntry.SetBounds(16, 11, otsiKast.Width - 30, 24);
            otsiEntry.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            otsiKast.Controls.Add(otsiEntry);
            pais.Controls.Add(otsiKast);

            foreach (var nupp in new[] { eemaldaBtn, tuhistaBtn, Administrator })
                nupp.Font = Teema.Paks;

            // ----- Kategooriate riba -----
            var nav = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Teema.PaisTume };
            kategooriadPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(12, 5, 0, 0),
                BackColor = Teema.PaisTume
            };
            nav.Controls.Add(kategooriadPanel);

            // ----- Ostukorv paremal -----
            var parem = new Panel { Dock = DockStyle.Right, Width = 380, Padding = new Padding(0, 16, 16, 16) };
            var korvKaart = new KaartPaneel { Dock = DockStyle.Fill };

            korvLabel.AutoSize = false;
            korvLabel.Dock = DockStyle.Top;
            korvLabel.Height = 40;
            korvLabel.Font = Teema.Pealkiri;
            korvLabel.ForeColor = Teema.Tekst;

            Teema.Tabel(korvGrid);
            korvGrid.Dock = DockStyle.Fill;

            alumine = new Panel { Dock = DockStyle.Bottom, Height = 258 };

            eemaldaBtn.Stiil = NupuStiil.Oht;
            eemaldaBtn.SetBounds(0, 10, 220, 34); // vene keeles on tekst pikem

            var joon = new Panel { BackColor = Teema.Piir, Bounds = new Rectangle(0, 56, 332, 1) };

            kokkuPealkiri = Teema.Silt("", new Font(Teema.FondiNimi, 12F, FontStyle.Bold));
            kokkuPealkiri.Location = new Point(0, 70);

            kokkuLabel.AutoSize = false;
            kokkuLabel.Font = new Font(Teema.FondiNimi, 18F, FontStyle.Bold);
            kokkuLabel.TextAlign = ContentAlignment.MiddleRight;
            kokkuLabel.SetBounds(120, 62, 212, 36);

            rahakottInfo.Font = Teema.Vaike;
            rahakottInfo.ForeColor = Teema.TekstHall;
            rahakottInfo.TextAlign = ContentAlignment.MiddleRight;
            rahakottInfo.SetBounds(0, 98, 332, 20);

            // Info: kuhu tšekk ja arve saadetakse. Tekst murdub ridadesse; kõrguse ja
            // nuppude asukoha arvutab PaigutaEpostInfo (sõltub keelest ja aadressi pikkusest)
            epostLabel.AutoSize = false;
            epostLabel.AutoEllipsis = true;
            epostLabel.UseMnemonic = false; // aadressis olev & ei tohi kaduda
            epostLabel.Font = Teema.Vaike;
            epostLabel.ForeColor = Teema.TekstHall;
            epostLabel.SetBounds(0, 126, 332, 32);

            maksaBtn.Font = new Font(Teema.FondiNimi, 12F, FontStyle.Bold);
            maksaBtn.SetBounds(0, 168, 332, 46);

            tuhistaBtn.Stiil = NupuStiil.Teine;
            tuhistaBtn.SetBounds(0, 220, 332, 34);

            alumine.Controls.AddRange(new Control[]
            {
                eemaldaBtn, joon, kokkuPealkiri, kokkuLabel, rahakottInfo, epostLabel, maksaBtn, tuhistaBtn
            });

            // Dock: esimesena lisatud Fill paigutatakse viimasena
            korvKaart.Controls.Add(korvGrid);
            korvKaart.Controls.Add(alumine);
            korvKaart.Controls.Add(korvLabel);
            parem.Controls.Add(korvKaart);

            // ----- Toodete ala -----
            var sisu = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 6, 8, 0) };
            leitudLabel.AutoSize = false;
            leitudLabel.Dock = DockStyle.Top;
            leitudLabel.Height = 40;
            leitudLabel.Font = Teema.Pealkiri;
            leitudLabel.ForeColor = Teema.Tekst;
            leitudLabel.TextAlign = ContentAlignment.MiddleLeft;

            tootedPanel.Dock = DockStyle.Fill;
            tootedPanel.BorderStyle = BorderStyle.None;
            tootedPanel.BackColor = Teema.Taust;
            tootedPanel.Padding = new Padding(0, 0, 0, 12);

            sisu.Controls.Add(tootedPanel);
            sisu.Controls.Add(leitudLabel);

            Controls.Add(sisu);
            Controls.Add(parem);
            Controls.Add(nav);
            Controls.Add(pais);

            // Tekstid praeguses keeles; käivitub uuesti iga kord, kui keel vahetub
            Keel.Jalgi(this, RakendaKeel);
        }

        private readonly Label rahakottInfo = new() { AutoSize = false };

        /// <summary>Kõik akna tekstid praeguses keeles. Tootekaardid uuendavad oma tekstid ise.</summary>
        private void RakendaKeel()
        {
            Text = $"{PoeTekst()} – {Session.Username}";

            otsiEntry.PlaceholderText = Keel.T("Otsi toodet nime või kategooria järgi...",
                "Поиск товара по названию или категории...", "Search products by name or category...");
            otsiBtn.Text = Keel.T("Otsi", "Найти", "Search");
            vihje.SetToolTip(rahakottLabel, Keel.T("Sinu rahakott", "Ваш кошелёк", "Your wallet"));
            Administrator.Text = Keel.T("⚙ Admin", "⚙ Админ", "⚙ Admin");

            eemaldaBtn.Text = Keel.T("Eemalda valitud", "Удалить выбранное", "Remove selected");
            kokkuPealkiri.Text = Keel.T("Kokku", "Итого", "Total");
            epostLabel.Text = EpostOlemas
                ? Keel.T($"Tšekk ja PDF-arve saadetakse aadressile:\n{kasutajaEmail}",
                         $"Чек и PDF-счёт будут отправлены на адрес:\n{kasutajaEmail}",
                         $"The receipt and PDF invoice will be sent to:\n{kasutajaEmail}")
                : Keel.T("Tšekki ja arvet ei saadeta – kontol puudub e-posti aadress.",
                         "Чек и счёт не будут отправлены – у учётной записи нет адреса e-mail.",
                         "No receipt or invoice will be sent – the account has no e-mail address.");
            PaigutaEpostInfo();
            maksaBtn.Text = Keel.T("Maksa", "Оплатить", "Pay");
            tuhistaBtn.Text = Keel.T("Jätka hiljem", "Продолжить позже", "Continue later");

            if (koikNupp != null)
            {
                koikNupp.Text = KoikTootedTekst();
                koikNupp.Width = TextRenderer.MeasureText(koikNupp.Text, koikNupp.Font).Width + 28;
            }

            TabeliPealkirjad();
            UuendaLeitud();
            UuendaKokku();
        }

        /// <summary>
        /// E-posti info kõrgus teksti järgi (kuni 3 rida, pikem lõpeb kolme punktiga)
        /// ning nupud kohe selle alla, et vahele ei jääks tühja auku.
        /// </summary>
        private void PaigutaEpostInfo()
        {
            const TextFormatFlags lipud = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
            int reaKorgus = TextRenderer.MeasureText("Ag", epostLabel.Font).Height;
            int vajalik = TextRenderer.MeasureText(epostLabel.Text, epostLabel.Font,
                new Size(epostLabel.Width, int.MaxValue), lipud).Height;

            epostLabel.Height = Math.Clamp(vajalik, reaKorgus, reaKorgus * 3) + 2;
            maksaBtn.Top = epostLabel.Bottom + 10;
            tuhistaBtn.Top = maksaBtn.Bottom + 6;
            alumine.Height = tuhistaBtn.Bottom + 4;

            // Kui tekst ei mahu ära, saab terve teksti lugeda vihjest
            vihje.SetToolTip(epostLabel, epostLabel.Text);
        }

        /// <summary>Ostukorvi tabeli veergude pealkirjad (veerud tekivad OstuRida omadustest).</summary>
        private void TabeliPealkirjad()
        {
            var pealkirjad = new Dictionary<string, string>
            {
                ["Nimetus"] = Keel.T("Toode", "Товар", "Product"),
                ["Kogus"] = Keel.T("Kogus", "Кол-во", "Qty"),
                ["Hind"] = Keel.T("Hind", "Цена", "Price"),
                ["Summa"] = Keel.T("Summa", "Сумма", "Total")
            };

            foreach (var (nimi, tekst) in pealkirjad)
                if (korvGrid.Columns.Contains(nimi))
                    korvGrid.Columns[nimi]!.HeaderText = tekst;
        }

        /// <summary>Kategooriate nupud riba peal (nagu kaup24 menüü). Klikk otsib selle kategooria järgi.</summary>
        private void LooKategooriad(List<PoodToode> tooted)
        {
            kategooriadLoodud = true;
            var nimed = tooted
                .Select(t => t.Kategooria)
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(k => k)
                .ToList();

            // "Kõik tooted" on kasutajaliidese tekst (tõlgitakse); otsing on tühi, seega võrdlus ei sõltu keelest
            koikNupp = LooKategooriaNupp(KoikTootedTekst(), "");
            kategooriadPanel.Controls.Add(koikNupp);
            foreach (var nimi in nimed)
                kategooriadPanel.Controls.Add(LooKategooriaNupp(nimi!, nimi!));
        }

        private TeemaNupp LooKategooriaNupp(string tekst, string otsing)
        {
            var nupp = Teema.Nupp(tekst, NupuStiil.Pais);
            nupp.Tag = otsing;
            nupp.Height = 32;
            nupp.Width = TextRenderer.MeasureText(tekst, nupp.Font).Width + 28;
            nupp.Margin = new Padding(2, 0, 2, 0);
            nupp.Click += (s, e) =>
            {
                otsiEntry.Text = otsing;
                otsiTimer.Stop();
                LaeTooted(otsing);
            };
            return nupp;
        }

        /// <summary>Värvib oranžiks selle kategooria nupu, mis vastab otsingule.</summary>
        private void MargiKategooria(string otsing)
        {
            foreach (var nupp in kategooriadPanel.Controls.OfType<TeemaNupp>())
                nupp.Stiil = string.Equals(nupp.Tag as string, otsing, StringComparison.OrdinalIgnoreCase)
                    ? NupuStiil.Pohi
                    : NupuStiil.Pais;
        }

        /// <summary>Laos olev kogus miinus see, mis on juba korvis.</summary>
        private int Saadaval(PoodToode toode) =>
            toode.Kogus - korv.Where(r => r.ToodeId == toode.Id).Sum(r => r.Kogus);

        // ---------- Otsing (ootab 300 ms pärast viimast klahvivajutust) ----------

        private void otsiEntry_TextChanged(object? sender, EventArgs e)
        {
            otsiTimer.Stop();
            otsiTimer.Start();
        }

        private void otsiTimer_Tick(object? sender, EventArgs e)
        {
            otsiTimer.Stop();
            LaeTooted(otsiEntry.Text.Trim());
        }

        // ---------- Ostukorv ----------

        private void Kaart_LisaKorvi(object? sender, int kogus)
        {
            if (sender is not ToodeKaart kaart || kogus <= 0) return;

            var toode = kaart.Toode;
            kogus = Math.Min(kogus, Saadaval(toode));
            if (kogus <= 0) return;

            var olemas = korv.FirstOrDefault(r => r.ToodeId == toode.Id);
            if (olemas != null)
                olemas.Kogus += kogus;
            else
                korv.Add(new OstuRida
                {
                    ToodeId = toode.Id,
                    Nimetus = toode.Nimetus,
                    Kogus = kogus,
                    Hind = toode.Hind,
                    Pilt = toode.Pilt
                });

            korv.ResetBindings();
            UuendaKokku();
            kaart.SeaSaadaval(Saadaval(toode));
        }

        private void eemaldaBtn_Click(object? sender, EventArgs e)
        {
            if (korvGrid.CurrentRow?.DataBoundItem is not OstuRida rida) return;

            korv.Remove(rida);
            UuendaKokku();

            if (kaardid.TryGetValue(rida.ToodeId, out var kaart))
                kaart.SeaSaadaval(Saadaval(kaart.Toode));
        }

        private void UuendaKokku()
        {
            kokkuLabel.Text = $"{Kokku:0.00} €";
            kokkuLabel.ForeColor = Kokku > rahakott ? Teema.Viga : Teema.Tekst;
            int tk = korv.Sum(r => r.Kogus);
            string korvTekst = Keel.T("Ostukorv", "Корзина", "Cart");
            korvLabel.Text = tk > 0 ? $"{korvTekst} ({tk})" : korvTekst;
            rahakottInfo.Text = Kokku > rahakott
                ? Keel.T($"Raha ei jätku! Sinu rahakott: {rahakott:0.00} €",
                         $"Не хватает денег! Кошелёк: {rahakott:0.00} €",
                         $"Not enough money! Your wallet: {rahakott:0.00} €")
                : Keel.T($"Sinu rahakott: {rahakott:0.00} €",
                         $"Ваш кошелёк: {rahakott:0.00} €",
                         $"Your wallet: {rahakott:0.00} €");
            rahakottInfo.ForeColor = Kokku > rahakott ? Teema.Viga : Teema.TekstHall;
            maksaBtn.Enabled = korv.Count > 0;
            eemaldaBtn.Enabled = korv.Count > 0;
        }

        private void korvGrid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (var nimi in new[] { "Hind", "Summa" })
                if (korvGrid.Columns.Contains(nimi))
                    korvGrid.Columns[nimi]!.DefaultCellStyle.Format = "0.00";

            TabeliPealkirjad();
        }

        private void maksaBtn_Click(object? sender, EventArgs e)
        {
            if (korv.Count == 0)
            {
                MessageBox.Show(Keel.T("Ostukorv on tühi.", "Корзина пуста.", "The cart is empty."), PoeTekst());
                return;
            }

            if (Kokku > rahakott)
            {
                MessageBox.Show(Keel.T(
                        $"Sul ei ole piisavalt raha.\nVaja: {Kokku:0.00} €, sul on: {rahakott:0.00} €.\nMine tööle ja teeni juurde!",
                        $"У вас недостаточно денег.\nНужно: {Kokku:0.00} €, у вас есть: {rahakott:0.00} €.\nИдите на работу и заработайте ещё!",
                        $"You don't have enough money.\nNeeded: {Kokku:0.00} €, you have: {rahakott:0.00} €.\nGo to work and earn some more!"),
                    PoeTekst(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // E-posti puudumine ostu ei takista – siis lihtsalt tšekki ja arvet ei saadeta
            string epostiInfo = EpostOlemas
                ? Keel.T($"Tšekk ja PDF-arve saadetakse aadressile {kasutajaEmail}",
                         $"Чек и PDF-счёт будут отправлены на адрес {kasutajaEmail}",
                         $"The receipt and PDF invoice will be sent to {kasutajaEmail}")
                : Keel.T("Tšekki ja arvet ei saadeta, sest kontol puudub e-posti aadress.",
                         "Чек и счёт не будут отправлены, так как у учётной записи нет адреса e-mail.",
                         "No receipt or invoice will be sent because the account has no e-mail address.");

            var vastus = MessageBox.Show(
                Keel.T($"Kas maksta {Kokku:0.00} €?", $"Оплатить {Kokku:0.00} €?", $"Pay {Kokku:0.00} €?")
                    + "\n" + epostiInfo,
                Keel.T("Maksmine", "Оплата", "Payment"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (vastus == DialogResult.Yes)
                DialogResult = DialogResult.OK; // sulgeb akna, Form2 loeb Ostud ja Kokku
        }
    }

    public class OstuRida
    {
        [Browsable(false)]
        public int ToodeId { get; set; }

        [DisplayName("Toode")]
        public string Nimetus { get; set; } = "";

        public int Kogus { get; set; }

        public decimal Hind { get; set; }

        public decimal Summa => Kogus * Hind;

        /// <summary>Toote pilt (failinimi või link) tšeki jaoks; ostukorvi tabelis ei näidata.</summary>
        [Browsable(false)]
        public string? Pilt { get; set; }
    }
}