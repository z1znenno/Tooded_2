using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Tooded_2
{
    public partial class Form2 : Form
    {
        private readonly AppDbContext _context;
        private string valitudPilt;

        // Üks koht, kus pildid asuvad (projekti kaust, mitte bin\Debug)
        private static readonly string PildiKaust = Path.GetFullPath(@"..\..\..\Images");

        public Form2()
        {
            InitializeComponent();
            _context = new AppDbContext();

            Directory.CreateDirectory(PildiKaust);

            kategooriaBox.DropDownStyle = ComboBoxStyle.DropDownList;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            LaeKategooriad();
            LaeTooted();
        }

        // ---------- Andmete laadimine ----------

        private void LaeKategooriad()
        {
            // Enne DisplayMember/ValueMember, siis DataSource
            kategooriaBox.DisplayMember = "nimetus";
            kategooriaBox.ValueMember = "Id";
            kategooriaBox.DataSource = _context.Kategooriatabel.ToList();
        }

        private void LaeTooted()
        {
            dataGridView1.DataSource = _context.Toodetabel
                .Include(t => t.Kategooria)
                .Select(t => new
                {
                    t.Id,
                    t.Toodenimetus,
                    t.Kogus,
                    t.Hind,
                    t.KategooriaId,
                    Kategooria = t.Kategooria.nimetus,
                    t.Pilt
                })
                .ToList();

            if (dataGridView1.Columns["KategooriaId"] != null)
                dataGridView1.Columns["KategooriaId"].Visible = false;
        }

        // ---------- Abimeetodid ----------

        // Laeb pildi nii, et fail ei jää lukku
        private static Image LaePiltLukustamata(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var tmp = Image.FromStream(fs);
            return new Bitmap(tmp);
        }

        private void NaitaPilti(string path)
        {
            toodePilt.Image?.Dispose();
            toodePilt.Image = null;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                toodePilt.Image = LaePiltLukustamata(path);
        }

        // Kopeerib valitud pildi Images kausta ja tagastab failinime (või null)
        private string KopeeriPilt()
        {
            if (string.IsNullOrEmpty(valitudPilt))
                return null;

            string failinimi = Path.GetFileName(valitudPilt);
            string siht = Path.Combine(PildiKaust, failinimi);

            // Kui fail on juba Images kaustas, ära kopeeri iseendale
            if (!string.Equals(Path.GetFullPath(valitudPilt), siht, StringComparison.OrdinalIgnoreCase))
                File.Copy(valitudPilt, siht, true);

            return failinimi;
        }

        private bool LoeVorm(string nimi, string kogusTekst, string hindTekst,
                             out int kogus, out float hind, out int katId)
        {
            kogus = 0; hind = 0; katId = 0;

            if (string.IsNullOrWhiteSpace(nimi))
            {
                MessageBox.Show("Sisesta toote nimetus!");
                return false;
            }
            if (!int.TryParse(kogusTekst, out kogus))
            {
                MessageBox.Show("Kogus peab olema täisarv!");
                return false;
            }
            if (!float.TryParse(hindTekst, out hind))
            {
                MessageBox.Show("Hind on vigane!");
                return false;
            }
            if (kategooriaBox.SelectedValue is not int id)
            {
                MessageBox.Show("Vali kategooria!");
                return false;
            }

            katId = id;
            return true;
        }

        private int? ValitudId()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vali tabelist toode!");
                return null;
            }
            return (int)dataGridView1.SelectedRows[0].Cells["Id"].Value;
        }

        // ---------- Nupud ----------

        private void btnValiPilt_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Pildid|*.jpg;*.png;*.jpeg";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                valitudPilt = ofd.FileName;
                NaitaPilti(valitudPilt);
            }
        }

        private void btnLisa_Click(object sender, EventArgs e)
        {
            if (!LoeVorm(toodeEntry.Text, kogusEntry.Text, hindEntry.Text,
                         out int kogus, out float hind, out int katId))
                return;

            try
            {
                var uus = new Toode
                {
                    Toodenimetus = toodeEntry.Text,
                    Kogus = kogus,
                    Hind = hind,
                    KategooriaId = katId,
                    Pilt = KopeeriPilt()
                };

                _context.Toodetabel.Add(uus);
                _context.SaveChanges();

                valitudPilt = null;
                LaeTooted();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga lisamisel: " + ex.GetBaseException().Message);
            }
        }

        private void dataGridViewTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var rida = dataGridView1.Rows[e.RowIndex];

            nimetus.Text = rida.Cells["Toodenimetus"].Value?.ToString();
            kogus.Text = rida.Cells["Kogus"].Value?.ToString();
            hind.Text = rida.Cells["Hind"].Value?.ToString();

            if (rida.Cells["KategooriaId"].Value is int katId)
                kategooriaBox.SelectedValue = katId;

            valitudPilt = null; // uut pilti pole veel valitud

            string pilt = rida.Cells["Pilt"].Value?.ToString();
            NaitaPilti(string.IsNullOrEmpty(pilt) ? null : Path.Combine(PildiKaust, pilt));
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            int? id = ValitudId();
            if (id == null) return;

            if (!LoeVorm(nimetus.Text, kogus.Text, hind.Text,
                         out int uusKogus, out float uusHind, out int katId))
                return;

            try
            {
                var toode = _context.Toodetabel.Find(id.Value);
                if (toode == null) return;

                toode.Toodenimetus = nimetus.Text;
                toode.Kogus = uusKogus;
                toode.Hind = uusHind;
                toode.KategooriaId = katId;

                if (!string.IsNullOrEmpty(valitudPilt))
                    toode.Pilt = KopeeriPilt();

                _context.SaveChanges();

                valitudPilt = null;
                LaeTooted();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga lisamisel: " + ex.GetBaseException().Message);
            }
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            int? id = ValitudId();
            if (id == null) return;

            if (MessageBox.Show("Kas kustutada toode?", "Kinnitus",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            var toode = _context.Toodetabel.Find(id.Value);
            if (toode == null) return;

            _context.Toodetabel.Remove(toode);
            _context.SaveChanges();

            NaitaPilti(null);
            LaeTooted();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            toodePilt.Image?.Dispose();
            _context.Dispose();
            base.OnFormClosed(e);
        }

        private void btnLisaKategooria_Click(object sender, EventArgs e)
        {
            string nimi = kategooriaEntry.Text.Trim();

            if (string.IsNullOrEmpty(nimi))
            {
                MessageBox.Show("Sisesta kategooria nimetus!");
                return;
            }

            if (_context.Kategooriatabel.Any(k => k.nimetus.ToLower() == nimi.ToLower()))
            {
                MessageBox.Show("Selline kategooria on juba olemas!");
                return;
            }

            try
            {
                var uus = new Kategooria { nimetus = nimi };
                _context.Kategooriatabel.Add(uus);
                _context.SaveChanges();

                kategooriaEntry.Clear();
                LaeKategooriad();
                kategooriaBox.SelectedValue = uus.Id; // сразу выбираем новую категорию
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga kategooria lisamisel: " + ex.GetBaseException().Message);
            }
        }

        private void btnKustutaKategooria_Click(object sender, EventArgs e)
        {
            if (kategooriaBox.SelectedValue is not int katId)
            {
                MessageBox.Show("Vali kustutatav kategooria nimekirjast!");
                return;
            }

            int tooteid = _context.Toodetabel.Count(t => t.KategooriaId == katId);
            if (tooteid > 0)
            {
                MessageBox.Show($"Kategoorias on {tooteid} toodet. Kustuta või muuda enne need tooted.");
                return;
            }

            if (MessageBox.Show($"Kas kustutada kategooria \"{kategooriaBox.Text}\"?",
                    "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                var kategooria = _context.Kategooriatabel.Find(katId);
                if (kategooria == null) return;

                _context.Kategooriatabel.Remove(kategooria);
                _context.SaveChanges();

                LaeKategooriad();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Viga kategooria kustutamisel: " + ex.GetBaseException().Message);
            }
        }

        private async void poodBtn_Click(object sender, EventArgs e)
        {
            List<OstuRida> ostud;
            string email;
            decimal kokku;

            using (var pood = new Form3(OtsiPoest))
            {
                if (pood.ShowDialog(this) != DialogResult.OK) return;
                ostud = pood.Ostud;
                email = pood.Email;
                kokku = pood.Kokku;
            }

            // 1) Kontrollime laoseisu uuesti ja vähendame kogust andmebaasis
            foreach (var o in ostud)
            {
                var toode = _context.Toodetabel.Find(o.ToodeId);
                if (toode == null || toode.Kogus < o.Kogus)
                {
                    MessageBox.Show($"Toodet \"{o.Nimetus}\" pole laos piisavalt. Ost tühistati.", "Pood");
                    _context.ChangeTracker.Clear(); // tühistab salvestamata muudatused
                    LaeTooted();
                    return;
                }
                toode.Kogus -= o.Kogus;
            }
            _context.SaveChanges();
            LaeTooted();

            int kokkuTk = ostud.Sum(o => o.Kogus);
            string tooteNimed = string.Join(", ", ostud.Select(o => $"{o.Nimetus} ×{o.Kogus}"));
            ostuInfoLabel.Text = $"Ostetud: {kokkuTk} tk\n{tooteNimed}\nKokku: {kokku:0.00} €";

            // 2) Saadame tšeki e-postile
            DateTime aeg = DateTime.Now;
            string tsekiNr = aeg.ToString("yyyyMMddHHmmss");
            string html = TsekiKoostaja.LooHtml(ostud, tsekiNr, aeg, email);

            try
            {
                poodBtn.Enabled = false;
                Cursor = Cursors.WaitCursor;
                await EpostiSaatja.SaadaAsync(email, $"Teie ostutšekk nr {tsekiNr}", html);
                MessageBox.Show($"Ost õnnestus! Tšekk saadeti aadressile {email}.", "Pood",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ost on tehtud, aga tšeki saatmine ebaõnnestus:\n" + ex.Message, "Pood",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
                poodBtn.Enabled = true;
            }
        }

        /// <summary>Otsib tooteid andmebaasist nime või kategooria järgi (SQL LIKE).</summary>
        private List<PoodToode> OtsiPoest(string otsing)
        {
            var paring = _context.Toodetabel.AsQueryable();

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