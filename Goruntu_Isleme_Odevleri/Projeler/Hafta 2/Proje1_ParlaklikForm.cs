using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_ParlaklikForm : Form
    {
        private PictureBox pcbResim;
        private TrackBar tbParlaklik;
        private Button btnYukle, btnGeri;
        private Label lblParlaklik;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje1_ParlaklikForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 1: Parlaklık Ayarlama";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(500, 400),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            lblParlaklik = new Label()
            {
                Text = "Parlaklık: 0",
                Location = new Point(25, 440),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            tbParlaklik = new TrackBar()
            {
                Location = new Point(20, 460),
                Size = new Size(505, 45),
                Minimum = -100,
                Maximum = 100,
                Value = 0,
                TickFrequency = 10,
                Enabled = false // Resim yüklenene kadar pasif
            };
            tbParlaklik.Scroll += new EventHandler(tbParlaklik_Scroll);

            btnYukle = new Button()
            {
                Text = "Resim Yükle",
                Location = new Point(25, 520),
                Size = new Size(150, 40)
            };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button()
            {
                Text = "Hafta Menüsüne Dön",
                Location = new Point(this.ClientSize.Width - 175, 520),
                Size = new Size(150, 40),
                BackColor = Color.LightCoral
            };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, lblParlaklik, tbParlaklik, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje1_ParlaklikForm";
            this.Size = new Size(560, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void btnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(dialog.FileName);
                pcbResim.Image = originalBitmap;
                tbParlaklik.Enabled = true;
                tbParlaklik.Value = 0;
                lblParlaklik.Text = "Parlaklık: 0";
            }
        }

        private void tbParlaklik_Scroll(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            lblParlaklik.Text = $"Parlaklık: {tbParlaklik.Value}";

            Bitmap parlakResim = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            int parlaklikDegeri = tbParlaklik.Value;

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);

                    int r = originalColor.R + parlaklikDegeri;
                    int g = originalColor.G + parlaklikDegeri;
                    int b = originalColor.B + parlaklikDegeri;

                    // Değerlerin 0-255 aralığında kalmasını sağla (taşmayı önle)
                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    parlakResim.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            pcbResim.Image = parlakResim;
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }
    }
}
