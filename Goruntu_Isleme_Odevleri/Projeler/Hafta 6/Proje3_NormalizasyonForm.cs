using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_NormalizasyonForm : Form
    {
        private PictureBox pcbOriginal, pcbClamped, pcbNormalized;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblOriginal, lblClamped, lblNormalized, lblInfoClamped, lblInfoNormalized;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje3_NormalizasyonForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Normalizasyon vs Kesme (Clamping)";

            int pcbSize = 300;
            int margin = 20;

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblClamped = new Label() { Text = "Standart Yöntem (Kesme/Clamping)", Location = new Point(margin * 2 + pcbSize + 50, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbClamped = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            lblInfoClamped = new Label() { Text = "Değerler 0-255 dışındaysa kesilir.\nVeri kaybı olabilir.", Location = new Point(margin * 2 + pcbSize, margin + 25 + pcbSize + 5), AutoSize = true, ForeColor = Color.Red };

            lblNormalized = new Label() { Text = "Normalizasyon", Location = new Point(margin * 3 + pcbSize * 2 + 50, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbNormalized = new PictureBox() { Location = new Point(margin * 3 + pcbSize * 2, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            lblInfoNormalized = new Label() { Text = "En düşük ve en yüksek değerler\n0-255 aralığına genişletilir.", Location = new Point(margin * 3 + pcbSize * 2, margin + 25 + pcbSize + 5), AutoSize = true, ForeColor = Color.Green };

            int controlsY = margin + pcbSize + 60;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnUygula = new Button() { Text = "Netleştir ve Karşılaştır", Location = new Point(200, controlsY), Size = new Size(200, 40), BackColor = Color.LightBlue, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 200, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblClamped, pcbClamped, lblNormalized, pcbNormalized,
                lblInfoClamped, lblInfoNormalized, btnYukle, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_NormalizasyonForm";
            this.Size = new Size(1000, 600);
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
                pcbClamped.Image = null;
                pcbNormalized.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // Farkı net görmek için güçlü bir Netleştirme (Sharpen) matrisi
            // Bu matris negatif ve 255 üstü değerler üretmeye çok meyillidir.
            double[,] kernel = {
                { -1, -1, -1 },
                { -1,  9, -1 },
                { -1, -1, -1 }
            };

            // Önce ham (işlenmemiş) piksel değerlerini hesapla (Double dizisi olarak)
            double[,,] rawPixels = ConvolveImage(originalBitmap, kernel);

            // Yöntem A: Clamping (Kesme - Standart)
            pcbClamped.Image = GenerateClampedImage(rawPixels, originalBitmap.Width, originalBitmap.Height);

            // Yöntem B: Normalizasyon
            pcbNormalized.Image = GenerateNormalizedImage(rawPixels, originalBitmap.Width, originalBitmap.Height);

            this.Cursor = Cursors.Default;
        }

        // Konvolüsyon işlemi (Sonuçları Bitmap değil, ham double dizisi olarak döner)
        // Böylece verileri kaybetmeden analiz edebiliriz.
        private double[,,] ConvolveImage(Bitmap src, double[,] kernel)
        {
            int w = src.Width;
            int h = src.Height;
            double[,,] result = new double[w, h, 3]; // R, G, B için
            int radius = 1; // 3x3 kernel

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = Math.Max(0, Math.Min(w - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(h - 1, y + ky));

                            Color c = src.GetPixel(pX, pY);
                            double weight = kernel[ky + radius, kx + radius];

                            rSum += c.R * weight;
                            gSum += c.G * weight;
                            bSum += c.B * weight;
                        }
                    }
                    result[x, y, 0] = rSum;
                    result[x, y, 1] = gSum;
                    result[x, y, 2] = bSum;
                }
            }
            return result;
        }

        // Yöntem A: Değerleri 0 ve 255 sınırlarına kes (Clamp)
        private Bitmap GenerateClampedImage(double[,,] data, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int r = Math.Max(0, Math.Min(255, (int)data[x, y, 0]));
                    int g = Math.Max(0, Math.Min(255, (int)data[x, y, 1]));
                    int b = Math.Max(0, Math.Min(255, (int)data[x, y, 2]));
                    bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return bmp;
        }

        // Yöntem B: Değerleri Min-Max aralığına göre normalize et (Scale)
        private Bitmap GenerateNormalizedImage(double[,,] data, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);

            // Tüm resimdeki en küçük ve en büyük değeri bul
            double minVal = double.MaxValue;
            double maxVal = double.MinValue;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (data[x, y, c] < minVal) minVal = data[x, y, c];
                        if (data[x, y, c] > maxVal) maxVal = data[x, y, c];
                    }
                }
            }

            lblInfoNormalized.Text = $"Hesaplanan Aralık: {minVal:0.0} ile {maxVal:0.0}\nBu aralık 0-255'e genişletildi.";

            // Eğer tüm resim tek renkse (min == max), bölme hatasını önle
            if (maxVal == minVal) return GenerateClampedImage(data, w, h);

            // Tüm pikselleri bu aralığa göre 0-255'e çek
            // Formül: Yeni = (Eski - Min) * 255 / (Max - Min)
            double range = maxVal - minVal;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int r = (int)((data[x, y, 0] - minVal) / range * 255.0);
                    int g = (int)((data[x, y, 1] - minVal) / range * 255.0);
                    int b = (int)((data[x, y, 2] - minVal) / range * 255.0);

                    // Yine de 0-255 garantisi verelim (yuvarlama hataları için)
                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return bmp;
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