using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_RGBKanallariForm : Form
    {
        private PictureBox pcbOriginal, pcbRed, pcbGreen, pcbBlue;
        private Button btnYukle, btnKanallariAyir, btnGeri;
        private Label lblOriginal, lblRed, lblGreen, lblBlue;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje5_RGBKanallariForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: RGB Renk Kanalları";

            int pcbSize = 300;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal Resim (RGB)", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblRed = new Label() { Text = "Kırmızı Kanal (R,0,0)", Location = new Point(margin * 2 + pcbSize + 90, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblGreen = new Label() { Text = "Yeşil Kanal (0,G,0)", Location = new Point(margin + 100, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblBlue = new Label() { Text = "Mavi Kanal (0,0,B)", Location = new Point(margin * 2 + pcbSize + 90, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbRed = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            pcbGreen = new PictureBox() { Location = new Point(margin, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            pcbBlue = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin * 3 + pcbSize * 2;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnKanallariAyir = new Button() { Text = "Kanalları Ayır", Location = new Point(margin + 170, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen };
            btnKanallariAyir.Click += new EventHandler(btnKanallariAyir_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(margin * 2 + pcbSize * 2 - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, lblRed, lblGreen, lblBlue, pcbOriginal, pcbRed, pcbGreen, pcbBlue, btnYukle, btnKanallariAyir, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje5_RGBKanallariForm";
            this.Size = new Size(700, 770);
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
                pcbOriginal.Image = originalBitmap;

                pcbRed.Image = null;
                pcbGreen.Image = null;
                pcbBlue.Image = null;
            }
        }

        private void btnKanallariAyir_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) { MessageBox.Show("Lütfen önce bir resim yükleyin!"); return; }

            int genislik = originalBitmap.Width;
            int yukseklik = originalBitmap.Height;

            Bitmap redBitmap = new Bitmap(genislik, yukseklik);
            Bitmap greenBitmap = new Bitmap(genislik, yukseklik);
            Bitmap blueBitmap = new Bitmap(genislik, yukseklik);

            for (int y = 0; y < yukseklik; y++)
            {
                for (int x = 0; x < genislik; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);

                    int r = originalColor.R;
                    int g = originalColor.G;
                    int b = originalColor.B;

                    Color redChannelColor = Color.FromArgb(r, 0, 0);
                    Color greenChannelColor = Color.FromArgb(0, g, 0);
                    Color blueChannelColor = Color.FromArgb(0, 0, b);

                    redBitmap.SetPixel(x, y, redChannelColor);
                    greenBitmap.SetPixel(x, y, greenChannelColor);
                    blueBitmap.SetPixel(x, y, blueChannelColor);
                }
            }

            pcbRed.Image = redBitmap;
            pcbGreen.Image = greenBitmap;
            pcbBlue.Image = blueBitmap;
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
