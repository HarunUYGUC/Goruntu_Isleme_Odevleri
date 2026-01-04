using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_OzelBulaniklastirmaForm : Form
    {
        private PictureBox pcbOriginal, pcbMean, pcbMedian, pcbGauss, pcbCustom;
        private Label lblOriginal, lblMean, lblMedian, lblGauss, lblCustom;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbKernelSize;
        private Label lblKernel;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje2_OzelBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Özel Bulanıklaştırma Algoritması";

            int pcbSize = 250;
            int margin = 15;
            int labelOffset = 20;

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 80, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblCustom = new Label() { Text = "Özel Algoritma (Conservative Smoothing)", Location = new Point(margin * 2 + pcbSize + 10, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Blue };
            pcbCustom = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.Fixed3D, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int row2Y = margin + labelOffset + pcbSize + 30;

            lblMean = new Label() { Text = "Mean (Ortalama)", Location = new Point(margin + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMean = new PictureBox() { Location = new Point(margin, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblMedian = new Label() { Text = "Median (Ortanca)", Location = new Point(margin * 2 + pcbSize + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMedian = new PictureBox() { Location = new Point(margin * 2 + pcbSize, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblGauss = new Label() { Text = "Gauss (Gaussian)", Location = new Point(margin * 3 + pcbSize * 2 + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbGauss = new PictureBox() { Location = new Point(margin * 3 + pcbSize * 2, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsX = margin * 3 + pcbSize * 2;
            int controlsY = margin + 50;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(controlsX, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblKernel = new Label() { Text = "Matris Boyutu:", Location = new Point(controlsX, controlsY + 60), AutoSize = true };
            cmbKernelSize = new ComboBox() { Location = new Point(controlsX + 90, controlsY + 58), DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 };
            cmbKernelSize.Items.AddRange(new object[] { "3x3", "5x5", "7x7", "9x9" });
            cmbKernelSize.SelectedIndex = 0;

            btnUygula = new Button() { Text = "Filtreleri Uygula", Location = new Point(controlsX, controlsY + 100), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(controlsX, controlsY + 160), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblCustom, pcbCustom,
                lblMean, pcbMean, lblMedian, pcbMedian, lblGauss, pcbGauss,
                btnYukle, lblKernel, cmbKernelSize, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_OzelBulaniklastirmaForm";
            this.Size = new Size(850, 700);
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
                // Diğer kutuları temizle
                pcbMean.Image = null; pcbMedian.Image = null; pcbGauss.Image = null; pcbCustom.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;

            int kernelSize = int.Parse(cmbKernelSize.SelectedItem.ToString().Substring(0, 1));

            Bitmap src = new Bitmap(originalBitmap);
            if (src.PixelFormat != PixelFormat.Format24bppRgb)
            {
                Bitmap temp = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(temp))
                {
                    g.DrawImage(src, 0, 0, src.Width, src.Height);
                }
                src = temp;
            }

            pcbCustom.Image = ApplyConservativeSmoothingSafe(src, kernelSize);
            pcbMean.Image = ApplyMeanFilterSafe(src, kernelSize);
            pcbMedian.Image = ApplyMedianFilterSafe(src, kernelSize);
            pcbGauss.Image = ApplyGaussianFilterSafe(src, kernelSize);

            this.Cursor = Cursors.Default;
        }

        // Conservative Smoothing (DİNAMİK BOYUTLU - HIZLI)
        private Bitmap ApplyConservativeSmoothingSafe(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;

            // Seçilen boyuta göre yarıçap belirleniyor
            // 3x3 için radius=1, 9x9 için radius=4 olur.
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, srcImage.PixelFormat);
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);

            int stride = srcData.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int kIndex = (y * stride) + (x * 3);

                    byte b = srcBuffer[kIndex];
                    byte g = srcBuffer[kIndex + 1];
                    byte r = srcBuffer[kIndex + 2];

                    byte minR = 255, maxR = 0;
                    byte minG = 255, maxG = 0;
                    byte minB = 255, maxB = 0;

                    bool neighborFound = false;

                    // KERNEL DÖNGÜSÜ: -radius değerinden +radius değerine kadar döner.
                    // 9x9 seçtiğinizde burası -4'ten +4'e kadar geniş bir alana bakar.
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int nY = y + ky;
                        if (nY < 0 || nY >= h) continue;

                        int yOffset = nY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            if (kx == 0 && ky == 0) continue; // Merkez pikseli dahil etme

                            int nX = x + kx;
                            if (nX < 0 || nX >= w) continue;

                            int nIndex = yOffset + (nX * 3);

                            byte nb = srcBuffer[nIndex];
                            byte ng = srcBuffer[nIndex + 1];
                            byte nr = srcBuffer[nIndex + 2];

                            // Komşulardaki Min ve Max değerleri bul
                            if (nr < minR) minR = nr; if (nr > maxR) maxR = nr;
                            if (ng < minG) minG = ng; if (ng > maxG) maxG = ng;
                            if (nb < minB) minB = nb; if (nb > maxB) maxB = nb;

                            neighborFound = true;
                        }
                    }

                    if (neighborFound)
                    {
                        // Eğer merkez piksel komşuların max değerinden büyükse, max'a eşitle (Clamp)
                        // Eğer merkez piksel komşuların min değerinden küçükse, min'e eşitle
                        dstBuffer[kIndex + 2] = (r > maxR) ? maxR : (r < minR ? minR : r);
                        dstBuffer[kIndex + 1] = (g > maxG) ? maxG : (g < minG ? minG : g);
                        dstBuffer[kIndex] = (b > maxB) ? maxB : (b < minB ? minB : b);
                    }
                    else
                    {
                        // Kenar pikseli ise işlem yapma
                        dstBuffer[kIndex + 2] = r;
                        dstBuffer[kIndex + 1] = g;
                        dstBuffer[kIndex] = b;
                    }
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            return dstImage;
        }

        // MEAN (ORTALAMA) FİLTRESİ (BYTE ARRAY / SAFE)
        private Bitmap ApplyMeanFilterSafe(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, srcImage.PixelFormat);
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            int stride = srcData.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, count = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int nY = y + ky;
                        if (nY < 0 || nY >= h) continue;

                        int yOffset = nY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nX = x + kx;
                            if (nX < 0 || nX >= w) continue;

                            int nIndex = yOffset + (nX * 3);

                            bSum += srcBuffer[nIndex];
                            gSum += srcBuffer[nIndex + 1];
                            rSum += srcBuffer[nIndex + 2];
                            count++;
                        }
                    }

                    int kIndex = (y * stride) + (x * 3);
                    dstBuffer[kIndex] = (byte)(bSum / count);
                    dstBuffer[kIndex + 1] = (byte)(gSum / count);
                    dstBuffer[kIndex + 2] = (byte)(rSum / count);
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            return dstImage;
        }

        // MEDIAN (ORTANCA) FİLTRESİ (BYTE ARRAY / SAFE)
        private Bitmap ApplyMedianFilterSafe(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int radius = kernelSize / 2;

            // Performans için listeleri döngü dışında değil içinde tanımlıyoruz veya array kullanıyoruz
            int maxItems = kernelSize * kernelSize;

            Bitmap dstImage = new Bitmap(w, h, srcImage.PixelFormat);
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            int stride = srcData.Stride;

            // Her piksel için yeniden kullanacağımız diziler
            byte[] rArr = new byte[maxItems];
            byte[] gArr = new byte[maxItems];
            byte[] bArr = new byte[maxItems];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int count = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int nY = y + ky;
                        if (nY < 0 || nY >= h) continue;
                        int yOffset = nY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nX = x + kx;
                            if (nX < 0 || nX >= w) continue;

                            int idx = yOffset + (nX * 3);
                            bArr[count] = srcBuffer[idx];
                            gArr[count] = srcBuffer[idx + 1];
                            rArr[count] = srcBuffer[idx + 2];
                            count++;
                        }
                    }

                    Array.Sort(bArr, 0, count);
                    Array.Sort(gArr, 0, count);
                    Array.Sort(rArr, 0, count);

                    int mid = count / 2;
                    int kIndex = (y * stride) + (x * 3);

                    dstBuffer[kIndex] = bArr[mid];
                    dstBuffer[kIndex + 1] = gArr[mid];
                    dstBuffer[kIndex + 2] = rArr[mid];
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            return dstImage;
        }

        // GAUSSIAN (GAUSS) FİLTRESİ (BYTE ARRAY / SAFE)
        private Bitmap ApplyGaussianFilterSafe(Bitmap srcImage, int kernelSize)
        {
            double[,] kernel = CalculateGaussianKernel(kernelSize, kernelSize / 3.0);

            int w = srcImage.Width;
            int h = srcImage.Height;
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, srcImage.PixelFormat);
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            int stride = srcData.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int nY = y + ky;
                        // Clamp (Kenar taşmalarını en yakın pikselle doldurma)
                        if (nY < 0) nY = 0;
                        if (nY >= h) nY = h - 1;

                        int yOffset = nY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nX = x + kx;
                            if (nX < 0) nX = 0;
                            if (nX >= w) nX = w - 1;

                            int idx = yOffset + (nX * 3);
                            double weight = kernel[ky + radius, kx + radius];

                            bSum += srcBuffer[idx] * weight;
                            gSum += srcBuffer[idx + 1] * weight;
                            rSum += srcBuffer[idx + 2] * weight;
                        }
                    }

                    int kIndex = (y * stride) + (x * 3);

                    dstBuffer[kIndex] = (byte)Math.Min(255, Math.Max(0, bSum));
                    dstBuffer[kIndex + 1] = (byte)Math.Min(255, Math.Max(0, gSum));
                    dstBuffer[kIndex + 2] = (byte)Math.Min(255, Math.Max(0, rSum));
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            return dstImage;
        }

        // Gauss Kernel Hesaplama (Aynı kalacak)
        private double[,] CalculateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int radius = size / 2;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    double value = (1.0 / (2.0 * Math.PI * sigma * sigma)) * Math.Exp(-(x * x + y * y) / (2.0 * sigma * sigma));
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