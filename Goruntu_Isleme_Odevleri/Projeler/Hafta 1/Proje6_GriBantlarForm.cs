using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje6_GriBantlarForm : Form
    {
        private PictureBox pcbOriginal, pcbRedGray, pcbGreenGray, pcbBlueGray;
        private Button btnYukle, btnBantlariGoster, btnGeri;
        private Label lblOriginal, lblRedGray, lblGreenGray, lblBlueGray;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje6_GriBantlarForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 6: Renk Bantlarının Gri Gösterimi";

            int pcbSize = 300;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblRedGray = new Label() { Text = "Kırmızı Bant (Gri)", Location = new Point(margin * 2 + pcbSize + 90, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblGreenGray = new Label() { Text = "Yeşil Bant (Gri)", Location = new Point(margin + 100, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblBlueGray = new Label() { Text = "Mavi Bant (Gri)", Location = new Point(margin * 2 + pcbSize + 90, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbRedGray = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            pcbGreenGray = new PictureBox() { Location = new Point(margin, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            pcbBlueGray = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = this.ClientSize.Height - 60;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnBantlariGoster = new Button() { Text = "Gri Bantları Göster", Location = new Point(margin + 170, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen };
            btnBantlariGoster.Click += new EventHandler(btnBantlariGoster_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, lblRedGray, lblGreenGray, lblBlueGray, pcbOriginal, pcbRedGray, pcbGreenGray, pcbBlueGray, btnYukle, btnBantlariGoster, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje6_GriBantlarForm";
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
                pcbRedGray.Image = null;
                pcbGreenGray.Image = null;
                pcbBlueGray.Image = null;
            }
        }

        private void btnBantlariGoster_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) { MessageBox.Show("Lütfen önce bir resim yükleyin!"); return; }

            int genislik = originalBitmap.Width;
            int yukseklik = originalBitmap.Height;

            Bitmap redGrayBitmap = new Bitmap(genislik, yukseklik);
            Bitmap greenGrayBitmap = new Bitmap(genislik, yukseklik);
            Bitmap blueGrayBitmap = new Bitmap(genislik, yukseklik);

            for (int y = 0; y < yukseklik; y++)
            {
                for (int x = 0; x < genislik; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);

                    int r = originalColor.R;
                    int g = originalColor.G;
                    int b = originalColor.B;

                    // RGB değerleri teker teker hepsi aynı değeri alırsa grinin bir tonu oluşuyor.
                    // (0, 0, 0), siyah; (255, 255, 255), beyaz; (128, 128, 128), orta gri vb.

                    // Kırmızı bant için: R, G ve B değerlerinin hepsi Kırmızı kanalın değeri yapılır.
                    Color redGrayColor = Color.FromArgb(r, r, r);
                    // Yeşil bant için: R, G ve B değerlerinin hepsi Yeşil kanalın değeri yapılır.
                    Color greenGrayColor = Color.FromArgb(g, g, g);
                    // Mavi bant için: R, G ve B değerlerinin hepsi Mavi kanalın değeri yapılır.
                    Color blueGrayColor = Color.FromArgb(b, b, b);

                    redGrayBitmap.SetPixel(x, y, redGrayColor);
                    greenGrayBitmap.SetPixel(x, y, greenGrayColor);
                    blueGrayBitmap.SetPixel(x, y, blueGrayColor);
                }
            }

            pcbRedGray.Image = redGrayBitmap;
            pcbGreenGray.Image = greenGrayBitmap;
            pcbBlueGray.Image = blueGrayBitmap;
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
