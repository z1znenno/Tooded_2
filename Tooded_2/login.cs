namespace Tooded_2;

public partial class Login : Form
{
    private readonly AppDbContext _context;
    private const string AdminKood = "admin123";

    TeemaNupp loginBtn, regBtn, loginTab, regTab;
    Panel login, reg, tabJoon;

    // Sisselogimise väljad
    TextBox loginUserEntry, loginPassEntry;
    Label tereSilt, loginUserSilt, loginPassSilt;
    LinkLabel uusKonto;

    // Registreerimise väljad (iga kontroll saab olla ainult ühes kohas!)
    TextBox regUserEntry, regPassEntry, regPass2Entry, adminPassEntry, emailEntry;
    ComboBox roleCombo;
    Label regUserSilt, regPassSilt, regPass2Silt, emailSilt, roleSilt, adminPassLabel;

    // Andmebaasi salvestatavad rollid – neid ei tõlgita (valikus näidatakse tõlgitud nimesid)
    string[] roles = new string[] { "Ostja", "Admin" };

    // true, kuni RakendaKeel vahetab rollivaliku tekste (valik ise ei muutu)
    bool rolleUuendatakse;

    public Login()
    {
        _context = new AppDbContext();
        InitializeComponent();
        Teema.Vorm(this, new Size(500, 540));
        StartPosition = FormStartPosition.CenterScreen;

        // ---------- Valge kaart päise all ----------
        var kaart = new KaartPaneel
        {
            Location = new Point(20, 84),
            Size = new Size(460, 436),
            Padding = new Padding(0)
        };

        // Vahelehed: kaks teksti-nuppu ja oranž joon aktiivse all
        loginTab = LooTab(0);
        regTab = LooTab(230);
        tabJoon = new Panel { BackColor = Teema.Aktsent, Size = new Size(230, 3), Location = new Point(0, 49) };
        var tabAlus = new Panel { BackColor = Teema.Piir, Size = new Size(460, 1), Location = new Point(0, 51) };
        kaart.Controls.Add(tabJoon);
        kaart.Controls.Add(tabAlus);
        kaart.Controls.Add(loginTab);
        kaart.Controls.Add(regTab);

        login = new Panel { Location = new Point(1, 56), Size = new Size(458, 376), BackColor = Color.White };
        reg = new Panel { Location = new Point(1, 56), Size = new Size(458, 376), BackColor = Color.White };
        kaart.Controls.Add(login);
        kaart.Controls.Add(reg);

        // ---------- Logi sisse (sildid sisendi kohal, nagu kaup24) ----------
        tereSilt = Teema.Silt("", Teema.Pealkiri);
        tereSilt.Location = new Point(24, 16);
        login.Controls.Add(tereSilt);

        loginUserSilt = PealSilt(60);
        login.Controls.Add(loginUserSilt);
        loginUserEntry = LooTextBox(82, false);
        loginUserEntry.Width = 410;
        loginUserEntry.Left = 24;
        login.Controls.Add(loginUserEntry);

        loginPassSilt = PealSilt(124);
        login.Controls.Add(loginPassSilt);
        loginPassEntry = LooTextBox(146, true);
        loginPassEntry.Width = 410;
        loginPassEntry.Left = 24;
        login.Controls.Add(loginPassEntry);

        loginBtn = LooNupp(200);
        loginBtn.Click += login_BtnClick;
        login.Controls.Add(loginBtn);

        uusKonto = new LinkLabel
        {
            Font = Teema.Tavaline,
            LinkColor = Teema.Link,
            ActiveLinkColor = Teema.Aktsent,
            LinkBehavior = LinkBehavior.HoverUnderline,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 256),
            Size = new Size(410, 24)
        };
        uusKonto.LinkClicked += (s, e) => NaitaTab(false);
        login.Controls.Add(uusKonto);

        // ---------- Registreeri (sildid vasakul, et kõik mahuks) ----------
        regUserSilt = LooLabel(14);
        reg.Controls.Add(regUserSilt);
        regUserEntry = LooTextBox(14, false);
        reg.Controls.Add(regUserEntry);

        regPassSilt = LooLabel(52);
        reg.Controls.Add(regPassSilt);
        regPassEntry = LooTextBox(52, true);
        reg.Controls.Add(regPassEntry);

        regPass2Silt = LooLabel(90);
        reg.Controls.Add(regPass2Silt);
        regPass2Entry = LooTextBox(90, true);
        reg.Controls.Add(regPass2Entry);

        emailSilt = LooLabel(128);
        reg.Controls.Add(emailSilt);
        emailEntry = LooTextBox(128, false);
        reg.Controls.Add(emailEntry);

        roleSilt = LooLabel(166);
        reg.Controls.Add(roleSilt);
        roleCombo = new ComboBox
        {
            Width = 248,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(186, 166)
        };
        Teema.Valik(roleCombo);
        roleCombo.Items.AddRange(roles);
        roleCombo.SelectedIndex = 0;
        roleCombo.SelectedIndexChanged += (s, e) => UuendaAdminVäli();
        reg.Controls.Add(roleCombo);

        adminPassLabel = LooLabel(204);
        reg.Controls.Add(adminPassLabel);
        adminPassEntry = LooTextBox(204, true);
        reg.Controls.Add(adminPassEntry);

        regBtn = LooNupp(250);
        regBtn.Click += registration_btnClick;
        reg.Controls.Add(regBtn);

        UuendaAdminVäli();

        // Päis: logo vasakul, keelevalik paremal
        var pais = Teema.LooPais(this);
        var keeleValik = new KeeleValik { Anchor = AnchorStyles.Top | AnchorStyles.Right };
        keeleValik.Location = new Point(pais.Width - keeleValik.Width - 18, (pais.Height - keeleValik.Height) / 2);
        pais.Controls.Add(keeleValik);

        Controls.Add(kaart);
        Controls.Add(pais);

        NaitaTab(true);

        Keel.Jalgi(this, RakendaKeel);
    }

    // ---------- Keel ----------

    /// <summary>Seab kõik vormi tekstid praeguses keeles (käivitub ka keele vahetamisel).</summary>
    private void RakendaKeel()
    {
        Text = $"{TsekiKoostaja.PoeNimi} – " + Keel.T("sisselogimine", "вход", "sign in");

        loginTab.Text = Keel.T("Logi sisse", "Вход", "Log in");
        regTab.Text = Keel.T("Registreeri", "Регистрация", "Sign up");

        // Logi sisse
        tereSilt.Text = Keel.T("Tere tulemast tagasi!", "С возвращением!", "Welcome back!");
        loginUserSilt.Text = Keel.T("Kasutajanimi", "Логин", "Username");
        loginPassSilt.Text = Keel.T("Salasõna", "Пароль", "Password");
        loginBtn.Text = Keel.T("Logi sisse", "Войти", "Log in");
        SeaLink(uusKonto,
            Keel.T("Pole veel kontot? Registreeru", "Ещё нет аккаунта? Зарегистрируйтесь", "No account yet? Sign up"),
            Keel.T("Registreeru", "Зарегистрируйтесь", "Sign up"));

        // Registreeri
        regUserSilt.Text = Keel.T("Kasutajanimi", "Логин", "Username");
        regPassSilt.Text = Keel.T("Salasõna", "Пароль", "Password");
        regPass2Silt.Text = Keel.T("Korda salasõna", "Повтор пароля", "Repeat password");
        emailSilt.Text = Keel.T("E-post", "Эл. почта", "E-mail");
        emailEntry.PlaceholderText = Keel.T("nimi@gmail.com", "name@gmail.com", "name@gmail.com");
        roleSilt.Text = Keel.T("Roll", "Роль", "Role");
        adminPassLabel.Text = Keel.T("Admin kood", "Код админа", "Admin code");
        regBtn.Text = Keel.T("Registreeri", "Зарегистрироваться", "Sign up");

        // Rollivalik: tekstid tõlgitakse, valitud rida jääb samaks (andmebaasi läheb roles[indeks])
        int valitud = roleCombo.SelectedIndex;
        rolleUuendatakse = true;
        try
        {
            roleCombo.BeginUpdate();
            roleCombo.Items.Clear();
            roleCombo.Items.Add(Keel.T("Ostja", "Покупатель", "Buyer"));
            roleCombo.Items.Add(Keel.T("Admin", "Администратор", "Admin"));
            roleCombo.SelectedIndex = valitud >= 0 ? valitud : 0;
            roleCombo.EndUpdate();
        }
        finally
        {
            rolleUuendatakse = false;
        }
    }

    /// <summary>Seab lingisildi teksti ja märgib lingiks ainult osa "link".</summary>
    private static void SeaLink(LinkLabel silt, string tekst, string link)
    {
        silt.LinkArea = new LinkArea(0, 0);
        silt.Text = tekst;
        int algus = tekst.IndexOf(link, StringComparison.Ordinal);
        if (algus >= 0) silt.LinkArea = new LinkArea(algus, link.Length);
    }

    // ---------- Abimeetodid UI jaoks ----------

    private TeemaNupp LooTab(int x)
    {
        var tab = new TeemaNupp
        {
            Stiil = NupuStiil.Teine,
            Raadius = 0,
            Font = new Font(Teema.FondiNimi, 11F, FontStyle.Bold),
            Location = new Point(x + 1, 1),
            Size = new Size(228, 48)
        };
        tab.Click += (s, e) => NaitaTab(tab == loginTab);
        return tab;
    }

    /// <summary>Näitab kas sisselogimise (true) või registreerimise (false) vahelehte.</summary>
    private void NaitaTab(bool sisse)
    {
        login.Visible = sisse;
        reg.Visible = !sisse;
        tabJoon.Left = sisse ? 0 : 230;
        loginTab.ForeColor = sisse ? Teema.Tekst : Teema.TekstHall;
        regTab.ForeColor = sisse ? Teema.TekstHall : Teema.Tekst;
        // Enter vajutamine käivitab aktiivse vahelehe nupu
        AcceptButton = sisse ? loginBtn : regBtn;
        (sisse ? loginUserEntry : regUserEntry).Focus();
    }

    private static Label PealSilt(int y) => new Label
    {
        Font = Teema.Paks,
        ForeColor = Teema.Tekst,
        AutoSize = true,
        Location = new Point(22, y)
    };

    // Silt on piisavalt lai ka pikema vene/inglise teksti jaoks
    private static Label LooLabel(int y) => new Label
    {
        Font = Teema.Tavaline,
        ForeColor = Teema.TekstHall,
        TextAlign = ContentAlignment.MiddleLeft,
        Location = new Point(22, y),
        Size = new Size(162, 26)
    };

    private static TextBox LooTextBox(int y, bool parool)
    {
        var t = new TextBox
        {
            Width = 248,
            Location = new Point(186, y),
            UseSystemPasswordChar = parool
        };
        Teema.Sisestus(t);
        return t;
    }

    private static TeemaNupp LooNupp(int y) => new TeemaNupp
    {
        Font = new Font(Teema.FondiNimi, 11F, FontStyle.Bold),
        Location = new Point(24, y),
        Size = new Size(410, 44)
    };

    /// <summary>Valitud roll andmebaasi kujul ("Ostja" / "Admin"), sõltumata keelest.</summary>
    private string ValitudRoll() =>
        roleCombo.SelectedIndex >= 0 && roleCombo.SelectedIndex < roles.Length ? roles[roleCombo.SelectedIndex] : "";

    private void UuendaAdminVäli()
    {
        if (rolleUuendatakse) return;
        bool admin = ValitudRoll() == "Admin";
        adminPassLabel.Visible = admin;
        adminPassEntry.Visible = admin;
        if (!admin) adminPassEntry.Clear();
    }

    // ---------- Registreerimine ----------

    private void registration_btnClick(object? sender, EventArgs e)
    {
        string username = regUserEntry.Text.Trim();
        string password = regPassEntry.Text;
        string password2 = regPass2Entry.Text;
        string role = ValitudRoll();
        string adminPass = adminPassEntry.Text;
        string email = emailEntry.Text.Trim();

        if (!ValidateRegistrationInput(username, password, password2, role, adminPass))
            return;

        if (!EpostiSaatja.OnKorrektne(email))
        {
            MessageBox.Show(Keel.T("Sisesta korrektne e-posti aadress.",
                "Введите корректный адрес электронной почты.",
                "Enter a valid e-mail address."));
            return;
        }

        try
        {
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                Email = email
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            MessageBox.Show(Keel.T("Registreerimine õnnestus!", "Регистрация прошла успешно!", "Registration successful!"),
                "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LogiSisse(user);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Keel.T("Viga andmebaasiga: ", "Ошибка базы данных: ", "Database error: ")
                + (ex.InnerException?.Message ?? ex.Message),
                Keel.T("Viga", "Ошибка", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateRegistrationInput(string username, string password, string password2, string role, string adminPass)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
        {
            MessageBox.Show(Keel.T("Palun täitke kõik väljad.", "Пожалуйста, заполните все поля.", "Please fill in all fields."));
            return false;
        }

        if (password.Length < 4)
        {
            MessageBox.Show(Keel.T("Salasõna peab olema vähemalt 4 märki.",
                "Пароль должен содержать не менее 4 символов.",
                "The password must be at least 4 characters long."));
            return false;
        }

        if (password != password2)
        {
            MessageBox.Show(Keel.T("Salasõnad ei ühti.", "Пароли не совпадают.", "The passwords do not match."));
            return false;
        }

        if (role == "Admin" && adminPass != AdminKood)
        {
            MessageBox.Show(Keel.T("Vale administraatori kood.", "Неверный код администратора.", "Wrong administrator code."));
            return false;
        }

        if (_context.Users.Any(u => u.Username == username))
        {
            MessageBox.Show(Keel.T("Selline kasutajanimi on juba olemas.",
                "Такой логин уже занят.",
                "This username is already taken."));
            return false;
        }

        return true;
    }

    // ---------- Sisselogimine ----------

    private void login_BtnClick(object? sender, EventArgs e)
    {
        string username = loginUserEntry.Text.Trim();
        string password = loginPassEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show(Keel.T("Palun täitke kõik väljad.", "Пожалуйста, заполните все поля.", "Please fill in all fields."));
            return;
        }

        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                MessageBox.Show(Keel.T("Vale kasutajanimi või salasõna.",
                    "Неверный логин или пароль.",
                    "Wrong username or password."),
                    Keel.T("Viga", "Ошибка", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                loginPassEntry.Clear();
                return;
            }

            LogiSisse(user);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Keel.T("Viga andmebaasiga: ", "Ошибка базы данных: ", "Database error: ")
                + (ex.InnerException?.Message ?? ex.Message),
                Keel.T("Viga", "Ошибка", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Salvestab sessiooni ja sulgeb vormi tulemusega OK – Program.cs avab siis Avavormi
    private void LogiSisse(User user)
    {
        Session.Login(user);
        DialogResult = DialogResult.OK;
        Close();
    }
}
