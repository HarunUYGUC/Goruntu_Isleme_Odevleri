using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_AciKarsilastirmaForm : Form
    {
        private PictureBox pcbResim;
        private Button btnDuzCizgiOlustur, btnResimYukle, btnHesapla, btnGeri;
        private Label lblSobel, lblCompass, lblInfo;
        private TextBox txtSobelAci, txtCompassAci;
        private Form haftaFormu;
        private Bitmap originalBitmap;

        public Proje5_AciKarsilastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: Açı Hesaplama ve Karşılaştırma (Sobel vs Compass)";
            this.Size = new Size(800, 600);
        }

        private void InitializeComponent()
        {
            // Arayüz Elemanları
            int margin = 20;
            int pcbW = 500, pcbH = 400;

            pcbResim = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbW, pcbH), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };

            int ctrlX = margin + pcbW + 20;

            btnDuzCizgiOlustur = new Button() { Text = "Rastgele Düz Çizgi Çiz", Location = new Point(ctrlX, margin), Size = new Size(200, 40) };
            btnDuzCizgiOlustur.Click += BtnDuzCizgiOlustur_Click;

            btnResimYukle = new Button() { Text = "Dağınık Çizgi Yükle", Location = new Point(ctrlX, margin + 50), Size = new Size(200, 40) };
            btnResimYukle.Click += BtnResimYukle_Click;

            btnHesapla = new Button() { Text = "Açıları Hesapla", Location = new Point(ctrlX, margin + 110), Size = new Size(200, 50), BackColor = Color.LightGreen, Enabled = false };
            btnHesapla.Click += BtnHesapla_Click;

            lblSobel = new Label() { Text = "Sobel (Hassas) Açı:", Location = new Point(ctrlX, margin + 180), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            txtSobelAci = new TextBox() { Location = new Point(ctrlX, margin + 200), Size = new Size(200, 25), ReadOnly = true };

            lblCompass = new Label() { Text = "Compass (8 Matris) Açı:", Location = new Point(ctrlX, margin + 240), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            txtCompassAci = new TextBox() { Location = new Point(ctrlX, margin + 260), Size = new Size(200, 25), ReadOnly = true };

            lblInfo = new Label() { Text = "Not: Sobel matematiksel (Atan2) hesap yaptığı için ondalıklı hassas sonuç verir. Compass 8 yönlü olduğu için genelde 45 derecelik adımlarla sonuç verir.", Location = new Point(ctrlX, margin + 300), Size = new Size(200, 150), ForeColor = Color.Gray };

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX, 500), Size = new Size(200, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { pcbResim, btnDuzCizgiOlustur, btnResimYukle, btnHesapla, lblSobel, txtSobelAci, lblCompass, txtCompassAci, lblInfo, btnGeri });

            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        // --- 1. DÜZ ÇİZGİ OLUŞTURMA ---
        private void BtnDuzCizgiOlustur_Click(object sender, EventArgs e)
        {
            // Siyah zemin üzerine beyaz çizgi çiz
            Bitmap bmp = new Bitmap(pcbResim.Width, pcbResim.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                Random rnd = new Random();

                // Rastgele iki nokta seç
                Point p1 = new Point(rnd.Next(50, 200), rnd.Next(50, 350));
                Point p2 = new Point(rnd.Next(300, 450), rnd.Next(50, 350));

                // Çizgiyi çiz
                g.DrawLine(new Pen(Color.White, 3), p1, p2);
            }
            originalBitmap = bmp;
            pcbResim.Image = originalBitmap;
            btnHesapla.Enabled = true;
            Temizle();
        }

        private void BtnResimYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(ofd.FileName);
                pcbResim.Image = originalBitmap;
                btnHesapla.Enabled = true;
                Temizle();
            }
        }

        private void Temizle()
        {
            txtSobelAci.Text = "";
            txtCompassAci.Text = "";
        }

        private void BtnHesapla_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // 1. Sobel ile Açı Bul
            double sobelAngle = CalculateSobelAverageAngle(originalBitmap);
            txtSobelAci.Text = sobelAngle.ToString("F2") + "°";

            // 2. Compass (8 Matris) ile Açı Bul
            double compassAngle = CalculateCompass8DirectionAngle(originalBitmap);
            txtCompassAci.Text = compassAngle.ToString("F2") + "°";

            this.Cursor = Cursors.Default;
        }

        // --- SOBEL ALGORİTMASI (HASSAS HESAP) ---
        private double CalculateSobelAverageAngle(Bitmap src)
        {
            int w = src.Width;
            int h = src.Height;

            double totalAngle = 0;
            int pixelCount = 0;

            // Sobel Maskeleri
            int[,] gxMask = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gyMask = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    Color c = src.GetPixel(x, y);
                    // Siyah arka planı hızlıca geçmek için
                    if (c.R < 30) continue;

                    double valGx = 0;
                    double valGy = 0;

                    // Konvolüsyon
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color p = src.GetPixel(x + j, y + i);
                            // Gri tonlama
                            double gray = (p.R + p.G + p.B) / 3.0;
                            valGx += gray * gxMask[i + 1, j + 1];
                            valGy += gray * gyMask[i + 1, j + 1];
                        }
                    }

                    double magnitude = Math.Sqrt(valGx * valGx + valGy * valGy);

                    // Sadece belirgin kenarları hesaba kat
                    if (magnitude > 100)
                    {
                        // Açıyı hesapla (Radyan -> Derece)
                        double angleRad = Math.Atan2(valGy, valGx);
                        double angleDeg = angleRad * (180.0 / Math.PI);

                        // 1. Adım: Negatif açıları pozitife çevir (0-360)
                        if (angleDeg < 0) angleDeg += 360;

                        // 2. Adım: ZIT KENARLARI BİRLEŞTİR
                        // Çizginin bir tarafı x derece ise, diğer tarafı x+180 derecedir.
                        // Biz çizginin yönünü aradığımız için bunları 0-180 arasına indirgeriz.
                        if (angleDeg >= 180) angleDeg -= 180;

                        // Not: Sobel gradyanı kenara DİK'tir. Çizginin yönü buna 90 derece fark eder.

                        totalAngle += angleDeg;
                        pixelCount++;
                    }
                }
            }

            if (pixelCount == 0) return 0;
            return totalAngle / pixelCount;
        }

        // --- COMPASS ALGORİTMASI (8 MATRİS - KABA HESAP) ---
        private double CalculateCompass8DirectionAngle(Bitmap src)
        {
            // Yönler ve Açılar (0 - 315)
            List<int[,]> masks = new List<int[,]>();
            List<double> angles = new List<double>(); 

            // Robinson Maskeleri (veya Kirsch)
            // Doğu (0)
            masks.Add(new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } }); angles.Add(0);
            // KuzeyDoğu (45)
            masks.Add(new int[,] { { 0, 1, 2 }, { -1, 0, 1 }, { -2, -1, 0 } }); angles.Add(45);
            // Kuzey (90)
            masks.Add(new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } }); angles.Add(90);
            // KuzeyBatı (135)
            masks.Add(new int[,] { { 2, 1, 0 }, { 1, 0, -1 }, { 0, -1, -2 } }); angles.Add(135);
            // Batı (180)
            masks.Add(new int[,] { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } }); angles.Add(180);
            // GüneyBatı (225)
            masks.Add(new int[,] { { 0, -1, -2 }, { 1, 0, -1 }, { 2, 1, 0 } }); angles.Add(225);
            // Güney (270)
            masks.Add(new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } }); angles.Add(270);
            // GüneyDoğu (315)
            masks.Add(new int[,] { { -2, -1, 0 }, { -1, 0, 1 }, { 0, 1, 2 } }); angles.Add(315);

            double totalAngle = 0;
            int count = 0;

            int w = src.Width;
            int h = src.Height;

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    Color c = src.GetPixel(x, y);
                    if (c.R < 30) continue;

                    double maxResponse = -1;
                    double selectedAngle = 0;

                    for (int k = 0; k < 8; k++)
                    {
                        double response = 0;
                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                Color p = src.GetPixel(x + j, y + i);
                                double gray = (p.R + p.G + p.B) / 3.0;
                                response += gray * masks[k][i + 1, j + 1];
                            }
                        }

                        if (response > maxResponse)
                        {
                            maxResponse = response;
                            selectedAngle = angles[k];
                        }
                    }

                    if (maxResponse > 100)
                    {
                        // 180 derece ve üzerini modlayarak ters yönleri eşle
                        if (selectedAngle >= 180) selectedAngle -= 180;

                        totalAngle += selectedAngle;
                        count++;
                    }
                }
            }

            if (count == 0) return 0;
            return totalAngle / count;
        }
    }
}