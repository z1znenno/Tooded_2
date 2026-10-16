namespace Tooded_2
{
    partial class Form3
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            otsiLabel = new Label();
            otsiEntry = new TextBox();
            tootedPanel = new FlowLayoutPanel();
            leitudLabel = new Label();
            korvLabel = new Label();
            korvGrid = new DataGridView();
            eemaldaBtn = new Button();
            kokkuLabel = new Label();
            maksaBtn = new Button();
            tuhistaBtn = new Button();
            epostLabel = new Label();
            epostEntry = new TextBox();
            otsiTimer = new System.Windows.Forms.Timer(components);
            Administrator = new Button();
            ((System.ComponentModel.ISupportInitialize)korvGrid).BeginInit();
            SuspendLayout();
            // 
            // otsiLabel
            // 
            otsiLabel.AutoSize = true;
            otsiLabel.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            otsiLabel.Location = new Point(20, 20);
            otsiLabel.Name = "otsiLabel";
            otsiLabel.Size = new Size(81, 18);
            otsiLabel.TabIndex = 0;
            otsiLabel.Text = "Otsi toodet:";
            // 
            // otsiEntry
            // 
            otsiEntry.Font = new Font("Segoe UI", 10F);
            otsiEntry.Location = new Point(120, 16);
            otsiEntry.Name = "otsiEntry";
            otsiEntry.PlaceholderText = "Toote nimi või kategooria...";
            otsiEntry.Size = new Size(350, 25);
            otsiEntry.TabIndex = 1;
            otsiEntry.TextChanged += otsiEntry_TextChanged;
            // 
            // tootedPanel
            // 
            tootedPanel.AutoScroll = true;
            tootedPanel.BackColor = Color.FromArgb(245, 246, 248);
            tootedPanel.BorderStyle = BorderStyle.FixedSingle;
            tootedPanel.Location = new Point(20, 50);
            tootedPanel.Name = "tootedPanel";
            tootedPanel.Padding = new Padding(4);
            tootedPanel.Size = new Size(610, 480);
            tootedPanel.TabIndex = 2;
            // 
            // leitudLabel
            // 
            leitudLabel.AutoSize = true;
            leitudLabel.Font = new Font("Modern No. 20", 11F, FontStyle.Italic, GraphicsUnit.Point, 0);
            leitudLabel.ForeColor = Color.DimGray;
            leitudLabel.Location = new Point(20, 540);
            leitudLabel.Name = "leitudLabel";
            leitudLabel.Size = new Size(0, 17);
            leitudLabel.TabIndex = 3;
            // 
            // korvLabel
            // 
            korvLabel.AutoSize = true;
            korvLabel.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            korvLabel.Location = new Point(650, 20);
            korvLabel.Name = "korvLabel";
            korvLabel.Size = new Size(69, 18);
            korvLabel.TabIndex = 4;
            korvLabel.Text = "Ostukorv:";
            // 
            // korvGrid
            // 
            korvGrid.AllowUserToAddRows = false;
            korvGrid.AllowUserToDeleteRows = false;
            korvGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            korvGrid.BackgroundColor = Color.White;
            korvGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            korvGrid.Location = new Point(650, 50);
            korvGrid.MultiSelect = false;
            korvGrid.Name = "korvGrid";
            korvGrid.ReadOnly = true;
            korvGrid.RowHeadersVisible = false;
            korvGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            korvGrid.Size = new Size(330, 320);
            korvGrid.TabIndex = 5;
            korvGrid.DataBindingComplete += korvGrid_DataBindingComplete;
            // 
            // eemaldaBtn
            // 
            eemaldaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            eemaldaBtn.Location = new Point(650, 378);
            eemaldaBtn.Name = "eemaldaBtn";
            eemaldaBtn.Size = new Size(100, 27);
            eemaldaBtn.TabIndex = 6;
            eemaldaBtn.Text = "Eemalda";
            eemaldaBtn.UseVisualStyleBackColor = true;
            eemaldaBtn.Click += eemaldaBtn_Click;
            // 
            // kokkuLabel
            // 
            kokkuLabel.AutoSize = true;
            kokkuLabel.Font = new Font("Modern No. 20", 14F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            kokkuLabel.Location = new Point(770, 381);
            kokkuLabel.Name = "kokkuLabel";
            kokkuLabel.Size = new Size(132, 21);
            kokkuLabel.TabIndex = 7;
            kokkuLabel.Text = "Kokku: 0.00 €";
            // 
            // maksaBtn
            // 
            maksaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            maksaBtn.Location = new Point(730, 525);
            maksaBtn.Name = "maksaBtn";
            maksaBtn.Size = new Size(120, 32);
            maksaBtn.TabIndex = 8;
            maksaBtn.Text = "Maksta";
            maksaBtn.UseVisualStyleBackColor = true;
            maksaBtn.Click += maksaBtn_Click;
            // 
            // tuhistaBtn
            // 
            tuhistaBtn.DialogResult = DialogResult.Cancel;
            tuhistaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            tuhistaBtn.Location = new Point(860, 525);
            tuhistaBtn.Name = "tuhistaBtn";
            tuhistaBtn.Size = new Size(120, 32);
            tuhistaBtn.TabIndex = 9;
            tuhistaBtn.Text = "Tühista";
            tuhistaBtn.UseVisualStyleBackColor = true;
            // 
            // epostLabel
            // 
            epostLabel.AutoSize = true;
            epostLabel.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            epostLabel.Location = new Point(650, 425);
            epostLabel.Name = "epostLabel";
            epostLabel.Size = new Size(166, 18);
            epostLabel.TabIndex = 10;
            epostLabel.Text = "Teie e-post (tšeki jaoks):";
            // 
            // epostEntry
            // 
            epostEntry.Font = new Font("Segoe UI", 10F);
            epostEntry.Location = new Point(650, 448);
            epostEntry.Name = "epostEntry";
            epostEntry.PlaceholderText = "nimi@näide.ee";
            epostEntry.Size = new Size(330, 25);
            epostEntry.TabIndex = 11;
            // 
            // otsiTimer
            // 
            otsiTimer.Interval = 300;
            otsiTimer.Tick += otsiTimer_Tick;
            // 
            // Administrator
            // 
            Administrator.Font = new Font("Modern No. 20", 11.9999981F, FontStyle.Italic, GraphicsUnit.Point, 0);
            Administrator.Location = new Point(500, 18);
            Administrator.Name = "Administrator";
            Administrator.Size = new Size(118, 23);
            Administrator.TabIndex = 12;
            Administrator.Text = "Admin Panel";
            Administrator.UseVisualStyleBackColor = true;
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = tuhistaBtn;
            ClientSize = new Size(1000, 570);
            Controls.Add(Administrator);
            Controls.Add(epostEntry);
            Controls.Add(epostLabel);
            Controls.Add(tuhistaBtn);
            Controls.Add(maksaBtn);
            Controls.Add(kokkuLabel);
            Controls.Add(eemaldaBtn);
            Controls.Add(korvGrid);
            Controls.Add(korvLabel);
            Controls.Add(leitudLabel);
            Controls.Add(tootedPanel);
            Controls.Add(otsiEntry);
            Controls.Add(otsiLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form3";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Pood";
            Load += Form3_Load;
            ((System.ComponentModel.ISupportInitialize)korvGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label otsiLabel;
        private TextBox otsiEntry;
        private FlowLayoutPanel tootedPanel;
        private Label leitudLabel;
        private Label korvLabel;
        private DataGridView korvGrid;
        private Button eemaldaBtn;
        private Label kokkuLabel;
        private Button maksaBtn;
        private Button tuhistaBtn;
        private Label epostLabel;
        private TextBox epostEntry;
        private System.Windows.Forms.Timer otsiTimer;
        private Button Administrator;
    }
}