using System.ComponentModel;

namespace Tooded_2
{
    /// <summary>
    /// "Minu konto": kasutaja saab muuta oma kasutajanime, e-posti ja salasõna
    /// ning konto jäädavalt kustutada.
    /// </summary>
    [DesignerCategory("")]
    public class Konto : Form
    {
        private const int MinParool = 4;

        // Profiil
        private readonly Label profiilPealkiri, nimiSilt, emailSilt;
        private readonly TextBox nimiEntry, emailEntry;
        private readonly TeemaNupp salvestaBtn;

        // Salasõna
        private readonly Label paroolPealkiri, vanaSilt, uusSilt, uus2Silt;
        private readonly TextBox vanaEntry, uusEntry, uus2Entry;
        private readonly TeemaNupp paroolBtn;

        // Kustutamine
        private readonly Label kustutaPealkiri, kustutaInfo, kustutaParoolSilt;
        private readonly TextBox kustutaParoolEntry;
        private readonly TeemaNupp kustutaBtn;

        /// <summary>true, kui kasutaja kustutas oma konto (helistaja peab välja logima).</summary>
        public bool Kustutatud { get; private set; }

        public Konto()
        {
            Teema.Vorm(this, new Size(500, 676));
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;

            var pais = Teema.LooPais(this);
            var keeleValik = new KeeleValik { Anchor = AnchorStyles.Top | AnchorStyles.Right };
            keeleValik.Location = new Point(pais.Width - keeleValik.Width - 18, (pais.Height - keeleValik.Height) / 2);
            pais.Controls.Add(keeleValik);

            // ---------- Profiil ----------
            var profiil = LooKaart(84, 178);
            profiilPealkiri = LooPealkiri(profiil, Teema.Tekst);
            nimiSilt = LooSilt(profiil, 52);
            nimiEntry = LooSisestus(profiil, 52, false);
            emailSilt = LooSilt(profiil, 90);
            emailEntry = LooSisestus(profiil, 90, false);
            salvestaBtn = LooNupp(profiil, 128, NupuStiil.Pohi);
            salvestaBtn.Click += Salvesta_Click;

            // ---------- Salasõna ----------
            var parool = LooKaart(profiil.Bottom + 12, 216);
            paroolPealkiri = LooPealkiri(parool, Teema.Tekst);
            vanaSilt = LooSilt(parool, 52);
            vanaEntry = LooSisestus(parool, 52, true);
            uusSilt = LooSilt(parool, 90);
            uusEntry = LooSisestus(parool, 90, true);
            uus2Silt = LooSilt(parool, 128);
            uus2Entry = LooSisestus(parool, 128, true);
            paroolBtn = LooNupp(parool, 166, NupuStiil.Pohi);
            paroolBtn.Click += Parool_Click;

            // ---------- Konto kustutamine ----------
            var kustuta = LooKaart(parool.Bottom + 12, 158);
            kustutaPealkiri = LooPealkiri(kustuta, Teema.Viga);
            kustutaInfo = new Label
            {
                Font = Teema.Vaike,
                ForeColor = Teema.TekstHall,
                AutoEllipsis = true,
                Location = new Point(22, 42),
                Size = new Size(416, 20)
            };
            kustuta.Controls.Add(kustutaInfo);
            kustutaParoolSilt = LooSilt(kustuta, 70);
            kustutaParoolEntry = LooSisestus(kustuta, 70, true);
            kustutaBtn = LooNupp(kustuta, 108, NupuStiil.Oht);
            kustutaBtn.Click += Kustuta_Click;

            Controls.Add(profiil);
            Controls.Add(parool);
            Controls.Add(kustuta);
            Controls.Add(pais);

            LaeAndmed();
            Keel.Jalgi(this, RakendaKeel);
        }

        // ---------- UI abimeetodid ----------

        private static KaartPaneel LooKaart(int y, int korgus) => new()
        {
            Location = new Point(20, y),
            Size = new Size(460, korgus),
            Padding = new Padding(0)
        };

        private static Label LooPealkiri(Control kaart, Color varv)
        {
            var silt = Teema.Silt("", Teema.Pealkiri, varv);
            silt.Location = new Point(20, 12);
            kaart.Controls.Add(silt);
            return silt;
        }

        private static Label LooSilt(Control kaart, int y)
        {
            var silt = new Label
            {
                Font = Teema.Tavaline,
                ForeColor = Teema.TekstHall,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(22, y),
                Size = new Size(162, 26)
            };
            kaart.Controls.Add(silt);
            return silt;
        }

        private static TextBox LooSisestus(Control kaart, int y, bool parool)
        {
            var t = new TextBox
            {
                Width = 252,
                Location = new Point(186, y),
                UseSystemPasswordChar = parool
            };
            Teema.Sisestus(t);
            kaart.Controls.Add(t);
            return t;
        }

        private static TeemaNupp LooNupp(Control kaart, int y, NupuStiil stiil)
        {
            var nupp = Teema.Nupp("", stiil);
            nupp.Location = new Point(186, y);
            nupp.Size = new Size(252, 36);
            kaart.Controls.Add(nupp);
            return nupp;
        }

        private void RakendaKeel()
        {
            Text = $"{TsekiKoostaja.PoeNimi} – " + Keel.T("minu konto", "мой аккаунт", "my account");

            profiilPealkiri.Text = Keel.T("Minu andmed", "Мои данные", "My details");
            nimiSilt.Text = Keel.T("Kasutajanimi", "Логин", "Username");
            emailSilt.Text = Keel.T("E-post", "Эл. почта", "E-mail");
            salvestaBtn.Text = Keel.T("Salvesta muudatused", "Сохранить изменения", "Save changes");

            paroolPealkiri.Text = Keel.T("Salasõna vahetamine", "Смена пароля", "Change password");
            vanaSilt.Text = Keel.T("Praegune salasõna", "Текущий пароль", "Current password");
            uusSilt.Text = Keel.T("Uus salasõna", "Новый пароль", "New password");
            uus2Silt.Text = Keel.T("Korda uut salasõna", "Повтор пароля", "Repeat new password");
            paroolBtn.Text = Keel.T("Vaheta salasõna", "Сменить пароль", "Change password");

            kustutaPealkiri.Text = Keel.T("Konto kustutamine", "Удаление аккаунта", "Delete account");
            kustutaInfo.Text = Keel.T(
                "Konto ja kõik selle andmed kustutatakse jäädavalt.",
                "Аккаунт и все его данные будут удалены безвозвратно.",
                "The account and all its data will be permanently deleted.");
            kustutaParoolSilt.Text = Keel.T("Salasõna kinnituseks", "Пароль для подтверждения", "Password to confirm");
            kustutaBtn.Text = Keel.T("Kustuta konto", "Удалить аккаунт", "Delete account");
        }

        // ---------- Teated ----------

        private static string Tiitel() => Keel.T("Minu konto", "Мой аккаунт", "My account");

        private static void Hoiatus(string tekst) =>
            MessageBox.Show(tekst, Tiitel(), MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private static void Info(string tekst) =>
            MessageBox.Show(tekst, Tiitel(), MessageBoxButtons.OK, MessageBoxIcon.Information);

        private static void AndmebaasiViga(Exception ex) =>
            MessageBox.Show(Keel.T("Viga andmebaasiga: ", "Ошибка базы данных: ", "Database error: ")
                + (ex.InnerException?.Message ?? ex.Message),
                Keel.T("Viga", "Ошибка", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static string KasutajatEiLeitud() =>
            Keel.T("Kasutajat ei leitud.", "Пользователь не найден.", "User not found.");

        private static string ValeParool() =>
            Keel.T("Vale salasõna.", "Неверный пароль.", "Wrong password.");

        // ---------- Andmed ----------

        private void LaeAndmed()
        {
            try
            {
                using var db = new AppDbContext();
                var kasutaja = Session.LaeKasutaja(db);
                nimiEntry.Text = kasutaja?.Username ?? Session.Username ?? "";
                emailEntry.Text = kasutaja?.Email ?? "";
            }
            catch (Exception ex)
            {
                AndmebaasiViga(ex);
            }
        }

        private void Salvesta_Click(object? sender, EventArgs e)
        {
            string nimi = nimiEntry.Text.Trim();
            string email = emailEntry.Text.Trim();

            if (string.IsNullOrWhiteSpace(nimi))
            {
                Hoiatus(Keel.T("Kasutajanimi ei tohi olla tühi.", "Логин не может быть пустым.", "The username cannot be empty."));
                return;
            }

            if (!EpostiSaatja.OnKorrektne(email))
            {
                Hoiatus(Keel.T("Sisesta korrektne e-posti aadress.",
                    "Введите корректный адрес электронной почты.",
                    "Enter a valid e-mail address."));
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var kasutaja = Session.LaeKasutaja(db);
                if (kasutaja == null)
                {
                    Hoiatus(KasutajatEiLeitud());
                    return;
                }

                if (db.Users.Any(u => u.Username == nimi && u.Id != kasutaja.Id))
                {
                    Hoiatus(Keel.T("Selline kasutajanimi on juba olemas.",
                        "Такой логин уже занят.",
                        "This username is already taken."));
                    return;
                }

                kasutaja.Username = nimi;
                kasutaja.Email = email;
                db.SaveChanges();

                // Sessioon ja user.txt peavad uut kasutajanime teadma
                Session.Login(kasutaja);

                nimiEntry.Text = nimi;
                emailEntry.Text = email;
                Info(Keel.T("Andmed on salvestatud.", "Данные сохранены.", "Your details have been saved."));
            }
            catch (Exception ex)
            {
                AndmebaasiViga(ex);
            }
        }

        private void Parool_Click(object? sender, EventArgs e)
        {
            string vana = vanaEntry.Text, uus = uusEntry.Text, uus2 = uus2Entry.Text;

            if (string.IsNullOrEmpty(vana) || string.IsNullOrEmpty(uus) || string.IsNullOrEmpty(uus2))
            {
                Hoiatus(Keel.T("Palun täitke kõik väljad.", "Пожалуйста, заполните все поля.", "Please fill in all fields."));
                return;
            }

            if (uus.Length < MinParool)
            {
                Hoiatus(Keel.T($"Salasõna peab olema vähemalt {MinParool} märki.",
                    $"Пароль должен содержать не менее {MinParool} символов.",
                    $"The password must be at least {MinParool} characters long."));
                return;
            }

            if (uus != uus2)
            {
                Hoiatus(Keel.T("Salasõnad ei ühti.", "Пароли не совпадают.", "The passwords do not match."));
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var kasutaja = Session.LaeKasutaja(db);
                if (kasutaja == null)
                {
                    Hoiatus(KasutajatEiLeitud());
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(vana, kasutaja.PasswordHash))
                {
                    Hoiatus(ValeParool());
                    vanaEntry.Clear();
                    vanaEntry.Focus();
                    return;
                }

                kasutaja.PasswordHash = BCrypt.Net.BCrypt.HashPassword(uus);
                db.SaveChanges();

                vanaEntry.Clear();
                uusEntry.Clear();
                uus2Entry.Clear();
                Info(Keel.T("Salasõna on vahetatud.", "Пароль изменён.", "The password has been changed."));
            }
            catch (Exception ex)
            {
                AndmebaasiViga(ex);
            }
        }

        private void Kustuta_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(kustutaParoolEntry.Text))
            {
                Hoiatus(Keel.T("Konto kustutamiseks sisesta oma salasõna.",
                    "Чтобы удалить аккаунт, введите свой пароль.",
                    "Enter your password to delete the account."));
                kustutaParoolEntry.Focus();
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var kasutaja = Session.LaeKasutaja(db);
                if (kasutaja == null)
                {
                    Hoiatus(KasutajatEiLeitud());
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(kustutaParoolEntry.Text, kasutaja.PasswordHash))
                {
                    Hoiatus(ValeParool());
                    kustutaParoolEntry.Clear();
                    kustutaParoolEntry.Focus();
                    return;
                }

                var vastus = MessageBox.Show(Keel.T(
                        $"Kas kustutada konto „{kasutaja.Username}“ jäädavalt?\nRahakott ({kasutaja.Raha:0.00} €) ja töö täiendused lähevad kaotsi. Seda ei saa tagasi võtta.",
                        $"Удалить аккаунт «{kasutaja.Username}» навсегда?\nКошелёк ({kasutaja.Raha:0.00} €) и улучшения работы будут потеряны. Это действие нельзя отменить.",
                        $"Permanently delete the account “{kasutaja.Username}”?\nThe wallet ({kasutaja.Raha:0.00} €) and work upgrades will be lost. This cannot be undone."),
                    Keel.T("Konto kustutamine", "Удаление аккаунта", "Delete account"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (vastus != DialogResult.Yes) return;

                db.Users.Remove(kasutaja);
                db.SaveChanges();

                Kustutatud = true;
                Info(Keel.T("Konto on kustutatud.", "Аккаунт удалён.", "The account has been deleted."));
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                AndmebaasiViga(ex);
            }
        }
    }
}
