using System;
using System.Drawing;
using System.Drawing.Imaging; // LockBits için gerekli
using System.Runtime.InteropServices; // Marshal için gerekli
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_NetlestirmeMatrisBoyutuForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblOriginal, lblResult, lblMatrisBoyutu;
        private NumericUpDown nudMatrisBoyutu;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje2_NetlestirmeMatrisBoyutuForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Netleştirmede Matris Boyutunun Etkisi";

            int pcbSize = 350;
            int margin = 25;

            // --- Arayüz Elemanları ---
            lblOriginal = new Label() { Text = "Orijinal (Bulanık) Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Netleştirilmiş Sonuç", Location = new Point(margin * 2 + pcbSize + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 30;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblMatrisBoyutu = new Label() { Text = "Matris Boyutu (Kernel Size):", Location = new Point(200, controlsY + 12), AutoSize = true };

            nudMatrisBoyutu = new NumericUpDown()
            {
                Location = new Point(370, controlsY + 10),
                Width = 60,
                Minimum = 3,
                Maximum = 51,
                Value = 3,
                Increment = 2
            };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(450, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            // lblInfo kodu buradan silindi.

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblMatrisBoyutu, nudMatrisBoyutu, btnUygula, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_NetlestirmeMatrisBoyutuForm";
            this.Size = new Size(850, 550);
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
                // Hızlı işlem için resmi 24bppRgb formatına çevirerek yüklüyoruz
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
            int kernelSize = (int)nudMatrisBoyutu.Value;

            if (kernelSize % 2 == 0) kernelSize++;

            // 1. Adım: Hızlı Bulanıklaştırma (Mean Filter)
            Bitmap blurredBitmap = ApplyMeanFilterFast(originalBitmap, kernelSize);

            // 2. Adım: Hızlı Netleştirme (Unsharp Mask)
            Bitmap sharpenedBitmap = ApplyUnsharpMaskFast(originalBitmap, blurredBitmap, 1.5);

            pcbResult.Image = sharpenedBitmap;
            this.Cursor = Cursors.Default;
        }

        // --- HIZLANDIRILMIŞ FONKSİYONLAR ---

        private Bitmap ApplyMeanFilterFast(Bitmap srcImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            srcImage.UnlockBits(srcData); // Kaynak kilidini hemen açabiliriz

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
                        if (pY < 0 || pY >= h) continue;

                        int offsetY = pY * stride;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = x + kx;
                            if (pX < 0 || pX >= w) continue;

                            int idx = offsetY + (pX * 3);

                            bSum += srcBuffer[idx];
                            gSum += srcBuffer[idx + 1];
                            rSum += srcBuffer[idx + 2];
                            count++;
                        }
                    }

                    int currentIdx = (y * stride) + (x * 3);
                    dstBuffer[currentIdx] = (byte)(bSum / count);
                    dstBuffer[currentIdx + 1] = (byte)(gSum / count);
                    dstBuffer[currentIdx + 2] = (byte)(rSum / count);
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
            dstImage.UnlockBits(dstData);

            return dstImage;
        }

        private Bitmap ApplyUnsharpMaskFast(Bitmap original, Bitmap blurred, double amount)
        {
            int w = original.Width;
            int h = original.Height;
            Bitmap result = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData orgData = original.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData blurData = blurred.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData resData = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = orgData.Stride * h;
            byte[] orgBuffer = new byte[bytes];
            byte[] blurBuffer = new byte[bytes];
            byte[] resBuffer = new byte[bytes];

            Marshal.Copy(orgData.Scan0, orgBuffer, 0, bytes);
            Marshal.Copy(blurData.Scan0, blurBuffer, 0, bytes);

            int stride = orgData.Stride;

            // Tek döngüde tüm pikselleri işle
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = (y * stride) + (x * 3);

                    // Mavi (B)
                    int valB = (int)(orgBuffer[idx] + (orgBuffer[idx] - blurBuffer[idx]) * amount);
                    resBuffer[idx] = (byte)Math.Max(0, Math.Min(255, valB));

                    // Yeşil (G)
                    int valG = (int)(orgBuffer[idx + 1] + (orgBuffer[idx + 1] - blurBuffer[idx + 1]) * amount);
                    resBuffer[idx + 1] = (byte)Math.Max(0, Math.Min(255, valG));

                    // Kırmızı (R)
                    int valR = (int)(orgBuffer[idx + 2] + (orgBuffer[idx + 2] - blurBuffer[idx + 2]) * amount);
                    resBuffer[idx + 2] = (byte)Math.Max(0, Math.Min(255, valR));
                }
            }

            Marshal.Copy(resBuffer, 0, resData.Scan0, bytes);

            original.UnlockBits(orgData);
            blurred.UnlockBits(blurData);
            result.UnlockBits(resData);

            return result;
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