using System;
using System.Drawing;
using System.Drawing.Imaging; // Hızlı işlem için gerekli
using System.Runtime.InteropServices; // Marshal.Copy için gerekli
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

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Gauss Filtresi Sonucu", Location = new Point(margin * 2 + pcbSize + 80, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 20;

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

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(700, controlsY), Size = new Size(120, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            lblKernelInfo = new Label()
            {
                Text = "Otomatik Hesaplanan Kernel Boyutu: -",
                Location = new Point(margin, controlsY + 50), 
                AutoSize = true,
                ForeColor = Color.Blue,
                Font = new Font("Consolas", 10)
            };

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblSigma, nudSigma, btnUygula, lblKernelInfo, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje4_GaussSigmaForm";
            this.Size = new Size(880, 550);
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
                Bitmap temp = new Bitmap(dialog.FileName);

                originalBitmap = new Bitmap(temp.Width, temp.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(originalBitmap))
                {
                    g.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                }
                temp.Dispose();

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

            // Kernel Boyutu Hesabı: 6 * Sigma (Gauss dağılımının %99.7'sini kapsar)
            int kernelSize = (int)Math.Ceiling(sigma * 6);
            if (kernelSize % 2 == 0) kernelSize++; // Tek sayı yap
            if (kernelSize < 3) kernelSize = 3;

            lblKernelInfo.Text = $"Sigma: {sigma} -> Kernel Boyutu: {kernelSize}x{kernelSize}";

            pcbResult.Image = ApplyGaussianFilterFast(originalBitmap, kernelSize, sigma);

            this.Cursor = Cursors.Default;
        }

        private Bitmap ApplyGaussianFilterFast(Bitmap srcImage, int kernelSize, double sigma)
        {
            double[,] kernel = CalculateGaussianKernel(kernelSize, sigma);
            return ApplyConvolutionFast(srcImage, kernel);
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
                    double distance = x * x + y * y;
                    double value = Math.Exp(-distance / twoSigmaSquare) / piSigmaSquare;
                    kernel[y + radius, x + radius] = value;
                    sum += value;
                }
            }

            // Normalize et
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    kernel[y, x] /= sum;

            return kernel;
        }

        private Bitmap ApplyConvolutionFast(Bitmap srcImage, double[,] kernel)
        {
            int width = srcImage.Width;
            int height = srcImage.Height;
            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            // Sonuç resmi (24 bit formatında)
            Bitmap dstImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            // 1. Belleği Kilitle (LockBits)
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            // 2. Verileri Byte Dizisine Kopyala
            int bytes = srcData.Stride * height;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);

            // Stride: Bir satırın bellekte kapladığı gerçek genişlik (Padding dahil)
            int stride = srcData.Stride;

            // 3. Piksel İşleme (Döngüler)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    // Kernel Döngüsü
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int nY = y + ky;

                        // Kenar kontrolü (Clamp yöntemi - Sınır taşarsa en yakın pikseli al)
                        if (nY < 0) nY = 0;
                        if (nY >= height) nY = height - 1;

                        int rowOffset = nY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nX = x + kx;

                            if (nX < 0) nX = 0;
                            if (nX >= width) nX = width - 1;

                            // 24bpp olduğu için her piksel 3 byte (Blue, Green, Red)
                            int pixelIndex = rowOffset + (nX * 3);

                            double weight = kernel[ky + radius, kx + radius];

                            bSum += srcBuffer[pixelIndex] * weight;     // Blue
                            gSum += srcBuffer[pixelIndex + 1] * weight; // Green
                            rSum += srcBuffer[pixelIndex + 2] * weight; // Red
                        }
                    }

                    int dstIndex = (y * stride) + (x * 3);

                    dstBuffer[dstIndex] = (byte)Math.Min(255, Math.Max(0, bSum));
                    dstBuffer[dstIndex + 1] = (byte)Math.Min(255, Math.Max(0, gSum));
                    dstBuffer[dstIndex + 2] = (byte)Math.Min(255, Math.Max(0, rSum));
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);

            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);

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