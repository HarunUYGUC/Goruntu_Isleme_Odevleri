using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_FiltrelemeKarsilastirmaForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbFiltreTipi, cmbMatrisBoyutu;
        private Label lblOriginal, lblResult, lblFiltre, lblMatris;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje1_FiltrelemeKarsilastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 1: Filtreleme Algoritmaları Karşılaştırması";

            int pcbSize = 350;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Filtrelenmiş Resim", Location = new Point(margin * 2 + pcbSize + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 20;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblFiltre = new Label() { Text = "Filtre Tipi:", Location = new Point(160, controlsY + 10), AutoSize = true };
            cmbFiltreTipi = new ComboBox() { Location = new Point(230, controlsY + 8), DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            cmbFiltreTipi.Items.AddRange(new object[] { "Mean (Ortalama)", "Median (Ortanca)", "Gauss (Bulanıklaştırma)" });
            cmbFiltreTipi.SelectedIndex = 0;

            lblMatris = new Label() { Text = "Matris Boyutu:", Location = new Point(370, controlsY + 10), AutoSize = true };
            cmbMatrisBoyutu = new ComboBox() { Location = new Point(460, controlsY + 8), DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
            cmbMatrisBoyutu.Items.AddRange(new object[] { "3x3", "5x5", "7x7", "9x9" });
            cmbMatrisBoyutu.SelectedIndex = 0;

            btnUygula = new Button() { Text = "Uygula", Location = new Point(600, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Geri", Location = new Point(730, controlsY), Size = new Size(120, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblFiltre, cmbFiltreTipi, lblMatris, cmbMatrisBoyutu, btnUygula, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje1_FiltrelemeKarsilastirmaForm";
            this.Size = new Size(880, 500);
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
                // İşlem kolaylığı ve hız için resmi 24bppRgb formatına çeviriyoruz.
                // Bu formatta her piksel 3 byte (B, G, R) yer kaplar.
                Bitmap temp = new Bitmap(dialog.FileName);
                originalBitmap = new Bitmap(temp.Width, temp.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(originalBitmap))
                {
                    g.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                }

                pcbOriginal.Image = originalBitmap;
                pcbResult.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;

            int kernelSize = int.Parse(cmbMatrisBoyutu.SelectedItem.ToString().Substring(0, 1));
            string filterType = cmbFiltreTipi.SelectedItem.ToString();

            Bitmap resultBitmap = null;

            if (filterType.Contains("Mean"))
            {
                resultBitmap = ApplyMeanFilterFast(originalBitmap, kernelSize);
            }
            else if (filterType.Contains("Median"))
            {
                resultBitmap = ApplyMedianFilterFast(originalBitmap, kernelSize);
            }
            else if (filterType.Contains("Gauss"))
            {
                resultBitmap = ApplyGaussianFilterFast(originalBitmap, kernelSize);
            }

            pcbResult.Image = resultBitmap;
            this.Cursor = Cursors.Default;
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }

        // 1. MEAN (ORTALAMA) FİLTRESİ
        private Bitmap ApplyMeanFilterFast(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            // Verileri Kitle
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            // Bayt dizilerine kopyala
            int bytes = srcData.Stride * h;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];

            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData); 

            int stride = srcData.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, count = 0;

                    // Kernel Döngüsü
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int pY = y + ky;
                        // Y Sınır kontrolü
                        if (pY < 0 || pY >= h) continue;

                        int offsetY = pY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = x + kx;
                            // X Sınır kontrolü
                            if (pX < 0 || pX >= w) continue;

                            // Pikselin dizideki yerini bul (Her piksel 3 byte: B, G, R)
                            int idx = offsetY + (pX * 3);

                            bSum += buffer[idx];
                            gSum += buffer[idx + 1];
                            rSum += buffer[idx + 2];
                            count++;
                        }
                    }

                    // Sonucu yaz
                    int currentIdx = (y * stride) + (x * 3);
                    result[currentIdx] = (byte)(bSum / count);
                    result[currentIdx + 1] = (byte)(gSum / count);
                    result[currentIdx + 2] = (byte)(rSum / count);
                }
            }

            // Sonucu bitmab'e geri yükle
            Marshal.Copy(result, 0, dstData.Scan0, bytes);
            dstImage.UnlockBits(dstData);

            return dstImage;
        }

        // 2. MEDIAN (ORTANCA) FİLTRESİ
        private Bitmap ApplyMedianFilterFast(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);

            int stride = srcData.Stride;

            int maxPixels = kernelSize * kernelSize;
            byte[] rArray = new byte[maxPixels];
            byte[] gArray = new byte[maxPixels];
            byte[] bArray = new byte[maxPixels];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int count = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int pY = y + ky;
                        if (pY < 0 || pY >= h) continue;

                        int offsetY = pY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = x + kx;
                            if (pX < 0 || pX >= w) continue;

                            int idx = offsetY + (pX * 3);

                            bArray[count] = buffer[idx];
                            gArray[count] = buffer[idx + 1];
                            rArray[count] = buffer[idx + 2];
                            count++;
                        }
                    }

                    // Sıralama
                    Array.Sort(bArray, 0, count);
                    Array.Sort(gArray, 0, count);
                    Array.Sort(rArray, 0, count);

                    int mid = count / 2;
                    int currentIdx = (y * stride) + (x * 3);

                    result[currentIdx] = bArray[mid];
                    result[currentIdx + 1] = gArray[mid];
                    result[currentIdx + 2] = rArray[mid];
                }
            }

            Marshal.Copy(result, 0, dstData.Scan0, bytes);
            dstImage.UnlockBits(dstData);

            return dstImage;
        }

        // 3. GAUSSIAN (BULANIKLAŞTIRMA) FİLTRESİ
        private Bitmap ApplyGaussianFilterFast(Bitmap srcImage, int kernelSize)
        {
            // Kernel matrisini hesapla
            double[,] kernel = CalculateGaussianKernel(kernelSize, kernelSize / 3.0);
            return ApplyConvolutionFast(srcImage, kernel);
        }

        private Bitmap ApplyConvolutionFast(Bitmap srcImage, double[,] kernel)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);

            int stride = srcData.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int pY = y + ky;
                        // Sınır dışındaysa, en yakın sınır pikselini kullan (Clamp yöntemi)
                        // Veya sadece atla (Skip yöntemi). Burada clamp daha iyi sonuç verir ama basitlik için sınır kontrolü yapıyoruz.
                        if (pY < 0) pY = 0;
                        if (pY >= h) pY = h - 1;

                        int offsetY = pY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = x + kx;
                            if (pX < 0) pX = 0;
                            if (pX >= w) pX = w - 1;

                            int idx = offsetY + (pX * 3);
                            double weight = kernel[ky + radius, kx + radius];

                            bSum += buffer[idx] * weight;
                            gSum += buffer[idx + 1] * weight;
                            rSum += buffer[idx + 2] * weight;
                        }
                    }

                    int currentIdx = (y * stride) + (x * 3);

                    // Renkleri 0-255 arasına sıkıştır
                    result[currentIdx] = (byte)Math.Max(0, Math.Min(255, (int)bSum));
                    result[currentIdx + 1] = (byte)Math.Max(0, Math.Min(255, (int)gSum));
                    result[currentIdx + 2] = (byte)Math.Max(0, Math.Min(255, (int)rSum));
                }
            }

            Marshal.Copy(result, 0, dstData.Scan0, bytes);
            dstImage.UnlockBits(dstData);

            return dstImage;
        }

        // Gaussian çekirdek hesaplama 
        private double[,] CalculateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int radius = size / 2;
            double calculatedEuler = 1.0 / (2.0 * Math.PI * sigma * sigma);

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    double distance = (x * x + y * y);
                    double value = calculatedEuler * Math.Exp(-distance / (2.0 * sigma * sigma));
                    kernel[y + radius, x + radius] = value;
                    sum += value;
                }
            }

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    kernel[y, x] /= sum;

            return kernel;
        }
    }
}