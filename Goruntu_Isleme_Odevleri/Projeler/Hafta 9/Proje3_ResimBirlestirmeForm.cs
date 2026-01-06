using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_ResimBirlestirmeForm : Form
    {
        private PictureBox pcbResim1, pcbResim2, pcbSonuc;
        private Button btnResimSec1, btnResimSec2, btnBirlestirManuel, btnOtomatikNormalize, btnGeri;
        private TrackBar tbOran1, tbOran2;
        private Label lblOran1, lblOran2, lblBilgi;

        private Bitmap bmpResim1, bmpResim2;
        private Form haftaFormu;

        public Proje3_ResimBirlestirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Normalizasyon ve Otomatik Ölçekleme";
            ArayuzOlustur();
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_ResimBirlestirmeForm";
            this.Size = new Size(1050, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void ArayuzOlustur()
        {
            pcbResim1 = new PictureBox() { Location = new Point(20, 20), Size = new Size(250, 250), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.LightGray };
            this.Controls.Add(pcbResim1);

            btnResimSec1 = new Button() { Text = "1. Resmi Yükle", Location = new Point(20, 280), Size = new Size(120, 30) };
            btnResimSec1.Click += btnResimSec1_Click;
            this.Controls.Add(btnResimSec1);

            tbOran1 = new TrackBar() { Orientation = Orientation.Vertical, Minimum = 0, Maximum = 100, Value = 50, Location = new Point(280, 20), Size = new Size(45, 250), TickFrequency = 10 };
            tbOran1.Scroll += (s, e) => lblOran1.Text = "%" + tbOran1.Value;
            this.Controls.Add(tbOran1);

            lblOran1 = new Label() { Text = "%50", Location = new Point(280, 280), Size = new Size(40, 20) };
            this.Controls.Add(lblOran1);

            pcbResim2 = new PictureBox() { Location = new Point(340, 20), Size = new Size(250, 250), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.LightGray };
            this.Controls.Add(pcbResim2);

            btnResimSec2 = new Button() { Text = "2. Resmi Yükle", Location = new Point(340, 280), Size = new Size(120, 30) };
            btnResimSec2.Click += btnResimSec2_Click;
            this.Controls.Add(btnResimSec2);

            tbOran2 = new TrackBar() { Orientation = Orientation.Vertical, Minimum = 0, Maximum = 100, Value = 50, Location = new Point(600, 20), Size = new Size(45, 250), TickFrequency = 10 };
            tbOran2.Scroll += (s, e) => lblOran2.Text = "%" + tbOran2.Value;
            this.Controls.Add(tbOran2);

            lblOran2 = new Label() { Text = "%50", Location = new Point(600, 280), Size = new Size(40, 20) };
            this.Controls.Add(lblOran2);

            pcbSonuc = new PictureBox() { Location = new Point(660, 20), Size = new Size(250, 250), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pcbSonuc);

            // 1. Manuel Birleştirme
            btnBirlestirManuel = new Button() { Text = "MANUEL BİRLEŞTİR (Slider)", Location = new Point(660, 280), Size = new Size(250, 35) };
            btnBirlestirManuel.Click += btnBirlestirManuel_Click;
            this.Controls.Add(btnBirlestirManuel);

            // 2. OTOMATİK ve NORMALİZASYONLU Birleştirme
            btnOtomatikNormalize = new Button() { Text = "OTOMATİK NORMALİZASYON", Location = new Point(660, 325), Size = new Size(250, 45), BackColor = Color.LightGreen, Font = new Font("Arial", 9, FontStyle.Bold) };
            btnOtomatikNormalize.Click += btnOtomatikNormalize_Click;
            this.Controls.Add(btnOtomatikNormalize);

            lblBilgi = new Label() { Text = "Bilgi: Sonuçlar burada görünecek.", Location = new Point(20, 400), AutoSize = true, ForeColor = Color.Blue };
            this.Controls.Add(lblBilgi);

            btnGeri = new Button() { Text = "Geri Dön", BackColor = Color.LightCoral, Location = new Point(810, 400), Size = new Size(100, 40) };
            btnGeri.Click += (s, e) => this.Close();
            this.Controls.Add(btnGeri);
        }

        private void btnResimSec1_Click(object sender, EventArgs e) { ResimYukle(ref bmpResim1, pcbResim1); }
        private void btnResimSec2_Click(object sender, EventArgs e) { ResimYukle(ref bmpResim2, pcbResim2); }

        private void ResimYukle(ref Bitmap hedefBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                hedefBmp = new Bitmap(ofd.FileName);
                pcb.Image = hedefBmp;
            }
        }

        private void btnBirlestirManuel_Click(object sender, EventArgs e)
        {
            if (bmpResim1 == null || bmpResim2 == null) return;

            int w = bmpResim1.Width;
            int h = bmpResim1.Height;
            Bitmap resized2 = new Bitmap(bmpResim2, new Size(w, h));
            Bitmap sonuc = new Bitmap(w, h);

            double oran1 = tbOran1.Value / 100.0;
            double oran2 = tbOran2.Value / 100.0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c1 = bmpResim1.GetPixel(x, y);
                    Color c2 = resized2.GetPixel(x, y);

                    int r = (int)(c1.R * oran1 + c2.R * oran2);
                    int g = (int)(c1.G * oran1 + c2.G * oran2);
                    int b = (int)(c1.B * oran1 + c2.B * oran2);

                    // 255'i geçerse 255'e eşitle (Clamp)
                    if (r > 255) r = 255;
                    if (g > 255) g = 255;
                    if (b > 255) b = 255;

                    sonuc.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            pcbSonuc.Image = sonuc;
            lblBilgi.Text = "Manuel birleştirme tamamlandı. 255'i geçen değerler kırpıldı.";
        }

        private void btnOtomatikNormalize_Click(object sender, EventArgs e)
        {
            if (bmpResim1 == null || bmpResim2 == null) { MessageBox.Show("Resimleri yükleyin."); return; }

            this.Cursor = Cursors.WaitCursor;

            // 1. Resimlerin Ortalama Parlaklığını Hesapla
            double ortalama1 = OrtalamaParlaklikBul(bmpResim1);
            double ortalama2 = OrtalamaParlaklikBul(bmpResim2);

            // 2. Otomatik Ölçekleme Katsayılarını Belirle
            double hedefParlaklik = 128.0;
            double katsayi1 = hedefParlaklik / ortalama1;
            double katsayi2 = hedefParlaklik / ortalama2;

            if (katsayi1 > 3.0) katsayi1 = 3.0;
            if (katsayi2 > 3.0) katsayi2 = 3.0;

            int w = bmpResim1.Width;
            int h = bmpResim1.Height;
            Bitmap resized2 = new Bitmap(bmpResim2, new Size(w, h));
            Bitmap sonuc = new Bitmap(w, h);

            double[,] tempR = new double[w, h];
            double[,] tempG = new double[w, h];
            double[,] tempB = new double[w, h];

            double maxDeger = 0;

            // 3. PİKSEL TOPLAMA İŞLEMİ
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c1 = bmpResim1.GetPixel(x, y);
                    Color c2 = resized2.GetPixel(x, y);

                    // Pikselleri katsayılarla çarpıp topluyoruz (Ham Toplam)
                    double tR = (c1.R * katsayi1) + (c2.R * katsayi2);
                    double tG = (c1.G * katsayi1) + (c2.G * katsayi2);
                    double tB = (c1.B * katsayi1) + (c2.B * katsayi2);

                    tempR[x, y] = tR;
                    tempG[x, y] = tG;
                    tempB[x, y] = tB;

                    // En büyük değeri takip et (Normalizasyon için lazım)
                    if (tR > maxDeger) maxDeger = tR;
                    if (tG > maxDeger) maxDeger = tG;
                    if (tB > maxDeger) maxDeger = tB;
                }
            }

            // 4. NORMALİZASYON İŞLEMİ
            // Bulduğumuz en büyük değeri 255'e, diğerlerini de orantılı olarak aşağıya çekeceğiz.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Normalizasyon formülü
                    int finalR = (int)((tempR[x, y] / maxDeger) * 255);
                    int finalG = (int)((tempG[x, y] / maxDeger) * 255);
                    int finalB = (int)((tempB[x, y] / maxDeger) * 255);

                    // Son güvenlik kontrolü
                    if (finalR > 255) finalR = 255;
                    if (finalG > 255) finalG = 255;
                    if (finalB > 255) finalB = 255;

                    sonuc.SetPixel(x, y, Color.FromArgb(finalR, finalG, finalB));
                }
            }

            pcbSonuc.Image = sonuc;
            lblBilgi.Text = $"Otomatik İşlem Tamamlandı.\nOrtalama1: {ortalama1:F1}, Katsayı1: {katsayi1:F2} | Ortalama2: {ortalama2:F1}, Katsayı2: {katsayi2:F2}\nNormalizasyon için Max Değer: {maxDeger:F1} (255'e ölçeklendi).";

            this.Cursor = Cursors.Default;
        }

        // Yardımcı Fonksiyon: Resmin ortalama parlaklığını bulur
        private double OrtalamaParlaklikBul(Bitmap bmp)
        {
            long toplamParlaklik = 0;
            Bitmap kucuk = new Bitmap(bmp, new Size(50, 50));

            for (int y = 0; y < kucuk.Height; y++)
            {
                for (int x = 0; x < kucuk.Width; x++)
                {
                    Color c = kucuk.GetPixel(x, y);
                    // Parlaklık = (R+G+B)/3
                    toplamParlaklik += (c.R + c.G + c.B) / 3;
                }
            }
            // Ortalama = Toplam / Piksel Sayısı
            return (double)toplamParlaklik / (kucuk.Width * kucuk.Height);
        }
    }
}