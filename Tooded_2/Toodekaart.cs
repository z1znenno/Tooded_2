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

        private static readonly Color Aktsent = Color.FromArgb(46, 125, 50);
        private static readonly Color Piirjoon = Color.FromArgb(222, 222, 222);
        private static readonly Image Kohataide = LooKohataide();

        private readonly PictureBox pilt;
        private readonly Label nimiLabel;
        private readonly Label kategooriaLabel;
        private readonly Label hindLabel;
        private readonly Label laosLabel;
        private readonly NumericUpDown kogusNum;
        private readonly Button lisaBtn;
        private bool hover;

        public PoodToode Toode { get; }

        /// <summary>Käivitub, kui vajutatakse "Lisa korvi". Argument on valitud kogus.</summary>
        public event EventHandler<int>? LisaKorvi;

        public ToodeKaart(PoodToode toode)
        {
            Toode = toode;
            DoubleBuffered = true;
            Size = new Size(180, 262);
            Margin = new Padding(7);
            BackColor = Color.White;

            var laetud = LaePilt(toode.Pilt);
            pilt = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(160, 110),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245, 245, 245),
                Image = laetud ?? Kohataide
            };

            nimiLabel = new Label
            {
                Location = new Point(10, 126),
                Size = new Size(160, 22),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                AutoEllipsis = true,
                Text = toode.Nimetus
            };

            kategooriaLabel = new Label
            {
                Location = new Point(10, 148),
                Size = new Size(160, 18),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.Gray,
                AutoEllipsis = true,
                Text = string.IsNullOrWhiteSpace(toode.Kategooria) ? "Kategooriata" : toode.Kategooria
            };

            hindLabel = new Label
            {
                Location = new Point(10, 168),
                Size = new Size(160, 28),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Aktsent,
                Text = $"{toode.Hind:0.00} €"
            };

            laosLabel = new Label
            {
                Location = new Point(10, 198),
                Size = new Size(160, 18),
                Font = new Font("Segoe UI", 8.5F)
            };

            kogusNum = new NumericUpDown
            {
                Location = new Point(10, 224),
                Size = new Size(55, 25),
                Font = new Font("Segoe UI", 9F),
                TextAlign = HorizontalAlignment.Center
            };

            lisaBtn = new Button
            {
                Location = new Point(72, 222),
                Size = new Size(98, 28),
                Text = "Lisa korvi",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Aktsent,
                Cursor = Cursors.Hand
            };
            lisaBtn.FlatAppearance.BorderSize = 0;
            lisaBtn.Click += (_, _) => LisaKorvi?.Invoke(this, (int)kogusNum.Value);

            Controls.AddRange(new Control[] { pilt, nimiLabel, kategooriaLabel, hindLabel, laosLabel, kogusNum, lisaBtn });

            var vihje = new ToolTip();
            vihje.SetToolTip(nimiLabel, toode.Nimetus);

            // Kui pilti ei leitud, näitab hiire all, mis väärtus andmebaasis on (aitab vigu leida)
            if (laetud == null)
            {
                vihje.SetToolTip(pilt, string.IsNullOrWhiteSpace(toode.Pilt)
                    ? "Andmebaasis pole pilti määratud"
                    : $"Pilti ei leitud: \"{toode.Pilt}\"\nKäivituskaust: {Application.StartupPath}");
            }

            SeoHover(this);
            foreach (Control c in Controls) SeoHover(c);

            SeaSaadaval(toode.Kogus);
        }

        /// <summary>Uuendab, mitu tükki saab veel korvi lisada.</summary>
        public void SeaSaadaval(int saadaval)
        {
            saadaval = Math.Max(0, saadaval);

            kogusNum.Minimum = 0;
            kogusNum.Maximum = saadaval;
            kogusNum.Minimum = saadaval > 0 ? 1 : 0;
            kogusNum.Value = kogusNum.Minimum;

            bool saab = saadaval > 0;
            kogusNum.Enabled = saab;
            lisaBtn.Enabled = saab;
            lisaBtn.BackColor = saab ? Aktsent : Color.Silver;

            if (Toode.Kogus <= 0) laosLabel.Text = "Otsas";
            else if (saab) laosLabel.Text = $"Laos: {saadaval} tk";
            else laosLabel.Text = "Kõik on juba korvis";

            laosLabel.ForeColor = saab ? Color.DimGray : Color.Firebrick;
        }

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var tee = Umar(new Rectangle(0, 0, Width - 1, Height - 1), 10);
            using var pliiats = new Pen(hover ? Aktsent : Piirjoon, hover ? 2f : 1f);
            e.Graphics.DrawPath(pliiats, tee);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            using var tee = Umar(new Rectangle(0, 0, Width, Height), 10);
            var vana = Region;
            Region = new Region(tee);
            vana?.Dispose();
        }

        private static GraphicsPath Umar(Rectangle r, int raadius)
        {
            int d = raadius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // ---------- Pildid ----------

        private static Image? LaePilt(string? fail)
        {
            if (string.IsNullOrWhiteSpace(fail)) return null;

            string? tee = LeiaPildiFail(fail);
            if (tee == null) return null;

            try
            {
                // Kopeerime pildi mällu, et fail ei jääks lukku
                using var voog = File.OpenRead(tee);
                using var algne = Image.FromStream(voog);
                return new Bitmap(algne);
            }
            catch
            {
                return null;
            }
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

        private static Image LooKohataide()
        {
            var bmp = new Bitmap(160, 110);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(245, 245, 245));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var f = new Font("Segoe UI", 9F, FontStyle.Italic);
            using var pintsel = new SolidBrush(Color.Gray);
            using var formaat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString("Pilt puudub", f, pintsel, new RectangleF(0, 0, 160, 110), formaat);
            return bmp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && pilt.Image != null && pilt.Image != Kohataide)
            {
                pilt.Image.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}