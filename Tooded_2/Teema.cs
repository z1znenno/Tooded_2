using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Tooded_2
{
    /// <summary>
    /// Rakenduse ühine kujundus (kaup24.ee stiilis): tumesinine päis, oranžid nupud,
    /// hele hall taust ja valged ümarate nurkadega kaardid.
    /// Värvide muutmiseks piisab selle faili muutmisest.
    /// </summary>
    public static class Teema
    {
        // ---------- Värvid ----------
        public static readonly Color Pais = Color.FromArgb(14, 34, 64);          // päise taust
        public static readonly Color PaisTume = Color.FromArgb(9, 24, 47);       // kategooriate riba
        public static readonly Color PaisHover = Color.FromArgb(36, 58, 92);
        public static readonly Color Aktsent = Color.FromArgb(255, 102, 0);      // oranž CTA
        public static readonly Color AktsentHover = Color.FromArgb(232, 88, 0);
        public static readonly Color AktsentHele = Color.FromArgb(255, 240, 229);
        public static readonly Color Taust = Color.FromArgb(243, 244, 246);      // lehe taust
        public static readonly Color Kaart = Color.White;
        public static readonly Color Piir = Color.FromArgb(225, 228, 233);
        public static readonly Color PiirTume = Color.FromArgb(200, 205, 212);
        public static readonly Color Tekst = Color.FromArgb(28, 32, 38);
        public static readonly Color TekstHall = Color.FromArgb(112, 120, 130);
        public static readonly Color Edu = Color.FromArgb(0, 145, 75);           // "laos"
        public static readonly Color Viga = Color.FromArgb(220, 30, 50);         // soodus / viga
        public static readonly Color Link = Color.FromArgb(0, 98, 196);
        public static readonly Color Keelatud = Color.FromArgb(226, 229, 233);

        // ---------- Fondid ----------
        public const string FondiNimi = "Segoe UI";
        public static readonly Font Tavaline = new(FondiNimi, 9.75F);
        public static readonly Font Vaike = new(FondiNimi, 8.5F);
        public static readonly Font Paks = new(FondiNimi, 9.75F, FontStyle.Bold);
        public static readonly Font Sisend = new(FondiNimi, 10.5F);
        public static readonly Font Pealkiri = new(FondiNimi, 13F, FontStyle.Bold);
        public static readonly Font SuurPealkiri = new(FondiNimi, 16F, FontStyle.Bold);

        /// <summary>Põhiline vormi seadistus: taust, font ja suurus.</summary>
        public static void Vorm(Form f, Size suurus, bool muudetav = false)
        {
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Taust;
            f.ForeColor = Tekst;
            f.Font = Tavaline;
            f.ClientSize = suurus;
            if (!muudetav)
            {
                f.FormBorderStyle = FormBorderStyle.FixedSingle;
                f.MaximizeBox = false;
            }
        }

        /// <summary>Tumesinine päis logoga. Paremale poole saab lisada nuppe (Anchor = Right).</summary>
        public static Panel LooPais(Control omanik, string? alapealkiri = null, int korgus = 64)
        {
            var pais = new Panel
            {
                Dock = DockStyle.Top,
                Height = korgus,
                Width = omanik.ClientSize.Width,
                BackColor = Pais,
                ForeColor = Color.White
            };
            var logo = new Logo { Location = new Point(18, (korgus - 36) / 2) };
            pais.Controls.Add(logo);

            if (alapealkiri != null)
            {
                pais.Controls.Add(new Label
                {
                    Text = alapealkiri,
                    Font = new Font(FondiNimi, 11F),
                    ForeColor = Color.FromArgb(190, 202, 220),
                    AutoSize = true,
                    Location = new Point(logo.Right + 14, korgus / 2 - 10)
                });
            }
            return pais;
        }

        public static TeemaNupp Nupp(string tekst, NupuStiil stiil = NupuStiil.Pohi) =>
            new() { Text = tekst, Stiil = stiil };

        public static Label Silt(string tekst, Font? font = null, Color? varv = null) => new()
        {
            Text = tekst,
            Font = font ?? Tavaline,
            ForeColor = varv ?? Tekst,
            AutoSize = true,
            BackColor = Color.Transparent
        };

        public static void Sisestus(TextBox t)
        {
            t.Font = Sisend;
            t.BorderStyle = BorderStyle.FixedSingle;
            t.BackColor = Color.White;
            t.ForeColor = Tekst;
        }

        public static void Valik(ComboBox c)
        {
            c.Font = Sisend;
            c.FlatStyle = FlatStyle.Flat;
            c.BackColor = Color.White;
            c.ForeColor = Tekst;
        }

        /// <summary>Puhas tabel: heledad horisontaalsed jooned, oranžikas valik.</summary>
        public static void Tabel(DataGridView g)
        {
            g.BorderStyle = BorderStyle.None;
            g.BackgroundColor = Color.White;
            g.GridColor = Piir;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.RowHeadersVisible = false;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Taust,
                ForeColor = TekstHall,
                Font = new Font(FondiNimi, 9F, FontStyle.Bold),
                SelectionBackColor = Taust,
                SelectionForeColor = TekstHall,
                Padding = new Padding(6, 0, 6, 0)
            };
            g.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Tekst,
                Font = Tavaline,
                SelectionBackColor = AktsentHele,
                SelectionForeColor = Tekst,
                Padding = new Padding(6, 0, 6, 0)
            };
            g.RowTemplate.Height = 34;
            g.AllowUserToResizeRows = false;
        }

        public static GraphicsPath Umar(RectangleF r, float raadius)
        {
            float d = Math.Min(raadius * 2, Math.Min(r.Width, r.Height));
            var p = new GraphicsPath();
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        /// <summary>Värv, mille peal juhtelement asub (ümarate nurkade taha joonistamiseks).</summary>
        internal static Color VanemaTaust(Control c)
        {
            for (var p = c.Parent; p != null; p = p.Parent)
                if (p.BackColor.A == 255) return p.BackColor;
            return Taust;
        }
    }

    public enum NupuStiil
    {
        /// <summary>Oranž põhinupp (Lisa korvi, Maksta).</summary>
        Pohi,
        /// <summary>Valge nupp halli äärisega.</summary>
        Teine,
        /// <summary>Valge nupp punase tekstiga (kustutamine).</summary>
        Oht,
        /// <summary>Läbipaistev nupp tumedas päises.</summary>
        Pais
    }

    /// <summary>Ümarate nurkadega nupp, mis joonistab ennast ise (sujuvad servad).</summary>
    [DesignerCategory("")]
    public class TeemaNupp : Button
    {
        private bool hover, all;
        private NupuStiil stiil = NupuStiil.Pohi;
        private int raadius = 8;

        public TeemaNupp()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = Teema.Paks;
            Size = new Size(120, 36);
        }

        [DefaultValue(NupuStiil.Pohi)]
        public NupuStiil Stiil
        {
            get => stiil;
            set { stiil = value; Invalidate(); }
        }

        [DefaultValue(8)]
        public int Raadius
        {
            get => raadius;
            set { raadius = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; all = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { all = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { all = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var vanem = Teema.VanemaTaust(this);
            g.Clear(vanem);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color taust, tekst, piir = Color.Empty;
            switch (stiil)
            {
                case NupuStiil.Teine:
                    taust = hover ? Teema.Taust : Color.White;
                    tekst = Teema.Tekst;
                    piir = hover ? Teema.PiirTume : Teema.Piir;
                    break;
                case NupuStiil.Oht:
                    taust = hover ? Color.FromArgb(253, 236, 238) : Color.White;
                    tekst = Teema.Viga;
                    piir = hover ? Teema.Viga : Teema.Piir;
                    break;
                case NupuStiil.Pais:
                    taust = hover ? Teema.PaisHover : vanem;
                    tekst = Color.White;
                    break;
                default:
                    taust = all ? Color.FromArgb(205, 75, 0) : hover ? Teema.AktsentHover : Teema.Aktsent;
                    tekst = Color.White;
                    break;
            }

            if (!Enabled)
            {
                taust = stiil == NupuStiil.Pais ? vanem : Teema.Keelatud;
                tekst = stiil == NupuStiil.Pais ? Color.FromArgb(120, 140, 170) : Teema.TekstHall;
                piir = Color.Empty;
            }

            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var tee = Teema.Umar(r, raadius))
            {
                using var pintsel = new SolidBrush(taust);
                g.FillPath(pintsel, tee);
                if (piir != Color.Empty)
                {
                    using var pliiats = new Pen(piir);
                    g.DrawPath(pliiats, tee);
                }
            }

            var lipud = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;
            var ala = new Rectangle(6, 0, Width - 12, Height);
            if (all) ala.Offset(0, 1);
            TextRenderer.DrawText(g, Text, Font, ala, tekst, lipud);
        }
    }

    /// <summary>Valge ümarate nurkadega kaart (paneel).</summary>
    [DesignerCategory("")]
    public class KaartPaneel : Panel
    {
        private Color piir = Teema.Piir;

        public KaartPaneel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Teema.Kaart;
            Padding = new Padding(16);
        }

        [DefaultValue(10)]
        public int Raadius { get; set; } = 10;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color PiirVarv
        {
            get => piir;
            set { piir = value; Invalidate(); }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Teema.VanemaTaust(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var tee = Teema.Umar(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), Raadius);
            using var pintsel = new SolidBrush(BackColor);
            g.FillPath(pintsel, tee);
            if (piir != Color.Empty)
            {
                using var pliiats = new Pen(piir);
                g.DrawPath(pliiats, tee);
            }
        }
    }

    /// <summary>Oranž ümar "pill" (nt rahakoti summa päises).</summary>
    [DesignerCategory("")]
    public class PillSilt : Label
    {
        public PillSilt()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Font = new Font(Teema.FondiNimi, 11F, FontStyle.Bold);
            ForeColor = Color.White;
            AutoSize = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Teema.VanemaTaust(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Teema.Umar(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), Height / 2f))
            using (var pintsel = new SolidBrush(Teema.Aktsent))
                g.FillPath(pintsel, tee);
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }

    /// <summary>Poe logo: oranž märk, valge nimi ja oranž number.</summary>
    [DesignerCategory("")]
    public class Logo : Control
    {
        public Logo()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(190, 36);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Märk: oranž ring, mille sees naerataja (meenutab kaup24 logo kaart ja punkte)
            using (var oranz = new SolidBrush(Teema.Aktsent))
                g.FillEllipse(oranz, 2, 4, 28, 28);
            using (var valge = new Pen(Color.White, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(valge, 9, 9, 14, 14, 20, 140);
            using (var valge = new SolidBrush(Color.White))
            {
                g.FillEllipse(valge, 10, 10, 4, 4);
                g.FillEllipse(valge, 18, 10, 4, 4);
            }

            string nimi = TsekiKoostaja.PoeNimi;
            int tyhik = nimi.LastIndexOf(' ');
            string esimene = tyhik > 0 ? nimi[..tyhik] : nimi;
            string teine = tyhik > 0 ? nimi[(tyhik + 1)..] : "";

            using var f = new Font(Teema.FondiNimi, 17F, FontStyle.Bold);
            var lipud = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            var suurus1 = TextRenderer.MeasureText(g, esimene, f, Size.Empty, lipud);
            var ala1 = new Rectangle(38, 0, suurus1.Width, Height);
            TextRenderer.DrawText(g, esimene, f, ala1, Color.White, lipud);
            if (teine.Length > 0)
                TextRenderer.DrawText(g, teine, f, new Rectangle(ala1.Right + 4, 0, Width - ala1.Right, Height),
                    Teema.Aktsent, lipud);
        }
    }

    /// <summary>Suur avalehe plaat: ikoon, pealkiri ja kirjeldus.</summary>
    [DesignerCategory("")]
    public class Plaat : Control
    {
        private bool hover;

        [DefaultValue("")]
        public string Ikoon { get; set; } = "";

        [DefaultValue("")]
        public string Kirjeldus { get; set; } = "";

        public Plaat()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size = new Size(220, 190);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Teema.VanemaTaust(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var tee = Teema.Umar(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 12))
            {
                g.FillPath(Brushes.White, tee);
                using var pliiats = new Pen(hover ? Teema.Aktsent : Teema.Piir, hover ? 2f : 1f);
                g.DrawPath(pliiats, tee);
            }

            // Ikooni ring
            int ringX = (Width - 64) / 2;
            using (var ring = new SolidBrush(hover ? Teema.Aktsent : Teema.AktsentHele))
                g.FillEllipse(ring, ringX, 22, 64, 64);
            using (var ikoonFont = new Font("Segoe UI Emoji", 22F))
                TextRenderer.DrawText(g, Ikoon, ikoonFont, new Rectangle(ringX, 22, 64, 64), Teema.Tekst,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, Text, Teema.Pealkiri, new Rectangle(8, 96, Width - 16, 28), Teema.Tekst,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, Kirjeldus, Teema.Tavaline, new Rectangle(14, 126, Width - 28, Height - 134),
                Teema.TekstHall, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
        }
    }
}
