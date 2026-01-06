using System;
using System.Drawing;
using System.Drawing.Imaging; 
using System.Runtime.InteropServices; 
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje8_PiramitBulaniklastirmaForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblOriginal, lblResult, lblKernelSize, lblStep, lblKernelInfo;
        private NumericUpDown nudKernelSize, nudStep;
        private Form haftaFormu;
        private Bitmap originalBitmap;

        public Proje8_PiramitBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 8: Piramit (Lineer) Bulanıklaştırma";

            SetupUI();
        }

        private void InitializeComponent()
        {
            this.Name = "Proje8_PiramitBulaniklastirmaForm";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void SetupUI()
        {
            int pcbSize = 350; int margin = 25;
            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            lblResult = new Label() { Text = "Piramit Filtresi Sonucu", Location = new Point(margin * 2 + pcbSize + 80, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 20;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += btnYukle_Click;

            lblKernelSize = new Label() { Text = "Matris Boyutu:", Location = new Point(200, controlsY + 12), AutoSize = true };
            nudKernelSize = new NumericUpDown() { Location = new Point(340, controlsY + 10), Width = 60, Minimum = 3, Maximum = 51, Value = 7, Increment = 2 };

            lblStep = new Label() { Text = "Artış Adımı:", Location = new Point(420, controlsY + 12), AutoSize = true };
            nudStep = new NumericUpDown() { Location = new Point(500, controlsY + 10), Width = 60, Minimum = 1, Maximum = 100, Value = 2 };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(580, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += btnUygula_Click;

            lblKernelInfo = new Label() { Text = "Kernel Önizleme:", Location = new Point(200, controlsY + 50), AutoSize = true, Font = new Font("Consolas", 9), ForeColor = Color.DarkBlue };

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(700, controlsY), Size = new Size(100, 40), BackColor = Color.LightCoral };
            btnGeri.Click += btnGeri_Click;

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblKernelSize, nudKernelSize, lblStep, nudStep, btnUygula, lblKernelInfo, btnGeri });
        }

        private void btnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Formatı 24bppRgb'ye çevirerek yüklüyoruz (Byte dizisi işlemleri için en kolayı)
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
                ShowKernelPreview();
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;

            int size = (int)nudKernelSize.Value;
            if (size % 2 == 0) size++; // Tek sayı yap
            int step = (int)nudStep.Value;

            ShowKernelPreview();

            // Matrisi Oluştur
            double[,] kernel = CreatePyramidKernel(size, step);

            pcbResult.Image = ApplyConvolutionSafe(originalBitmap, kernel);

            this.Cursor = Cursors.Default;
        }

        // Piramit Kernel Mantığı
        private double[,] CreatePyramidKernel(int size, int step, bool normalize = true)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int radius = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int distX = Math.Abs(x - radius);
                    int distY = Math.Abs(y - radius);
                    int dist = Math.Max(distX, distY);

                    // Merkez yüksek, kenarlar düşük
                    double value = 1 + (radius - dist) * step;

                    kernel[y, x] = value;
                    sum += value;
                }
            }

            if (normalize)
            {
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        kernel[y, x] /= sum;
            }
            return kernel;
        }

        private Bitmap ApplyConvolutionSafe(Bitmap srcImage, double[,] kernel)
        {
            int width = srcImage.Width;
            int height = srcImage.Height;
            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            // 1. Veriyi Kilitle
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            // 2. Byte Dizisine Kopyala (Managed Array)
            int bytes = srcData.Stride * height;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);

            int stride = srcData.Stride;

            // 3. Diziler Üzerinde İşlem Yap
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    // Kernel Döngüsü
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int nY = y + ky;

                        // Sınır Kontrolü (Clamp)
                        if (nY < 0) nY = 0;
                        if (nY >= height) nY = height - 1;

                        // Satırın başlangıç indeksi
                        int rowOffset = nY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nX = x + kx;

                            // Sınır Kontrolü
                            if (nX < 0) nX = 0;
                            if (nX >= width) nX = width - 1;

                            // 24bpp'de her piksel 3 byte (Blue, Green, Red)
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

        private void ShowKernelPreview()
        {
            int size = (int)nudKernelSize.Value;
            if (size % 2 == 0) size++;
            int step = (int)nudStep.Value;

            if (size > 15) { lblKernelInfo.Text = $"Kernel çok büyük ({size}x{size})."; return; }

            double[,] kernel = CreatePyramidKernel(size, step, false);
            string preview = $"Kernel ({size}x{size}, Step={step}):\n";

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    preview += kernel[y, x].ToString("00") + " ";
                }
                preview += "\n";
            }
            lblKernelInfo.Text = preview;
        }

        private void btnGeri_Click(object sender, EventArgs e) { this.Close(); }
        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e) { haftaFormu.Show(); }
    }
}