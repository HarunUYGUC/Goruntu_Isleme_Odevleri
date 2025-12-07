using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // Grafik çizimleri için
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_KenarRenklendirmeForm : Form
    {
        private PictureBox pcbOriginal, pcbMagnitude, pcbColorEdge, pcbLegend;
        private Label lblOriginal, lblMagnitude, lblColorEdge, lblLegend;
        private Button btnYukle, btnUygula, btnGeri;
        private TrackBar tbThreshold; // Kenar algılama hassasiyeti
        private Label lblThreshold;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje2_KenarRenklendirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Kenar Renklendirme (Sobel & Compass)";

            int pcbSize = 250;
            int margin = 15;
            int labelOffset = 20;

            // --- 1. Satır ---
            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 80, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblMagnitude = new Label() { Text = "Sobel Büyüklük (Magnitude)", Location = new Point(margin * 2 + pcbSize + 40, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbMagnitude = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // --- 2. Satır ---
            int row2Y = margin + labelOffset + pcbSize + 30;

            lblColorEdge = new Label() { Text = "Renklendirilmiş Kenarlar (Açı)", Location = new Point(margin + 40, row2Y), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbColorEdge = new PictureBox() { Location = new Point(margin, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblLegend = new Label() { Text = "Açı - Renk Skalası (Lejant)", Location = new Point(margin * 2 + pcbSize + 40, row2Y), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbLegend = new PictureBox() { Location = new Point(margin * 2 + pcbSize, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, SizeMode = PictureBoxSizeMode.CenterImage };

            // --- Kontroller ---
            int controlsX = margin * 3 + pcbSize * 2;
            int controlsY = margin + 50;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(controlsX, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblThreshold = new Label() { Text = "Eşik Değeri: 50", Location = new Point(controlsX, controlsY + 60), AutoSize = true };
            tbThreshold = new TrackBar() { Location = new Point(controlsX, controlsY + 80), Size = new Size(150, 45), Minimum = 0, Maximum = 200, Value = 50, TickFrequency = 10 };
            tbThreshold.Scroll += (s, e) => { lblThreshold.Text = $"Eşik Değeri: {tbThreshold.Value}"; };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(controlsX, controlsY + 130), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(controlsX, controlsY + 190), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblMagnitude, pcbMagnitude,
                lblColorEdge, pcbColorEdge, lblLegend, pcbLegend,
                btnYukle, lblThreshold, tbThreshold, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_KenarRenklendirmeForm";
            this.Size = new Size(750, 650);
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
                pcbMagnitude.Image = null;
                pcbColorEdge.Image = null;
                pcbLegend.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;
            int threshold = tbThreshold.Value;

            // Sobel ve Renklendirme Algoritmasını Çalıştır
            ApplySobelAndColorize(originalBitmap, threshold);

            // Renk Skalasını (Grafik) Oluştur
            pcbLegend.Image = CreateLegendImage(pcbLegend.Width, pcbLegend.Height);

            this.Cursor = Cursors.Default;
        }

        // --- GÖRÜNTÜ İŞLEME ALGORİTMASI ---
        private void ApplySobelAndColorize(Bitmap src, int threshold)
        {
            int w = src.Width;
            int h = src.Height;

            Bitmap bmpMag = new Bitmap(w, h);  // Siyah-Beyaz Kenar
            Bitmap bmpColor = new Bitmap(w, h); // Renkli Kenar

            // Sobel Kernel Matrisleri
            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            // Kenarlar hariç döngü
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    double sumX = 0;
                    double sumY = 0;

                    // 3x3 Konvolüsyon
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color p = src.GetPixel(x + j, y + i);
                            // Gri tonlama dönüşümü
                            int gray = (int)(p.R * 0.3 + p.G * 0.59 + p.B * 0.11);

                            sumX += gray * gx[i + 1, j + 1];
                            sumY += gray * gy[i + 1, j + 1];
                        }
                    }

                    // 1. Magnitude (Büyüklük) Hesapla
                    double magnitude = Math.Sqrt(sumX * sumX + sumY * sumY);

                    // Magnitude Resmini Oluştur (Normalize etmeden basit clamp)
                    int magVal = (int)Math.Min(255, magnitude);
                    bmpMag.SetPixel(x, y, Color.FromArgb(magVal, magVal, magVal));

                    // 2. Açı ve Renklendirme
                    if (magnitude > threshold)
                    {
                        // Açı Hesapla (Radyan -> Derece)
                        double angleRad = Math.Atan2(sumY, sumX);
                        double angleDeg = angleRad * (180.0 / Math.PI);

                        // Negatif açıları 0-360 aralığına çek
                        if (angleDeg < 0) angleDeg += 360;

                        // Açıyı renge dönüştür
                        Color color = HsvToRgb(angleDeg, 1.0, 1.0);
                        bmpColor.SetPixel(x, y, color);
                    }
                    else
                    {
                        // Eşik altındaysa siyah
                        bmpColor.SetPixel(x, y, Color.Black);
                    }
                }
            }

            pcbMagnitude.Image = bmpMag;
            pcbColorEdge.Image = bmpColor;
        }

        // --- YARDIMCI METOTLAR ---

        // Renk Skalası Grafiği Oluşturma
        private Bitmap CreateLegendImage(int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                int barWidth = 60;
                int barHeight = h - 40;
                int startX = (w - barWidth) / 2;
                int startY = 20;

                // 0'dan 360'a renkleri çiz
                for (int i = 0; i <= 360; i++)
                {
                    int y = startY + (int)((float)i / 360 * barHeight);
                    Color c = HsvToRgb(i, 1, 1);

                    using (Pen p = new Pen(c, 1))
                    {
                        g.DrawLine(p, startX, y, startX + barWidth, y);
                    }

                    // Etiketler
                    if (i % 45 == 0)
                    {
                        g.DrawString($"{i}°", new Font("Arial", 8), Brushes.Black, startX + barWidth + 5, y - 6);
                    }
                }
                g.DrawString("Açı", new Font("Arial", 9, FontStyle.Bold), Brushes.Black, startX, startY - 15);
            }
            return bmp;
        }

        // HSV'den RGB'ye Dönüşüm
        private Color HsvToRgb(double h, double S, double V)
        {
            double C = V * S;
            double X = C * (1 - Math.Abs((h / 60.0) % 2 - 1));
            double m = V - C;

            double r = 0, g = 0, b = 0;

            if (h >= 0 && h < 60) { r = C; g = X; b = 0; }
            else if (h >= 60 && h < 120) { r = X; g = C; b = 0; }
            else if (h >= 120 && h < 180) { r = 0; g = C; b = X; }
            else if (h >= 180 && h < 240) { r = 0; g = X; b = C; }
            else if (h >= 240 && h < 300) { r = X; g = 0; b = C; }
            else if (h >= 300 && h <= 360) { r = C; g = 0; b = X; }

            return Color.FromArgb(
                (int)((r + m) * 255),
                (int)((g + m) * 255),
                (int)((b + m) * 255));
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