using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Tooded_2
{
    public partial class AdminPanel : Form
    {
        private readonly AppDbContext _context;
        private string? valitudPilt;

        // Pildi link internetist (eelistatakse valitud failile)
        private readonly TextBox pildiUrlEntry = new();
        private readonly TeemaNupp laadiUrlBtn = Teema.Nupp("", NupuStiil.Teine);

        // Tekstid, mis vahetuvad koos keelega (vt RakendaKeel)
        private readonly Label paisSilt = new()
        {
            Font = new Font(Teema.FondiNimi, 11F),
            ForeColor = Color.FromArgb(190, 202, 220),
            AutoSize = true,
            Location = new Point(222, 22)
        };
        private readonly Label andmedPealkiri = Pealkiri();
        private readonly Label pildiPealkiri = Pealkiri();
        private readonly Label tabeliPealkiri = Pealkiri();

        // Viimase ostu andmed – ostuInfoLabel koostatakse neist keele vahetumisel uuesti
        private int viimaneOstTk;
        private string? viimaneOstTooted; // null = ostu pole veel tehtud
        private decimal viimaneOstKokku;

        // Suureneb iga uue pildi näitamisega – nii ei kirjuta aeglane vana laadimine uut pilti üle
        private int pildiLaadimine;

        // Üks koht, kus pildid asuvad (projekti kaust, mitte bin\Debug)
        private static readonly string PildiKaust = Path.GetFullPath(@"..\..\..\Images");

        public AdminPanel()
        {
            InitializeComponent();
            _context = new AppDbContext();

            Directory.CreateDirectory(PildiKaust);

            kategooriaBox.DropDownStyle = ComboBoxStyle.DropDownList;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            Kujunda();
            LaeKategooriad();
            LaeTooted();
        }

        // ---------- Kujundus (kaup24 stiilis) ----------

        private void Kujunda()
        {
            Teema.Vorm(this, new Size(1280, 800));
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Clear();

            // Päise alapealkiri on oma silt (paisSilt), et seda saaks keele vahetumisel muuta
            var pais = Teema.LooPais(this);
            pais.Controls.Add(paisSilt);
            poodBtn.Stiil = NupuStiil.Pais;
            poodBtn.SetBounds(pais.Width - 176, 12, 160, 40);
            pais.Controls.Add(poodBtn);

            // Keelevalik "ET | RU | EN" poe nupust vasakul
            var keeleValik = new KeeleValik { Anchor = AnchorStyles.Top | AnchorStyles.Right };
            keeleValik.Location = new Point(poodBtn.Left - 12 - keeleValik.Width, (pais.Height - keeleValik.Height) / 2);
            pais.Controls.Add(keeleValik);

            // ----- Toote andmed -----
            var andmed = new KaartPaneel { Location = new Point(16, 80), Size = new Size(730, 360) };
            andmed.Controls.Add(andmedPealkiri);

            int y = 58;
            foreach (var (silt, sisend) in new (Label, Control)[]
            {
                (nimetus, toodeEntry),
                (kogus, kogusEntry),
                (hind, hindEntry),
                (kategooria, kategooriaBox),
                (label1, kategooriaEntry)
            })
            {
                silt.AutoSize = false;
                silt.Font = Teema.Tavaline;
                silt.ForeColor = Teema.TekstHall;
                silt.TextAlign = ContentAlignment.MiddleLeft;
                silt.SetBounds(20, y, 160, 32);
                sisend.SetBounds(186, y, 250, 32);
                if (sisend is TextBox t) Teema.Sisestus(t);
                if (sisend is ComboBox c) Teema.Valik(c);
                andmed.Controls.Add(silt);
                andmed.Controls.Add(sisend);
                y += 46;
            }

            kustutaKategooriaBtn.Stiil = NupuStiil.Oht;
            kustutaKategooriaBtn.SetBounds(450, 58 + 3 * 46 - 4, 260, 40);
            lisaKategooriaBtn.Stiil = NupuStiil.Teine;
            lisaKategooriaBtn.SetBounds(450, 58 + 4 * 46 - 4, 260, 40);

            lisaBtn.SetBounds(20, 296, 165, 44);
            uuendaBtn.Stiil = NupuStiil.Teine;
            uuendaBtn.SetBounds(195, 296, 165, 44);
            kustutaBtn.Stiil = NupuStiil.Oht;
            kustutaBtn.SetBounds(370, 296, 165, 44);
            puhastaBtn.Stiil = NupuStiil.Teine;
            puhastaBtn.SetBounds(545, 296, 165, 44);
            puhastaBtn.Click += (s, e) => PuhastaVorm();

            andmed.Controls.AddRange(new Control[]
            {
                kustutaKategooriaBtn, lisaKategooriaBtn, lisaBtn, uuendaBtn, kustutaBtn, puhastaBtn
            });

            // ----- Pilt -----
            var pildiKaart = new KaartPaneel { Location = new Point(762, 80), Size = new Size(502, 360) };
            pildiKaart.Controls.Add(pildiPealkiri);
            toodePilt.SetBounds(20, 56, 462, 156);
            toodePilt.SizeMode = PictureBoxSizeMode.Zoom;
            toodePilt.BackColor = Teema.Taust;

            // Link internetist + "Laadi" nupp (eelvaade)
            Teema.Sisestus(pildiUrlEntry);
            // Nupp on laiem, et mahuks ka "Загрузить"
            pildiUrlEntry.SetBounds(20, 228, 322, 32);
            laadiUrlBtn.SetBounds(352, 222, 130, 40);
            laadiUrlBtn.Click += LaadiUrlBtn_Click;
            pildiUrlEntry.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                LaadiUrlBtn_Click(laadiUrlBtn, EventArgs.Empty);
            };

            otsiFailBtn.Stiil = NupuStiil.Teine;
            otsiFailBtn.SetBounds(20, 284, 200, 44);
            ostuInfoLabel.Font = Teema.Vaike;
            ostuInfoLabel.ForeColor = Teema.TekstHall;
            ostuInfoLabel.SetBounds(232, 270, 250, 80);
            pildiKaart.Controls.AddRange(new Control[]
            {
                toodePilt, pildiUrlEntry, laadiUrlBtn, otsiFailBtn, ostuInfoLabel
            });

            // ----- Tooted tabelis -----
            var tabeliKaart = new KaartPaneel
            {
                Location = new Point(16, 456),
                Size = new Size(1248, 328),
                Padding = new Padding(16, 52, 16, 16)
            };
            tabeliKaart.Controls.Add(tabeliPealkiri);
            Teema.Tabel(dataGridView1);
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabeliKaart.Controls.Add(dataGridView1);

            foreach (var nupp in new[] { lisaKategooriaBtn, kustutaKategooriaBtn, otsiFailBtn, laadiUrlBtn,
                                         lisaBtn, kustutaBtn, uuendaBtn, puhastaBtn, poodBtn })
                nupp.Font = Teema.Paks;
            ostuInfoLabel.Font = Teema.Vaike;

            Controls.AddRange(new Control[] { andmed, pildiKaart, tabeliKaart });
            Controls.Add(pais);

            // Tekstid kohe ja uuesti iga kord, kui keel vahetub
            Keel.Jalgi(this, RakendaKeel);
        }

        private static Label Pealkiri()
        {
            var silt = Teema.Silt("", Teema.Pealkiri);
            silt.Location = new Point(18, 16);
            return silt;
        }

        // ---------- Keel ----------

        /// <summary>Paneb kõik akna tekstid praeguses keeles (käivitub ka keele vahetumisel).</summary>
        private void RakendaKeel()
        {
            string adminPaneel = Keel.T("Admin paneel", "Панель администратора", "Admin panel");
            Text = $"{TsekiKoostaja.PoeNimi} – {adminPaneel}";
            paisSilt.Text = adminPaneel;
            poodBtn.Text = Keel.T("🛒 Pood", "🛒 Магазин", "🛒 Shop");

            // Toote andmed
            andmedPealkiri.Text = Keel.T("Toote andmed", "Данные товара", "Product details");
            nimetus.Text = Keel.T("Nimetus", "Название", "Name");
            kogus.Text = Keel.T("Kogus", "Количество", "Quantity");
            hind.Text = Keel.T("Hind (€)", "Цена (€)", "Price (€)");
            kategooria.Text = Keel.T("Kategooria", "Категория", "Category");
            label1.Text = Keel.T("Uus kategooria", "Новая категория", "New category");

            kustutaKategooriaBtn.Text = Keel.T("Kustuta kategooria", "Удалить категорию", "Delete category");
            lisaKategooriaBtn.Text = Keel.T("+ Lisa kategooria", "+ Добавить категорию", "+ Add category");

            // Alumise rea nupud on 165 px laiad – vene ja inglise tekst on seepärast lühem
            lisaBtn.Text = Keel.T("+ Lisa toode", "+ Добавить", "+ Add");
            uuendaBtn.Text = Keel.T("Salvesta", "Сохранить", "Save");
            kustutaBtn.Text = Keel.T("Kustuta", "Удалить", "Delete");
            puhastaBtn.Text = Keel.T("Puhasta", "Очистить", "Clear");

            // Pilt
            pildiPealkiri.Text = Keel.T("Pilt", "Фото", "Image");
            pildiUrlEntry.PlaceholderText = Keel.T("https://... pildi link", "https://... ссылка на фото",
                "https://... image link");
            laadiUrlBtn.Text = Keel.T("Laadi", "Загрузить", "Load");
            otsiFailBtn.Text = Keel.T("Vali pilt...", "Выбрать фото...", "Choose image...");
            NaitaOstuInfot();

            // Tooted tabelis
            tabeliPealkiri.Text = Keel.T("Tooted", "Товары", "Products");
            SeaVeeruPealkirjad();
        }

        // Viimase ostu kokkuvõte (või algseis, kui ostu pole veel tehtud) praeguses keeles
        private void NaitaOstuInfot()
        {
            string tooted = viimaneOstTooted ?? "—";
            string kokku = viimaneOstTooted == null ? "0.00" : $"{viimaneOstKokku:0.00}";
            ostuInfoLabel.Text = Keel.T(
                $"Ostetud: {viimaneOstTk} tk\n{tooted}\nKokku: {kokku} €",
                $"Куплено: {viimaneOstTk} шт.\n{tooted}\nИтого: {kokku} €",
                $"Purchased: {viimaneOstTk} pcs\n{tooted}\nTotal: {kokku} €");
        }

        // Tabeli veergude pealkirjad. Veergude nimed (Id, Toodenimetus, ...) jäävad samaks,
        // sest kood loeb lahtreid nende järgi – muutub ainult HeaderText.
        private void SeaVeeruPealkirjad()
        {
            foreach (var (nimi, tekst) in new[]
            {
                ("Id", Keel.T("Id", "ID", "ID")),
                ("Toodenimetus", Keel.T("Toodenimetus", "Название товара", "Product name")),
                ("Kogus", Keel.T("Kogus", "Количество", "Quantity")),
                ("Hind", Keel.T("Hind", "Цена", "Price")),
                ("Kategooria", Keel.T("Kategooria", "Категория", "Category")),
                ("Pilt", Keel.T("Pilt", "Фото", "Image"))
            })
            {
                if (dataGridView1.Columns[nimi] is { } veerg)
                    veerg.HeaderText = tekst;
            }
        }

        private void PuhastaVorm()
        {
            toodeEntry.Clear();
            kogusEntry.Clear();
            hindEntry.Clear();
            kategooriaEntry.Clear();
            pildiUrlEntry.Clear();
            valitudPilt = null;
            _ = NaitaPilti(null);
            dataGridView1.ClearSelection();
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

            // Uus sidumine loob veerud uuesti – pealkirjad tuleb uuesti panna
            SeaVeeruPealkirjad();
        }

        // ---------- Abimeetodid ----------

        /// <summary>
        /// Näitab pilti eelvaates. Pilt võib olla faili tee või http(s) link
        /// (link laetakse taustal vahemälu kaudu). Tagastab false, kui pilti ei õnnestunud laadida.
        /// </summary>
        private async Task<bool> NaitaPilti(string? pilt)
        {
            int nr = ++pildiLaadimine;
            SeaEelvaade(null);

            if (string.IsNullOrWhiteSpace(pilt)) return true;

            Bitmap? bmp;
            if (PildiLaadija.OnUrl(pilt))
            {
                bmp = PildiLaadija.VotaMalust(pilt);
                if (bmp == null)
                {
                    try { bmp = await PildiLaadija.LaeUrlistAsync(pilt); }
                    catch { bmp = null; }
                }
            }
            else
            {
                // Laeb pildi nii, et fail ei jää lukku
                bmp = PildiLaadija.LaeFailist(pilt);
            }

            // Vahepeal valiti juba teine pilt või aken suleti
            if (nr != pildiLaadimine || IsDisposed || toodePilt.IsDisposed)
            {
                bmp?.Dispose();
                return false;
            }

            SeaEelvaade(bmp);
            return bmp != null;
        }

        private void SeaEelvaade(Image? uus)
        {
            var vana = toodePilt.Image;
            toodePilt.Image = uus;
            if (vana != null && vana != uus) vana.Dispose();
        }

        // Kohalik failinimi tabelist -> täielik tee Images kaustas; link jääb samaks
        private static string? PildiAllikas(string? pilt)
        {
            if (string.IsNullOrWhiteSpace(pilt)) return null;
            return PildiLaadija.OnUrl(pilt) ? pilt.Trim() : Path.Combine(PildiKaust, pilt);
        }

        /// <summary>
        /// Loeb lingi lahtrist. Tühi = linki pole (true, url = null).
        /// Vigane tekst = false (kasutajale näidatakse teade).
        /// </summary>
        private bool LoePildiUrl(out string? url)
        {
            url = null;
            string tekst = pildiUrlEntry.Text.Trim();
            if (tekst.Length == 0) return true;

            if (!PildiLaadija.OnUrl(tekst))
            {
                MessageBox.Show(Keel.T("Pildi link peab algama http:// või https://",
                    "Ссылка на фото должна начинаться с http:// или https://",
                    "The image link must start with http:// or https://"));
                return false;
            }

            url = tekst;
            return true;
        }

        // Kopeerib valitud pildi Images kausta ja tagastab failinime (või null)
        private string? KopeeriPilt()
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
                MessageBox.Show(Keel.T("Sisesta toote nimetus!", "Введите название товара!",
                    "Enter the product name!"));
                return false;
            }
            if (!int.TryParse(kogusTekst, out kogus))
            {
                MessageBox.Show(Keel.T("Kogus peab olema täisarv!", "Количество должно быть целым числом!",
                    "Quantity must be a whole number!"));
                return false;
            }
            if (!float.TryParse(hindTekst, out hind))
            {
                MessageBox.Show(Keel.T("Hind on vigane!", "Цена указана неверно!", "The price is invalid!"));
                return false;
            }
            if (kategooriaBox.SelectedValue is not int id)
            {
                MessageBox.Show(Keel.T("Vali kategooria!", "Выберите категорию!", "Select a category!"));
                return false;
            }

            katId = id;
            return true;
        }

        private int? ValitudId()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show(Keel.T("Vali tabelist toode!", "Выберите товар в таблице!",
                    "Select a product in the table!"));
                return null;
            }
            return (int)dataGridView1.SelectedRows[0].Cells["Id"].Value;
        }

        // ---------- Nupud ----------

        private void btnValiPilt_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = Keel.T("Pildid", "Изображения", "Images") + "|*.jpg;*.png;*.jpeg";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                valitudPilt = ofd.FileName;
                pildiUrlEntry.Clear(); // kohalik fail asendab lingi
                _ = NaitaPilti(valitudPilt);
            }
        }

        // "Laadi" – näitab lingilt pildi eelvaadet
        private async void LaadiUrlBtn_Click(object? sender, EventArgs e)
        {
            if (!LoePildiUrl(out string? url)) return;
            if (url == null)
            {
                MessageBox.Show(Keel.T("Kleebi pildi link lahtrisse!", "Вставьте ссылку на фото в поле!",
                    "Paste the image link into the field!"));
                return;
            }

            valitudPilt = null; // link on eelistatud kohalikule failile
            laadiUrlBtn.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                bool onnestus = await NaitaPilti(url);
                if (!onnestus && !IsDisposed && pildiUrlEntry.Text.Trim() == url)
                    MessageBox.Show(
                        Keel.T("Pilti ei õnnestunud laadida. Kontrolli linki ja internetiühendust.",
                            "Не удалось загрузить фото. Проверьте ссылку и подключение к интернету.",
                            "The image could not be loaded. Check the link and your internet connection."),
                        Keel.T("Pilt", "Фото", "Image"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed)
                {
                    Cursor = Cursors.Default;
                    laadiUrlBtn.Enabled = true;
                }
            }
        }

        private void btnLisa_Click(object sender, EventArgs e)
        {
            if (!LoeVorm(toodeEntry.Text, kogusEntry.Text, hindEntry.Text,
                         out int kogus, out float hind, out int katId))
                return;
            if (!LoePildiUrl(out string? url)) return;

            try
            {
                var uus = new Toode
                {
                    Toodenimetus = toodeEntry.Text,
                    Kogus = kogus,
                    Hind = hind,
                    KategooriaId = katId,
                    // Link on eelistatud; muidu kopeeritud fail; pildita toode -> "" (veerg on kohustuslik)
                    Pilt = url ?? KopeeriPilt() ?? ""
                };

                _context.Toodetabel.Add(uus);
                _context.SaveChanges();

                valitudPilt = null;
                LaeTooted();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show(Keel.T("Viga lisamisel: ", "Ошибка при добавлении: ", "Error while adding: ")
                                + ex.GetBaseException().Message);
            }
        }

        private void dataGridViewTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var rida = dataGridView1.Rows[e.RowIndex];

            toodeEntry.Text = rida.Cells["Toodenimetus"].Value?.ToString();
            kogusEntry.Text = rida.Cells["Kogus"].Value?.ToString();
            hindEntry.Text = rida.Cells["Hind"].Value?.ToString();

            if (rida.Cells["KategooriaId"].Value is int katId)
                kategooriaBox.SelectedValue = katId;

            valitudPilt = null; // uut pilti pole veel valitud

            string? pilt = rida.Cells["Pilt"].Value?.ToString();
            pildiUrlEntry.Text = PildiLaadija.OnUrl(pilt) ? pilt!.Trim() : "";
            _ = NaitaPilti(PildiAllikas(pilt));
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            int? id = ValitudId();
            if (id == null) return;

            if (!LoeVorm(toodeEntry.Text, kogusEntry.Text, hindEntry.Text,
                         out int uusKogus, out float uusHind, out int katId))
                return;
            if (!LoePildiUrl(out string? url)) return;

            try
            {
                var toode = _context.Toodetabel.Find(id.Value);
                if (toode == null) return;

                toode.Toodenimetus = toodeEntry.Text;
                toode.Kogus = uusKogus;
                toode.Hind = uusHind;
                toode.KategooriaId = katId;

                // Link on eelistatud valitud failile; kui kumbagi pole, jääb vana pilt alles
                if (url != null)
                    toode.Pilt = url;
                else if (!string.IsNullOrEmpty(valitudPilt))
                    toode.Pilt = KopeeriPilt() ?? toode.Pilt;

                _context.SaveChanges();

                valitudPilt = null;
                LaeTooted();
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show(Keel.T("Viga lisamisel: ", "Ошибка при сохранении: ", "Error while saving: ")
                                + ex.GetBaseException().Message);
            }
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            int? id = ValitudId();
            if (id == null) return;

            if (MessageBox.Show(Keel.T("Kas kustutada toode?", "Удалить товар?", "Delete the product?"),
                    Keel.T("Kinnitus", "Подтверждение", "Confirmation"),
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            var toode = _context.Toodetabel.Find(id.Value);
            if (toode == null) return;

            _context.Toodetabel.Remove(toode);
            _context.SaveChanges();

            _ = NaitaPilti(null);
            LaeTooted();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            pildiLaadimine++; // pooleliolev laadimine ei pane enam pilti
            toodePilt.Image?.Dispose();
            _context.Dispose();
            base.OnFormClosed(e);
        }

        private void btnLisaKategooria_Click(object sender, EventArgs e)
        {
            string nimi = kategooriaEntry.Text.Trim();

            if (string.IsNullOrEmpty(nimi))
            {
                MessageBox.Show(Keel.T("Sisesta kategooria nimetus!", "Введите название категории!",
                    "Enter the category name!"));
                return;
            }

            if (_context.Kategooriatabel.Any(k => k.nimetus.ToLower() == nimi.ToLower()))
            {
                MessageBox.Show(Keel.T("Selline kategooria on juba olemas!", "Такая категория уже существует!",
                    "This category already exists!"));
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
                MessageBox.Show(Keel.T("Viga kategooria lisamisel: ", "Ошибка при добавлении категории: ",
                                    "Error while adding the category: ") + ex.GetBaseException().Message);
            }
        }

        private void btnKustutaKategooria_Click(object sender, EventArgs e)
        {
            if (kategooriaBox.SelectedValue is not int katId)
            {
                MessageBox.Show(Keel.T("Vali kustutatav kategooria nimekirjast!",
                    "Выберите в списке категорию для удаления!",
                    "Select the category to delete from the list!"));
                return;
            }

            int tooteid = _context.Toodetabel.Count(t => t.KategooriaId == katId);
            if (tooteid > 0)
            {
                MessageBox.Show(Keel.T(
                    $"Kategoorias on {tooteid} toodet. Kustuta või muuda enne need tooted.",
                    $"Товаров в категории: {tooteid}. Сначала удалите или измените эти товары.",
                    $"Products in this category: {tooteid}. Delete or change these products first."));
                return;
            }

            if (MessageBox.Show(Keel.T($"Kas kustutada kategooria \"{kategooriaBox.Text}\"?",
                        $"Удалить категорию \"{kategooriaBox.Text}\"?",
                        $"Delete the category \"{kategooriaBox.Text}\"?"),
                    Keel.T("Kinnitus", "Подтверждение", "Confirmation"),
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
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
                MessageBox.Show(Keel.T("Viga kategooria kustutamisel: ", "Ошибка при удалении категории: ",
                                    "Error while deleting the category: ") + ex.GetBaseException().Message);
            }
        }

        private void poodBtn_Click(object sender, EventArgs e)
        {
            List<OstuRida> ostud;
            decimal kokku;

            using (var pood = new Pood(OtsiPoest))
            {
                if (pood.ShowDialog(this) != DialogResult.OK) return;
                ostud = pood.Ostud;
                kokku = pood.Kokku;
            }

            // Sama ost mis pealehel: raha ja laoseis muudetakse ühe andmebaasi tehinguga
            bool onnestus = OstuTehing.TeeOst(this, ostud, kokku, out string email);

            // Ost tehti eraldi andmebaasi ühendusega – selle akna jälgitud tooted on vananenud
            _context.ChangeTracker.Clear();
            LaeTooted();

            if (!onnestus) return;

            // Jätame ostu meelde, et silti saaks keele vahetumisel uuesti koostada
            viimaneOstTk = ostud.Sum(o => o.Kogus);
            viimaneOstTooted = string.Join(", ", ostud.Select(o => $"{o.Nimetus} ×{o.Kogus}"));
            viimaneOstKokku = kokku;
            NaitaOstuInfot();

            _ = OstuTehing.SaadaTsekk(this, ostud, email); // taustal, aken ei jää ootama
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