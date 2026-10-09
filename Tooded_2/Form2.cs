using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Tooded_2
{
    public partial class Form2 : Form
    {
        private readonly AppDbContext _context;
        private string valitudPilt;

        public Form2()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LaeKategooriad();
            LaeTooted();
        }

        private void LaeKategooriad()
        {
            kategooriaBox.DataSource = _context.Kategooriatabel.ToList();
            kategooriaBox.DisplayMember = "nimetus";
            kategooriaBox.ValueMember = "Id";
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
                    Kategooria = t.Kategooria.nimetus,
                    t.Pilt
                })
                .ToList();
        }

        private void btnValiPilt_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Pildid|*.jpg;*.png;*.jpeg";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                valitudPilt = ofd.FileName;
                toodePilt.Image = Image.FromFile(valitudPilt);
            }
        }
    }
}
