using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging; 
using System.Linq;
using System.Runtime.InteropServices; 
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje7_ModFiltresiForm : Form
    {
        private PictureBox pcbOriginal, pcbMode, pcbMean, pcbMedian;
        private Label lblOriginal, lblMode, lblMean, lblMedian, lblKernel;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbKernelSize;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje7_ModFiltresiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 7: Mod Filtresi";

            int pcbSize = 300;
            int margin = 20;

            lblOriginal = new Label() { Text = "1. Orijinal Resim", Location = new Point(margin + 100, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblMode = new Label() { Text = "2. Mod Filtresi (Hızlı)", Location = new Point(margin * 2 + pcbSize + 50, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Blue };
            pcbMode = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.Fixed3D, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int row2Y = margin + 25 + pcbSize + 30;

            lblMean = new Label() { Text = "3. Mean (Ortalama)", Location = new Point(margin + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMean = new PictureBox() { Location = new Point(margin, row2Y + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblMedian = new Label() { Text = "4. Median (Ortanca)", Location = new Point(margin * 2 + pcbSize + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMedian = new PictureBox() { Location = new Point(margin * 2 + pcbSize, row2Y + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsX = margin * 3 + pcbSize * 2;
            int controlsY = margin + 50;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(controlsX, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += btnYukle_Click;

            lblKernel = new Label() { Text = "Matris Boyutu:", Location = new Point(controlsX, controlsY + 60), AutoSize = true };
            cmbKernelSize = new ComboBox() { Location = new Point(controlsX + 90, controlsY + 58), DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 };
            cmbKernelSize.Items.AddRange(new object[] { "3x3", "5x5", "7x7", "9x9" });
            cmbKernelSize.SelectedIndex = 0;

            btnUygula = new Button() { Text = "Filtreleri Uygula", Location = new Point(controlsX, controlsY + 100), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += btnUygula_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(controlsX, controlsY + 160), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += btnGeri_Click;

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblMode, pcbMode,
                lblMean, pcbMean, lblMedian, pcbMedian,
                btnYukle, lblKernel, cmbKernelSize, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje7_ModFiltresiForm";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void btnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap temp = new Bitmap(dialog.FileName))
                {
                    // 1. Ölçekleme Oranını Hesapla (Resim kutuya sığsın)
                    // PictureBox'ın genişliği ve yüksekliğine göre en uygun oranı buluyoruz.
                    float scale = Math.Min((float)pcbOriginal.Width / temp.Width, (float)pcbOriginal.Height / temp.Height);

                    // Eğer resim zaten kutudan küçükse (scale > 1), büyütme yapma, olduğu gibi kalsın.
                    if (scale > 1) scale = 1;

                    int newW = (int)(temp.Width * scale);
                    int newH = (int)(temp.Height * scale);

                    // 2. Yeni Boyutlarda ve Hızlı Formatta (24bpp) Bitmap Oluştur
                    originalBitmap = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);

                    // 3. Resmi Yeni Boyuta Çiz (Resize İşlemi)
                    using (Graphics g = Graphics.FromImage(originalBitmap))
                    {
                        // Kaliteli küçültme ayarı (Resim bozulmasın diye)
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(temp, 0, 0, newW, newH);
                    }
                }

                // PictureBox ayarını CenterImage yapıyoruz, çünkü resmi zaten biz kodla küçülttük.
                pcbOriginal.SizeMode = PictureBoxSizeMode.CenterImage;
                pcbOriginal.Image = originalBitmap;

                // Diğer kutuları temizle
                pcbMode.Image = null;
                pcbMean.Image = null;
                pcbMedian.Image = null;

                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;
            int kernelSize = int.Parse(cmbKernelSize.SelectedItem.ToString().Substring(0, 1));

            // 1. Mod Filtresi (Hızlı)
            pcbMode.Image = ApplyModeFilterFast(originalBitmap, kernelSize);

            // 2. Mean (Hızlı)
            pcbMean.Image = ApplyMeanFilterFast(originalBitmap, kernelSize);

            // 3. Median (Hızlı)
            pcbMedian.Image = ApplyMedianFilterFast(originalBitmap, kernelSize);

            this.Cursor = Cursors.Default;
        }

        // --- HIZLI MODE FİLTRESİ (Marshal.Copy) ---
        private Bitmap ApplyModeFilterFast(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int r = kernelSize / 2;
            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            int stride = srcData.Stride;

            // Piksel Döngüsü
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Her piksel için frekans sözlüğü
                    Dictionary<int, int> freq = new Dictionary<int, int>();

                    // Kernel Döngüsü
                    for (int ky = -r; ky <= r; ky++)
                    {
                        int ny = y + ky;
                        if (ny < 0) ny = 0; if (ny >= h) ny = h - 1;

                        int rowStart = ny * stride;

                        for (int kx = -r; kx <= r; kx++)
                        {
                            int nx = x + kx;
                            if (nx < 0) nx = 0; if (nx >= w) nx = w - 1;

                            int i = rowStart + nx * 3;

                            // Renk kodunu tek bir INT olarak sakla (RGB)
                            // Bu sayede Dictionary key'i olarak Color nesnesi yerine int kullanırız (Daha hızlı)
                            int colorCode = (srcBuffer[i] << 16) | (srcBuffer[i + 1] << 8) | srcBuffer[i + 2];

                            if (freq.ContainsKey(colorCode)) freq[colorCode]++;
                            else freq[colorCode] = 1;
                        }
                    }

                    // En sık tekrar edeni bul
                    int modeColor = freq.OrderByDescending(k => k.Value).First().Key;

                    // Geri Yaz
                    int idx = y * stride + x * 3;
                    dstBuffer[idx] = (byte)((modeColor >> 16) & 0xFF);     // Blue
                    dstBuffer[idx + 1] = (byte)((modeColor >> 8) & 0xFF);  // Green
                    dstBuffer[idx + 2] = (byte)(modeColor & 0xFF);         // Red
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            return dstImage;
        }

        // --- HIZLI MEAN FİLTRESİ ---
        private Bitmap ApplyMeanFilterFast(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width; int h = srcImage.Height; int r = kernelSize / 2;
            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);

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
                    int sumB = 0, sumG = 0, sumR = 0, count = 0;
                    for (int ky = -r; ky <= r; ky++)
                    {
                        int ny = y + ky;
                        if (ny < 0) ny = 0; if (ny >= h) ny = h - 1;
                        int rowStart = ny * stride;

                        for (int kx = -r; kx <= r; kx++)
                        {
                            int nx = x + kx;
                            if (nx < 0) nx = 0; if (nx >= w) nx = w - 1;

                            int i = rowStart + nx * 3;
                            sumB += srcBuffer[i];
                            sumG += srcBuffer[i + 1];
                            sumR += srcBuffer[i + 2];
                            count++;
                        }
                    }
                    int idx = y * stride + x * 3;
                    dstBuffer[idx] = (byte)(sumB / count);
                    dstBuffer[idx + 1] = (byte)(sumG / count);
                    dstBuffer[idx + 2] = (byte)(sumR / count);
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            return dstImage;
        }

        // --- HIZLI MEDIAN FİLTRESİ ---
        private Bitmap ApplyMedianFilterFast(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width; int h = srcImage.Height; int r = kernelSize / 2;
            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            int stride = srcData.Stride;

            // Her döngüde dizi oluşturmamak için dışarıda tanımlayıp clear edebiliriz ama
            // basitlik için içeride bırakalım, GC (Garbage Collector) bunu yönetir.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    List<byte> listB = new List<byte>();
                    List<byte> listG = new List<byte>();
                    List<byte> listR = new List<byte>();

                    for (int ky = -r; ky <= r; ky++)
                    {
                        int ny = y + ky;
                        if (ny < 0) ny = 0; if (ny >= h) ny = h - 1;
                        int rowStart = ny * stride;

                        for (int kx = -r; kx <= r; kx++)
                        {
                            int nx = x + kx;
                            if (nx < 0) nx = 0; if (nx >= w) nx = w - 1;

                            int i = rowStart + nx * 3;
                            listB.Add(srcBuffer[i]);
                            listG.Add(srcBuffer[i + 1]);
                            listR.Add(srcBuffer[i + 2]);
                        }
                    }

                    listB.Sort(); listG.Sort(); listR.Sort();
                    int mid = listB.Count / 2;

                    int idx = y * stride + x * 3;
                    dstBuffer[idx] = listB[mid];
                    dstBuffer[idx + 1] = listG[mid];
                    dstBuffer[idx + 2] = listR[mid];
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            return dstImage;
        }

        private void btnGeri_Click(object sender, EventArgs e) { this.Close(); }
        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e) { haftaFormu.Show(); }
    }
}