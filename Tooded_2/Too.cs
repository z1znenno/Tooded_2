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
        private readonly Button tooBtn, tooriistBtn, abilineBtn;
        private readonly System.Windows.Forms.Timer abiTimer, salvestaTimer, plussTimer;

        private bool muudetud;

        private static readonly Font font = new("Segoe Script", 9F, FontStyle.Bold);

        public Too()
        {
            _user = Session.LaeKasutaja(_context)
                    ?? throw new InvalidOperationException("Kasutajat ei leitud.");

            Text = "Töö – teeni raha";
            ClientSize = new Size(420, 430);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = font;
            BackColor = Color.FromArgb(245, 248, 240);

            rahaLabel = new Label
            {
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 125, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 15),
                Size = new Size(400, 45)
            };
            Controls.Add(rahaLabel);

            infoLabel = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 60),
                Size = new Size(400, 25)
            };
            Controls.Add(infoLabel);

            tooBtn = new Button
            {
                Text = "TÖÖTA!",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 193, 7),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 180),
                Location = new Point(120, 95),
                Cursor = Cursors.Hand
            };
            tooBtn.FlatAppearance.BorderSize = 0;
            // Ümmargune nupp
            var ring = new System.Drawing.Drawing2D.GraphicsPath();
            ring.AddEllipse(0, 0, tooBtn.Width, tooBtn.Height);
            tooBtn.Region = new Region(ring);
            tooBtn.Click += TooBtn_Click;
            Controls.Add(tooBtn);

            // "+0.10 €" tekst, mis klikil korraks ilmub
            plussLabel = new Label
            {
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 125, 50),
                BackColor = Color.Transparent,
                AutoSize = true,
                Visible = false
            };
            Controls.Add(plussLabel);
            plussLabel.BringToFront();

            tooriistBtn = new Button { Font = font, Location = new Point(20, 295), Size = new Size(380, 50) };
            tooriistBtn.Click += TooriistBtn_Click;
            Controls.Add(tooriistBtn);

            abilineBtn = new Button { Font = font, Location = new Point(20, 355), Size = new Size(380, 50) };
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

            UuendaUI();
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

        private void UuendaUI()
        {
            rahaLabel.Text = $"{_user.Raha:0.00} €";
            infoLabel.Text = $"Klikk: +{KlikiTulu:0.00} €   |   Abilised: +{_user.AbilisteArv * AbiliseTulu:0.00} €/s";

            tooriistBtn.Text = $"🔨 Parem tööriist (tase {_user.TooriistaTase + 1})  –  {TooriistaHind:0.00} €";
            tooriistBtn.Enabled = _user.Raha >= TooriistaHind;

            abilineBtn.Text = $"👷 Palka abiline ({_user.AbilisteArv} tk)  –  {AbiliseHind:0.00} €";
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
                MessageBox.Show("Raha salvestamine ebaõnnestus: " + (ex.InnerException?.Message ?? ex.Message),
                    "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                abiTimer.Dispose();
                salvestaTimer.Dispose();
                plussTimer.Dispose();
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
