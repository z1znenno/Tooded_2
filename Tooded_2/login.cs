namespace Tooded_2;

public partial class Login : Form
{
    private readonly AppDbContext _context;
    private const string AdminKood = "admin123";

    Button loginBtn, regBtn;
    TabControl tabs;
    TabPage login, reg;

    // Sisselogimise väljad
    TextBox loginUserEntry, loginPassEntry;

    // Registreerimise väljad (iga kontroll saab olla ainult ühes kohas!)
    TextBox regUserEntry, regPassEntry, regPass2Entry, adminPassEntry, emailEntry, appPassEntry;
    ComboBox roleCombo;
    Label adminPassLabel;

    string[] roles = new string[] { "Ostja", "Admin" };

    public static Font font = new Font("Segoe Script", 9F, FontStyle.Bold);

    public Login()
    {
        _context = new AppDbContext();
        InitializeComponent();
        ClientSize = new Size(400, 450);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Login";
        Font = font;

        tabs = new TabControl { Dock = DockStyle.Fill };
        login = new TabPage("Logi sisse");
        reg = new TabPage("Registreeri");
        tabs.Controls.Add(login);
        tabs.Controls.Add(reg);
        Controls.Add(tabs);

        // ---------- Logi sisse ----------
        login.Controls.Add(LooLabel("Kasutajanimi:", 20));
        loginUserEntry = LooTextBox(17, false);
        login.Controls.Add(loginUserEntry);

        login.Controls.Add(LooLabel("Salasõna:", 70));
        loginPassEntry = LooTextBox(67, true);
        login.Controls.Add(loginPassEntry);

        loginBtn = LooNupp("Logi sisse", 140);
        loginBtn.Click += login_BtnClick;
        login.Controls.Add(loginBtn);

        // ---------- Registreeri ----------
        reg.Controls.Add(LooLabel("Kasutajanimi:", 20));
        regUserEntry = LooTextBox(17, false);
        reg.Controls.Add(regUserEntry);

        reg.Controls.Add(LooLabel("Salasõna:", 65));
        regPassEntry = LooTextBox(62, true);
        reg.Controls.Add(regPassEntry);

        reg.Controls.Add(LooLabel("Korda sala:", 110));
        regPass2Entry = LooTextBox(107, true);
        reg.Controls.Add(regPass2Entry);

        reg.Controls.Add(LooLabel("E-post:", 155));
        emailEntry = LooTextBox(152, false);
        emailEntry.PlaceholderText = "nimi@gmail.com";
        reg.Controls.Add(emailEntry);

        reg.Controls.Add(LooLabel("Rak. parool:", 200));
        appPassEntry = LooTextBox(197, true);
        appPassEntry.PlaceholderText = "xxxx xxxx xxxx xxxx";
        reg.Controls.Add(appPassEntry);

        var appPassInfo = new LinkLabel
        {
            Text = "Gmaili rakenduse parool (tšekkide saatmiseks). Kuidas saada?",
            Font = new Font("Segoe UI", 8F),
            Location = new Point(10, 225),
            Size = new Size(380, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };
        appPassInfo.LinkArea = new LinkArea(appPassInfo.Text.IndexOf("Kuidas"), "Kuidas saada?".Length);
        appPassInfo.LinkClicked += (s, e) => System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo("https://myaccount.google.com/apppasswords") { UseShellExecute = true });
        reg.Controls.Add(appPassInfo);

        reg.Controls.Add(LooLabel("Roll:", 255));
        roleCombo = new ComboBox
        {
            Width = 200,
            Font = font,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(150, 252)
        };
        roleCombo.Items.AddRange(roles);
        roleCombo.SelectedIndex = 0;
        roleCombo.SelectedIndexChanged += (s, e) => UuendaAdminVäli();
        reg.Controls.Add(roleCombo);

        adminPassLabel = LooLabel("Admin kood:", 300);
        reg.Controls.Add(adminPassLabel);
        adminPassEntry = LooTextBox(297, true);
        reg.Controls.Add(adminPassEntry);

        regBtn = LooNupp("Registreeri", 350);
        regBtn.Click += registration_btnClick;
        reg.Controls.Add(regBtn);

        UuendaAdminVäli();

        // Enter vajutamine käivitab aktiivse vahelehe nupu
        AcceptButton = loginBtn;
        tabs.SelectedIndexChanged += (s, e) =>
            AcceptButton = tabs.SelectedTab == login ? loginBtn : regBtn;
    }

    // ---------- Abimeetodid UI jaoks ----------

    private Label LooLabel(string tekst, int y) => new Label
    {
        Text = tekst,
        Font = font,
        Width = 135,
        TextAlign = ContentAlignment.MiddleRight,
        Location = new Point(5, y - 3),
        Height = 25
    };

    private TextBox LooTextBox(int y, bool parool) => new TextBox
    {
        Font = font,
        Width = 200,
        Location = new Point(150, y),
        UseSystemPasswordChar = parool
    };

    private Button LooNupp(string tekst, int y)
    {
        var nupp = new Button { Text = tekst, Font = font, Width = 150, Height = 32 };
        nupp.Location = new Point((392 - nupp.Width) / 2, y);
        return nupp;
    }

    private void UuendaAdminVäli()
    {
        bool admin = roleCombo.SelectedItem?.ToString() == "Admin";
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
        string role = roleCombo.SelectedItem?.ToString() ?? "";
        string adminPass = adminPassEntry.Text;
        string email = emailEntry.Text.Trim();
        string appPass = appPassEntry.Text.Replace(" ", "");

        if (!ValidateRegistrationInput(username, password, password2, role, adminPass))
            return;

        if (!EpostiSaatja.OnKorrektne(email))
        {
            MessageBox.Show("Sisesta korrektne e-posti aadress.");
            return;
        }

        if (appPass.Length > 0 && appPass.Length != 16)
        {
            MessageBox.Show("Gmaili rakenduse parool on 16 tähemärki pikk.");
            return;
        }

        if (appPass.Length == 0 &&
            MessageBox.Show((EpostiSaatja.PoeEpostSeadistatud
                    ? "Rakenduse parool on tühi – tšekid saadetakse siis poe e-postilt."
                    : "Rakenduse parool on tühi – tšekke ei saa e-postiga saata.") + "\nKas jätkata?",
                "Rakenduse parool", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                Email = email,
                EpostiParool = ParooliKaitse.Krupteeri(appPass)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            MessageBox.Show("Registreerimine õnnestus!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LogiSisse(user);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga andmebaasiga: " + (ex.InnerException?.Message ?? ex.Message),
                "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateRegistrationInput(string username, string password, string password2, string role, string adminPass)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
        {
            MessageBox.Show("Palun täitke kõik väljad.");
            return false;
        }

        if (password.Length < 4)
        {
            MessageBox.Show("Salasõna peab olema vähemalt 4 märki.");
            return false;
        }

        if (password != password2)
        {
            MessageBox.Show("Salasõnad ei ühti.");
            return false;
        }

        if (role == "Admin" && adminPass != AdminKood)
        {
            MessageBox.Show("Vale administraatori kood.");
            return false;
        }

        if (_context.Users.Any(u => u.Username == username))
        {
            MessageBox.Show("Selline kasutajanimi on juba olemas.");
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
            MessageBox.Show("Palun täitke kõik väljad.");
            return;
        }

        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                MessageBox.Show("Vale kasutajanimi või salasõna.", "Viga", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                loginPassEntry.Clear();
                return;
            }

            LogiSisse(user);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga andmebaasiga: " + (ex.InnerException?.Message ?? ex.Message),
                "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
