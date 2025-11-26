using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_GaussSigmaForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblOriginal, lblResult, lblSigma, lblKernelInfo;
        private NumericUpDown nudSigma;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje4_GaussSigmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Gauss Filtresinde Standart Sapma (Sigma) Etkisi";

            int pcbSize = 350;
            int margin = 25;

            // Resim Kutuları
            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Gauss Filtresi Sonucu", Location = new Point(margin * 2 + pcbSize + 80, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 20;

            // Kontroller
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblSigma = new Label() { Text = "Standart Sapma (Sigma):", Location = new Point(200, controlsY + 12), AutoSize = true };

            nudSigma = new NumericUpDown()
            {
                Location = new Point(350, controlsY + 10),
                Width = 60,
                Minimum = 0.1m,
                Maximum = 10.0m,
                DecimalPlaces = 1,
                Increment = 0.1m,
                Value = 1.0m
            };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(430, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            lblKernelInfo = new Label() { Text = "Otomatik Hesaplanan Kernel Boyutu: -", Location = new Point(570, controlsY + 12), AutoSize = true, ForeColor = Color.Blue };

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblSigma, nudSigma, btnUygula, lblKernelInfo, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje4_GaussSigmaForm";
            this.Size = new Size(850, 500);
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
                pcbResult.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;
            double sigma = (double)nudSigma.Value;

            // Sigma'ya göre ideal kernel boyutunu hesapla.
            // Kural: Kernel boyutu yaklaşık 6*sigma olmalıdır (ve tek sayı olmalıdır).
            // Çünkü Gauss eğrisi 3 sigma uzaklıkta neredeyse sıfıra iner (-3 sigma'dan +3 sigma'ya = 6 sigma).
            int kernelSize = (int)Math.Ceiling(sigma * 6);
            if (kernelSize % 2 == 0) kernelSize++; // Tek sayı yap
            if (kernelSize < 3) kernelSize = 3; // Minimum boyut

            lblKernelInfo.Text = $"Otomatik Hesaplanan Kernel Boyutu: {kernelSize}x{kernelSize}";

            pcbResult.Image = ApplyGaussianFilter(originalBitmap, kernelSize, sigma);
            this.Cursor = Cursors.Default;
        }

        private Bitmap ApplyGaussianFilter(Bitmap srcImage, int kernelSize, double sigma)
        {
            double[,] kernel = CalculateGaussianKernel(kernelSize, sigma);
            return ApplyConvolution(srcImage, kernel);
        }

        // 2D Gaussian Kernel Formülü
        private double[,] CalculateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int radius = size / 2;
            double twoSigmaSquare = 2 * sigma * sigma;
            double piSigmaSquare = Math.PI * twoSigmaSquare;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    // Formül: (1 / (2*pi*sigma^2)) * e^(-(x^2 + y^2) / (2*sigma^2))
                    double distance = x * x + y * y;
                    double value = Math.Exp(-distance / twoSigmaSquare) / piSigmaSquare;

                    kernel[y + radius, x + radius] = value;
                    sum += value;
                }
            }

            // Normalize et (Matris toplamını 1 yap, yoksa resim parlaklaşır/kararır)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }
            return kernel;
        }

        // Konvolüsyon İşlemi (Filtreyi resme uygula)
        private Bitmap ApplyConvolution(Bitmap srcImage, double[,] kernel)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            // Hız için LockBits kullanılabilir ama anlaşılırlık için GetPixel/SetPixel ile devam ediyoruz.
            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            // Sınır kontrolü (Padding: Edge replication)
                            int pX = Math.Max(0, Math.Min(srcImage.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(srcImage.Height - 1, y + ky));

                            Color p = srcImage.GetPixel(pX, pY);
                            double weight = kernel[ky + radius, kx + radius];

                            rSum += p.R * weight;
                            gSum += p.G * weight;
                            bSum += p.B * weight;
                        }
                    }

                    int r = Math.Max(0, Math.Min(255, (int)rSum));
                    int g = Math.Max(0, Math.Min(255, (int)gSum));
                    int b = Math.Max(0, Math.Min(255, (int)bSum));

                    dstImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return dstImage;
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