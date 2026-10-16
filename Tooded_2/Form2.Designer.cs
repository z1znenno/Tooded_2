namespace Tooded_2
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            nimetus = new Label();
            kogus = new Label();
            hind = new Label();
            kategooria = new Label();
            toodeEntry = new TextBox();
            kogusEntry = new TextBox();
            hindEntry = new TextBox();
            kategooriaBox = new ComboBox();
            imageList1 = new ImageList(components);
            lisaKategooriaBtn = new Button();
            kustutaKategooriaBtn = new Button();
            otsiFailBtn = new Button();
            toodePilt = new PictureBox();
            lisaBtn = new Button();
            kustutaBtn = new Button();
            uuendaBtn = new Button();
            puhastaBtn = new Button();
            poodBtn = new Button();
            ostuInfoLabel = new Label();
            dataGridView1 = new DataGridView();
            label1 = new Label();
            kategooriaEntry = new TextBox();
            ((System.ComponentModel.ISupportInitialize)toodePilt).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // nimetus
            // 
            nimetus.AutoSize = true;
            nimetus.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            nimetus.Location = new Point(46, 35);
            nimetus.Margin = new Padding(4, 0, 4, 0);
            nimetus.Name = "nimetus";
            nimetus.Size = new Size(111, 18);
            nimetus.TabIndex = 0;
            nimetus.Text = "Toode nimetus: ";
            // 
            // kogus
            // 
            kogus.AutoSize = true;
            kogus.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            kogus.Location = new Point(46, 80);
            kogus.Margin = new Padding(4, 0, 4, 0);
            kogus.Name = "kogus";
            kogus.Size = new Size(57, 18);
            kogus.TabIndex = 1;
            kogus.Text = "Kogus: ";
            // 
            // hind
            // 
            hind.AutoSize = true;
            hind.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            hind.Location = new Point(46, 121);
            hind.Margin = new Padding(4, 0, 4, 0);
            hind.Name = "hind";
            hind.Size = new Size(52, 18);
            hind.TabIndex = 2;
            hind.Text = "Hind: ";
            // 
            // kategooria
            // 
            kategooria.AutoSize = true;
            kategooria.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            kategooria.Location = new Point(49, 162);
            kategooria.Margin = new Padding(4, 0, 4, 0);
            kategooria.Name = "kategooria";
            kategooria.Size = new Size(94, 18);
            kategooria.TabIndex = 3;
            kategooria.Text = "Kategooriad: ";
            // 
            // toodeEntry
            // 
            toodeEntry.Location = new Point(195, 32);
            toodeEntry.Margin = new Padding(4, 3, 4, 3);
            toodeEntry.Name = "toodeEntry";
            toodeEntry.Size = new Size(116, 23);
            toodeEntry.TabIndex = 4;
            // 
            // kogusEntry
            // 
            kogusEntry.Location = new Point(195, 76);
            kogusEntry.Margin = new Padding(4, 3, 4, 3);
            kogusEntry.Name = "kogusEntry";
            kogusEntry.Size = new Size(116, 23);
            kogusEntry.TabIndex = 5;
            // 
            // hindEntry
            // 
            hindEntry.Location = new Point(195, 118);
            hindEntry.Margin = new Padding(4, 3, 4, 3);
            hindEntry.Name = "hindEntry";
            hindEntry.Size = new Size(116, 23);
            hindEntry.TabIndex = 6;
            // 
            // kategooriaBox
            // 
            kategooriaBox.DropDownStyle = ComboBoxStyle.DropDownList;
            kategooriaBox.FormattingEnabled = true;
            kategooriaBox.Location = new Point(195, 157);
            kategooriaBox.Margin = new Padding(4, 3, 4, 3);
            kategooriaBox.Name = "kategooriaBox";
            kategooriaBox.Size = new Size(140, 23);
            kategooriaBox.TabIndex = 7;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth8Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // lisaKategooriaBtn
            // 
            lisaKategooriaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lisaKategooriaBtn.Location = new Point(140, 219);
            lisaKategooriaBtn.Margin = new Padding(4, 3, 4, 3);
            lisaKategooriaBtn.Name = "lisaKategooriaBtn";
            lisaKategooriaBtn.Size = new Size(172, 27);
            lisaKategooriaBtn.TabIndex = 8;
            lisaKategooriaBtn.Text = "Lisa kategooriad";
            lisaKategooriaBtn.UseVisualStyleBackColor = true;
            lisaKategooriaBtn.Click += btnLisaKategooria_Click;
            // 
            // kustutaKategooriaBtn
            // 
            kustutaKategooriaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            kustutaKategooriaBtn.Location = new Point(318, 219);
            kustutaKategooriaBtn.Margin = new Padding(4, 3, 4, 3);
            kustutaKategooriaBtn.Name = "kustutaKategooriaBtn";
            kustutaKategooriaBtn.Size = new Size(172, 27);
            kustutaKategooriaBtn.TabIndex = 9;
            kustutaKategooriaBtn.Text = "Kustuta kategooriad";
            kustutaKategooriaBtn.UseVisualStyleBackColor = true;
            kustutaKategooriaBtn.Click += btnKustutaKategooria_Click;
            // 
            // otsiFailBtn
            // 
            otsiFailBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            otsiFailBtn.Location = new Point(524, 233);
            otsiFailBtn.Margin = new Padding(4, 3, 4, 3);
            otsiFailBtn.Name = "otsiFailBtn";
            otsiFailBtn.Size = new Size(88, 27);
            otsiFailBtn.TabIndex = 10;
            otsiFailBtn.Text = "Otsi pilt";
            otsiFailBtn.UseVisualStyleBackColor = true;
            otsiFailBtn.Click += btnValiPilt_Click;
            // 
            // toodePilt
            // 
            toodePilt.Location = new Point(524, 32);
            toodePilt.Margin = new Padding(4, 3, 4, 3);
            toodePilt.Name = "toodePilt";
            toodePilt.Size = new Size(336, 194);
            toodePilt.TabIndex = 11;
            toodePilt.TabStop = false;
            // 
            // lisaBtn
            // 
            lisaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lisaBtn.Location = new Point(140, 265);
            lisaBtn.Margin = new Padding(4, 3, 4, 3);
            lisaBtn.Name = "lisaBtn";
            lisaBtn.Size = new Size(62, 27);
            lisaBtn.TabIndex = 15;
            lisaBtn.Text = "Lisa";
            lisaBtn.UseVisualStyleBackColor = true;
            lisaBtn.Click += btnLisa_Click;
            // 
            // kustutaBtn
            // 
            kustutaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            kustutaBtn.Location = new Point(293, 265);
            kustutaBtn.Margin = new Padding(4, 3, 4, 3);
            kustutaBtn.Name = "kustutaBtn";
            kustutaBtn.Size = new Size(88, 27);
            kustutaBtn.TabIndex = 16;
            kustutaBtn.Text = "Kustuta";
            kustutaBtn.UseVisualStyleBackColor = true;
            kustutaBtn.Click += btnKustuta_Click;
            // 
            // uuendaBtn
            // 
            uuendaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            uuendaBtn.Location = new Point(209, 265);
            uuendaBtn.Margin = new Padding(4, 3, 4, 3);
            uuendaBtn.Name = "uuendaBtn";
            uuendaBtn.Size = new Size(77, 27);
            uuendaBtn.TabIndex = 17;
            uuendaBtn.Text = "Uuenda";
            uuendaBtn.UseVisualStyleBackColor = true;
            uuendaBtn.Click += btnUuenda_Click;
            // 
            // puhastaBtn
            // 
            puhastaBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            puhastaBtn.Location = new Point(387, 265);
            puhastaBtn.Margin = new Padding(4, 3, 4, 3);
            puhastaBtn.Name = "puhastaBtn";
            puhastaBtn.Size = new Size(88, 27);
            puhastaBtn.TabIndex = 18;
            puhastaBtn.Text = "Puhasta";
            puhastaBtn.UseVisualStyleBackColor = true;
            // 
            // poodBtn
            // 
            poodBtn.Font = new Font("Modern No. 20", 12F, FontStyle.Italic, GraphicsUnit.Point, 0);
            poodBtn.Location = new Point(620, 233);
            poodBtn.Margin = new Padding(4, 3, 4, 3);
            poodBtn.Name = "poodBtn";
            poodBtn.Size = new Size(88, 27);
            poodBtn.TabIndex = 20;
            poodBtn.Text = "Pood";
            poodBtn.UseVisualStyleBackColor = true;
            poodBtn.Click += poodBtn_Click;
            // 
            // ostuInfoLabel
            // 
            ostuInfoLabel.AutoEllipsis = true;
            ostuInfoLabel.Font = new Font("Modern No. 20", 10F, FontStyle.Italic, GraphicsUnit.Point, 0);
            ostuInfoLabel.Location = new Point(714, 232);
            ostuInfoLabel.Margin = new Padding(4, 0, 4, 0);
            ostuInfoLabel.Name = "ostuInfoLabel";
            ostuInfoLabel.Size = new Size(212, 64);
            ostuInfoLabel.TabIndex = 21;
            ostuInfoLabel.Text = "Ostetud: 0 tk\r\n—\r\nKokku: 0.00 €";
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(140, 300);
            dataGridView1.Margin = new Padding(4, 3, 4, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(751, 215);
            dataGridView1.TabIndex = 22;
            dataGridView1.CellClick += dataGridViewTooted_CellClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Modern No. 20", 12F, FontStyle.Italic);
            label1.Location = new Point(49, 198);
            label1.Name = "label1";
            label1.Size = new Size(112, 18);
            label1.TabIndex = 23;
            label1.Text = "Uus kategooria: ";
            // 
            // kategooriaEntry
            // 
            kategooriaEntry.Location = new Point(195, 193);
            kategooriaEntry.Name = "kategooriaEntry";
            kategooriaEntry.Size = new Size(100, 23);
            kategooriaEntry.TabIndex = 24;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(933, 519);
            Controls.Add(kategooriaEntry);
            Controls.Add(label1);
            Controls.Add(dataGridView1);
            Controls.Add(ostuInfoLabel);
            Controls.Add(poodBtn);
            Controls.Add(puhastaBtn);
            Controls.Add(uuendaBtn);
            Controls.Add(kustutaBtn);
            Controls.Add(lisaBtn);
            Controls.Add(toodePilt);
            Controls.Add(otsiFailBtn);
            Controls.Add(kustutaKategooriaBtn);
            Controls.Add(lisaKategooriaBtn);
            Controls.Add(kategooriaBox);
            Controls.Add(hindEntry);
            Controls.Add(kogusEntry);
            Controls.Add(toodeEntry);
            Controls.Add(kategooria);
            Controls.Add(hind);
            Controls.Add(kogus);
            Controls.Add(nimetus);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form2";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)toodePilt).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nimetus;
        private System.Windows.Forms.Label kogus;
        private System.Windows.Forms.Label hind;
        private System.Windows.Forms.Label kategooria;
        private System.Windows.Forms.TextBox toodeEntry;
        private System.Windows.Forms.TextBox kogusEntry;
        private System.Windows.Forms.TextBox hindEntry;
        private System.Windows.Forms.ComboBox kategooriaBox;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button lisaKategooriaBtn;
        private System.Windows.Forms.Button kustutaKategooriaBtn;
        private System.Windows.Forms.Button otsiFailBtn;
        private System.Windows.Forms.PictureBox toodePilt;
        private System.Windows.Forms.Button lisaBtn;
        private System.Windows.Forms.Button kustutaBtn;
        private System.Windows.Forms.Button uuendaBtn;
        private System.Windows.Forms.Button puhastaBtn;
        private System.Windows.Forms.Button poodBtn;
        private System.Windows.Forms.Label ostuInfoLabel;
        private System.Windows.Forms.DataGridView dataGridView1;
        private Label label1;
        private TextBox kategooriaEntry;
    }
}