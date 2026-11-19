namespace Tooded_2
{
    // Klikkermäng "Töö": iga klikk toob raha, mille eest saab poes tooteid osta.
    public class Too : Form
    {
        private const decimal KlikiBaas = 0.10m;     // € ühe kliki eest (tase 1)
        private const decimal AbiliseTulu = 0.05m;   // € sekundis ühe abilise kohta

        private readonly AppDbContext _context = new();
        private readonly User _user;

        private readonly Label rahaLabel, infoLabel, plussLabel;
        private readonly Label rahaPealkiri, taiendused;
        private readonly Label? paisPealkiri;
        private readonly Font tooFont, tooFontVaike;
        private readonly TeemaNupp tooBtn, tooriistBtn, abilineBtn;
        private readonly System.Windows.Forms.Timer abiTimer, salvestaTimer, plussTimer;

        private bool muudetud;


        public Too()
        {
            _user = Session.LaeKasutaja(_context)
                    ?? throw new InvalidOperationException(
                        Keel.T("Kasutajat ei leitud.", "Пользователь не найден.", "User not found."));

            // Aken on laiem, kui eestikeelsed tekstid nõuaksid: vene ja inglise tekstid on pikemad
            Teema.Vorm(this, new Size(560, 550));
            StartPosition = FormStartPosition.CenterParent;

            // ---------- Päis: logo, pealkiri | keelevalik ----------
            var pais = Teema.LooPais(this, "");
            paisPealkiri = pais.Controls.OfType<Label>().FirstOrDefault();
            var keeleValik = new KeeleValik();
            keeleValik.Location = new Point(pais.Width - keeleValik.Width - 16, 16);
            pais.Controls.Add(keeleValik);
            Controls.Add(pais);

            rahaPealkiri = Teema.Silt("", Teema.Tavaline, Teema.TekstHall);
            rahaPealkiri.AutoSize = false;
            rahaPealkiri.TextAlign = ContentAlignment.MiddleCenter;
            rahaPealkiri.SetBounds(20, 74, 520, 22);
            Controls.Add(rahaPealkiri);

            rahaLabel = new Label
            {
                Font = new Font(Teema.FondiNimi, 24F, FontStyle.Bold),
                ForeColor = Teema.Tekst,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 96),
                Size = new Size(520, 46)
            };
            Controls.Add(rahaLabel);

            infoLabel = new Label
            {
                ForeColor = Teema.TekstHall,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 142),
                Size = new Size(520, 22)
            };
            Controls.Add(infoLabel);

            // Ümmargune oranž nupp (raadius = pool laiusest, servad on sujuvad)
            // Venekeelne tekst on pikem, selle jaoks on väiksem font
            tooFont = new Font(Teema.FondiNimi, 20F, FontStyle.Bold);
            tooFontVaike = new Font(Teema.FondiNimi, 17F, FontStyle.Bold);
            tooBtn = new TeemaNupp
            {
                Font = tooFont,
                Size = new Size(200, 200),
                Raadius = 100,
                Location = new Point(180, 174)
            };
            tooBtn.Click += TooBtn_Click;
            Controls.Add(tooBtn);

            // "+0.10 €" tekst, mis klikil korraks ilmub
            plussLabel = new Label
            {
                Font = new Font(Teema.FondiNimi, 12F, FontStyle.Bold),
                ForeColor = Teema.Edu,
                BackColor = Color.Transparent,
                AutoSize = true,
                Visible = false
            };
            Controls.Add(plussLabel);
            plussLabel.BringToFront();

            taiendused = Teema.Silt("", Teema.Pealkiri);
            taiendused.Location = new Point(18, 386);
            Controls.Add(taiendused);

            tooriistBtn = new TeemaNupp { Stiil = NupuStiil.Teine, Location = new Point(20, 422), Size = new Size(520, 52) };
            tooriistBtn.Click += TooriistBtn_Click;
            Controls.Add(tooriistBtn);

            abilineBtn = new TeemaNupp { Stiil = NupuStiil.Teine, Location = new Point(20, 484), Size = new Size(520, 52) };
            abilineBtn.Click += AbilineBtn_Click;
            Controls.Add(abilineBtn);

            // Abilised teenivad raha iga sekund
            abiTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            abiTimer.Tick += (s, e) =>
            {
                if (_user.AbilisteArv == 0) return;
                LisaRaha(_user.AbilisteArv * AbiliseTulu);
            };
            abiTimer.Start();

            // Salvestame andmebaasi iga 5 sekundi järel (mitte iga kliki peale)
            salvestaTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            salvestaTimer.Tick += (s, e) => Salvesta();
            salvestaTimer.Start();

            plussTimer = new System.Windows.Forms.Timer { Interval = 30 };
            plussTimer.Tick += PlussTimer_Tick;

            FormClosing += (s, e) =>
            {
                abiTimer.Stop();
                salvestaTimer.Stop();
                Salvesta();
            };

            // Tekstid valitud keeles (ja uuesti iga kord, kui keelt vahetatakse)
            Keel.Jalgi(this, RakendaKeel);
        }

        // ---------- Arvutused ----------

        private decimal KlikiTulu => KlikiBaas * _user.TooriistaTase;

        private decimal TooriistaHind => Math.Round(1.00m * (decimal)Math.Pow(1.6, _user.TooriistaTase - 1), 2);

        private decimal AbiliseHind => Math.Round(2.00m * (decimal)Math.Pow(1.5, _user.AbilisteArv), 2);

        // ---------- Sündmused ----------

        private void TooBtn_Click(object? sender, EventArgs e)
        {
            LisaRaha(KlikiTulu);
            NäitaPlussi(KlikiTulu);
        }

        private void TooriistBtn_Click(object? sender, EventArgs e)
        {
            if (_user.Raha < TooriistaHind) return;
            _user.Raha -= TooriistaHind;
            _user.TooriistaTase++;
            muudetud = true;
            Salvesta();
            UuendaUI();
        }

        private void AbilineBtn_Click(object? sender, EventArgs e)
        {
            if (_user.Raha < AbiliseHind) return;
            _user.Raha -= AbiliseHind;
            _user.AbilisteArv++;
            muudetud = true;
            Salvesta();
            UuendaUI();
        }

        private void LisaRaha(decimal summa)
        {
            _user.Raha += summa;
            muudetud = true;
            UuendaUI();
        }

        // ---------- UI ----------

        /// <summary>Vormi staatilised tekstid praeguses keeles; olekust sõltuvad tekstid uuendab UuendaUI.</summary>
        private void RakendaKeel()
        {
            Text = Keel.T("Töö – teeni raha", "Работа – зарабатывай деньги", "Work – earn money");
            if (paisPealkiri != null)
                paisPealkiri.Text = Keel.T("Töö", "Работа", "Work");

            rahaPealkiri.Text = Keel.T("Sinu rahakott", "Твой кошелёк", "Your wallet");
            taiendused.Text = Keel.T("Täiendused", "Улучшения", "Upgrades");

            tooBtn.Font = Keel.Praegune == KeeleKood.Ru ? tooFontVaike : tooFont;
            tooBtn.Text = Keel.T("TÖÖTA!", "РАБОТАЙ!", "WORK!");
            tooBtn.Invalidate();

            UuendaUI();
        }

        private void UuendaUI()
        {
            rahaLabel.Text = $"{_user.Raha:0.00} €";

            decimal abiTulu = _user.AbilisteArv * AbiliseTulu;
            infoLabel.Text = Keel.T(
                $"Klikk: +{KlikiTulu:0.00} €   |   Abilised: +{abiTulu:0.00} €/s",
                $"Клик: +{KlikiTulu:0.00} €   |   Помощники: +{abiTulu:0.00} €/с",
                $"Click: +{KlikiTulu:0.00} €   |   Helpers: +{abiTulu:0.00} €/s");

            int jargmineTase = _user.TooriistaTase + 1;
            tooriistBtn.Text = Keel.T(
                $"🔨 Parem tööriist (tase {jargmineTase})  –  {TooriistaHind:0.00} €",
                $"🔨 Улучшить инструмент (уровень {jargmineTase})  –  {TooriistaHind:0.00} €",
                $"🔨 Better tool (level {jargmineTase})  –  {TooriistaHind:0.00} €");
            tooriistBtn.Enabled = _user.Raha >= TooriistaHind;

            abilineBtn.Text = Keel.T(
                $"👷 Palka abiline ({_user.AbilisteArv} tk)  –  {AbiliseHind:0.00} €",
                $"👷 Нанять помощника (нанято: {_user.AbilisteArv})  –  {AbiliseHind:0.00} €",
                $"👷 Hire a helper ({_user.AbilisteArv} hired)  –  {AbiliseHind:0.00} €");
            abilineBtn.Enabled = _user.Raha >= AbiliseHind;
        }

        private void NäitaPlussi(decimal summa)
        {
            plussLabel.Text = $"+{summa:0.00} €";
            var hiir = PointToClient(Cursor.Position);
            plussLabel.Location = new Point(hiir.X - 20, hiir.Y - 30);
            plussLabel.Visible = true;
            plussLabel.Tag = 15; // mitu sammu üles liigub
            plussTimer.Start();
        }

        private void PlussTimer_Tick(object? sender, EventArgs e)
        {
            int samme = (int)(plussLabel.Tag ?? 0);
            if (samme <= 0)
            {
                plussLabel.Visible = false;
                plussTimer.Stop();
                return;
            }
            plussLabel.Top -= 2;
            plussLabel.Tag = samme - 1;
        }

        private void Salvesta()
        {
            if (!muudetud) return;
            try
            {
                _context.SaveChanges();
                muudetud = false;
            }
            catch (Exception ex)
            {
                salvestaTimer.Stop();
                MessageBox.Show(Keel.T("Raha salvestamine ebaõnnestus: ", "Не удалось сохранить деньги: ", "Failed to save the money: ")
                    + (ex.InnerException?.Message ?? ex.Message),
                    Keel.T("Viga", "Ошибка", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                abiTimer.Dispose();
                salvestaTimer.Dispose();
                plussTimer.Dispose();
                tooFont.Dispose();
                tooFontVaike.Dispose();
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
