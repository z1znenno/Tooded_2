using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Tooded_2
{
    public enum KeeleKood
    {
        /// <summary>Eesti keel (põhikeel).</summary>
        Et,
        /// <summary>Vene keel.</summary>
        Ru,
        /// <summary>Inglise keel.</summary>
        En
    }

    /// <summary>
    /// Rakenduse keele vahetamine: eesti (põhikeel), vene ja inglise.
    /// Tekstid kirjutatakse kohapeal kolmes keeles: Keel.T("Salvesta", "Сохранить", "Save").
    /// Valitud keel salvestatakse faili keel.txt ja jääb järgmiseks käivituseks meelde.
    /// </summary>
    public static class Keel
    {
        private static readonly string Fail = Path.Combine(AppContext.BaseDirectory, "keel.txt");

        /// <summary>Praegu valitud keel.</summary>
        public static KeeleKood Praegune { get; private set; } = Lae();

        /// <summary>Käivitub, kui kasutaja vahetab keelt. Vormid uuendavad siis oma tekstid.</summary>
        public static event Action? Muutus;

        /// <summary>Tagastab teksti praeguses keeles.</summary>
        public static string T(string et, string ru, string en) => Praegune switch
        {
            KeeleKood.Ru => ru,
            KeeleKood.En => en,
            _ => et
        };

        /// <summary>Vahetab keelt, salvestab valiku ja teavitab avatud aknaid.</summary>
        public static void Vali(KeeleKood keel)
        {
            if (keel == Praegune) return;
            Praegune = keel;
            try { File.WriteAllText(Fail, keel.ToString()); }
            catch { /* valik kehtib vähemalt selle käivituse lõpuni */ }
            Muutus?.Invoke();
        }

        /// <summary>
        /// Rakendab tekstid kohe ja uuesti iga kord, kui keel vahetub.
        /// Kui juhtelement (vorm, kaart) suletakse, lõpetatakse jälgimine automaatselt.
        /// </summary>
        public static void Jalgi(Control omanik, Action rakenda)
        {
            void Kaitstud()
            {
                if (!omanik.IsDisposed) rakenda();
            }

            Kaitstud();
            Muutus += Kaitstud;
            omanik.Disposed += (s, e) => Muutus -= Kaitstud;
        }

        private static KeeleKood Lae()
        {
            try
            {
                if (File.Exists(Fail) &&
                    Enum.TryParse(File.ReadAllText(Fail).Trim(), true, out KeeleKood keel) &&
                    Enum.IsDefined(keel))
                    return keel;
            }
            catch { /* vigane fail -> põhikeel */ }
            return KeeleKood.Et;
        }
    }

    /// <summary>Keelevalik "ET | RU | EN" – klõps vahetab kogu rakenduse keelt.</summary>
    [DesignerCategory("")]
    public class KeeleValik : Control
    {
        private static readonly (KeeleKood Kood, string Tekst)[] Keeled =
        {
            (KeeleKood.Et, "ET"), (KeeleKood.Ru, "RU"), (KeeleKood.En, "EN")
        };

        private int hover = -1;

        public KeeleValik()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(132, 32);
            Cursor = Cursors.Hand;
            Font = new Font(Teema.FondiNimi, 9F, FontStyle.Bold);
            Keel.Muutus += KeelMuutus;
        }

        private void KeelMuutus()
        {
            if (!IsDisposed) Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Keel.Muutus -= KeelMuutus;
            base.Dispose(disposing);
        }

        private int Lahter(int x) => Math.Clamp(x * Keeled.Length / Math.Max(1, Width), 0, Keeled.Length - 1);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int uus = Lahter(e.X);
            if (uus != hover) { hover = uus; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hover = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left) Keel.Vali(Keeled[Lahter(e.X)].Kood);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var vanem = Teema.VanemaTaust(this);
            g.Clear(vanem);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Tumedal päisel hele tekst, heledal taustal tume
            bool tume = vanem.GetBrightness() < 0.5f;
            Color riba = tume ? Teema.PaisHover : Color.White;
            Color tekst = tume ? Color.FromArgb(210, 220, 235) : Teema.Tekst;

            using (var tee = Teema.Umar(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 8))
            using (var pintsel = new SolidBrush(riba))
            {
                g.FillPath(pintsel, tee);
                if (!tume)
                {
                    using var pliiats = new Pen(Teema.Piir);
                    g.DrawPath(pliiats, tee);
                }
            }

            float laius = Width / (float)Keeled.Length;
            for (int i = 0; i < Keeled.Length; i++)
            {
                var ala = new RectangleF(i * laius + 3, 3, laius - 6, Height - 6);
                bool valitud = Keeled[i].Kood == Keel.Praegune;
                if (valitud || i == hover)
                {
                    using var tee = Teema.Umar(ala, 6);
                    using var pintsel = new SolidBrush(valitud ? Teema.Aktsent
                        : tume ? Color.FromArgb(58, 82, 120) : Teema.Taust);
                    g.FillPath(pintsel, tee);
                }
                TextRenderer.DrawText(g, Keeled[i].Tekst, Font, Rectangle.Round(ala),
                    valitud ? Color.White : tekst,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }
    }
}
