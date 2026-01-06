using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices; 
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_BinaBulaniklastirmaForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnUygula, btnTemizle, btnGeri;
        private Label lblBilgi, lblBlurSiddeti;
        private TrackBar tbBlurSize;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap processedBitmap;
        private List<Point> polygonPoints = new List<Point>();

        public Proje5_BinaBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: Bina Netleştirme (Arka Plan Bulanıklaştırma)";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };
            pcbResim.MouseClick += PcbResim_MouseClick;
            pcbResim.Paint += PcbResim_Paint;

            int controlsY = 540;
            lblBilgi = new Label() { Text = "1. Resim yükleyin. 2. Binayı seçin. 3. 'Uygula' butonuna basın.", Location = new Point(25, controlsY), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY + 30), Size = new Size(120, 40) };
            btnYukle.Click += btnYukle_Click;

            btnTemizle = new Button() { Text = "Seçimi Sıfırla", Location = new Point(155, controlsY + 30), Size = new Size(120, 40) };
            btnTemizle.Click += btnTemizle_Click;

            lblBlurSiddeti = new Label() { Text = "Bulanıklık Şiddeti:", Location = new Point(300, controlsY + 15), AutoSize = true };
            tbBlurSize = new TrackBar() { Location = new Point(300, controlsY + 35), Size = new Size(150, 45), Minimum = 3, Maximum = 25, Value = 9, TickFrequency = 2, SmallChange = 2 };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(470, controlsY + 30), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += btnUygula_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(630, controlsY + 30), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += btnGeri_Click;

            this.Controls.AddRange(new Control[] { pcbResim, lblBilgi, btnYukle, btnTemizle, lblBlurSiddeti, tbBlurSize, btnUygula, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje5_BinaBulaniklastirmaForm";
            this.Size = new Size(820, 650);
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
                using (Bitmap tempImage = new Bitmap(dialog.FileName))
                {
                    int maxWidth = pcbResim.Width;
                    int maxHeight = pcbResim.Height;

                    // Oranları hesapla
                    float ratioX = (float)maxWidth / tempImage.Width;
                    float ratioY = (float)maxHeight / tempImage.Height;
                    float ratio = Math.Min(ratioX, ratioY); // En-boy oranını korumak için küçük olanı al

                    // KARAR ANI:
                    // Eğer oran < 1 ise resim kutudan BÜYÜKTÜR -> Küçültmemiz lazım.
                    // Eğer oran >= 1 ise resim KÜÇÜKTÜR -> Olduğu gibi kalsın ("az geliyorsa okey").

                    if (ratio < 1)
                    {
                        // Yeni boyutları hesapla
                        int newWidth = (int)(tempImage.Width * ratio);
                        int newHeight = (int)(tempImage.Height * ratio);

                        // Yeni (küçültülmüş) Bitmap oluştur (Format 24bpp - Hız için)
                        originalBitmap = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                        using (Graphics g = Graphics.FromImage(originalBitmap))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(tempImage, 0, 0, newWidth, newHeight);
                        }
                    }
                    else
                    {
                        // Resim küçükse, olduğu gibi al ama formatı standartlaştır
                        originalBitmap = new Bitmap(tempImage.Width, tempImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        using (Graphics g = Graphics.FromImage(originalBitmap))
                        {
                            g.DrawImage(tempImage, 0, 0);
                        }
                    }
                }

                // --- PictureBox Ayarı ---
                // CenterImage yapıyoruz ki küçük resimler ortada dursun,
                // büyük resimler zaten tam sığacak kadar küçültüldü.
                pcbResim.SizeMode = PictureBoxSizeMode.CenterImage;

                ResetSelection(); // Seçimleri ve ekranı sıfırla
            }
        }

        private void PcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || e.Button != MouseButtons.Left) return;
            polygonPoints.Add(e.Location);
            if (polygonPoints.Count >= 3) btnUygula.Enabled = true;
            pcbResim.Invalidate();
        }

        private void PcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (polygonPoints.Count > 0)
            {
                using (Pen pen = new Pen(Color.Yellow, 2) { DashStyle = DashStyle.Dash })
                {
                    if (polygonPoints.Count > 1)
                    {
                        e.Graphics.DrawLines(pen, polygonPoints.ToArray());
                        e.Graphics.DrawLine(Pens.Gray, polygonPoints[polygonPoints.Count - 1], polygonPoints[0]);
                    }
                    foreach (Point p in polygonPoints) e.Graphics.FillEllipse(Brushes.Red, p.X - 3, p.Y - 3, 6, 6);
                }
            }
        }

        private void btnTemizle_Click(object sender, EventArgs e) { ResetSelection(); }

        private void ResetSelection()
        {
            if (originalBitmap != null)
            {
                processedBitmap = new Bitmap(originalBitmap);
                pcbResim.Image = processedBitmap;
            }
            polygonPoints.Clear();
            btnUygula.Enabled = false;
            pcbResim.Invalidate();
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null || polygonPoints.Count < 3) return;

            this.Cursor = Cursors.WaitCursor;

            // 1. MASKE OLUŞTURMA (Mask Bitmap)
            Bitmap maskBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(maskBitmap))
            {
                g.Clear(Color.Black); // Arka plan (Bulanıklaşacak yerler) siyah

                // Koordinat dönüşümü
                List<PointF> imagePoints = new List<PointF>();
                foreach (Point p in polygonPoints) imagePoints.Add(ConvertPointToImage(p));

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddPolygon(imagePoints.ToArray());
                    g.FillPath(Brushes.White, path); // Bina (Net kalacak yer) beyaz
                }
            }

            // 2. BULANIKLAŞTIRMA (Hızlı Yöntem)
            int kernelSize = tbBlurSize.Value;
            if (kernelSize % 2 == 0) kernelSize++;

            processedBitmap = ApplyBlurWithMaskFast(originalBitmap, maskBitmap, kernelSize);

            pcbResim.Image = processedBitmap;
            this.Cursor = Cursors.Default;

            maskBitmap.Dispose(); // Maskeyi temizle
        }

        // HIZLI BLUR VE MASKELEME FONKSİYONU
        private Bitmap ApplyBlurWithMaskFast(Bitmap srcImage, Bitmap maskImage, int kernelSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;
            int radius = kernelSize / 2;

            Bitmap dstImage = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            // 3 Resmi de Belleğe Kilitle (Kaynak, Hedef ve Maske)
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData maskData = maskImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dstImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = srcData.Stride * h;
            byte[] srcBuffer = new byte[bytes];
            byte[] maskBuffer = new byte[bytes];
            byte[] dstBuffer = new byte[bytes];

            // Verileri RAM'e kopyala
            Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);
            Marshal.Copy(maskData.Scan0, maskBuffer, 0, bytes);

            int stride = srcData.Stride;

            // Piksel Döngüsü
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = (y * stride) + (x * 3);

                    // Maske Kontrolü (Kırmızı kanalına bakmak yeterli, Beyaz=255, Siyah=0)
                    // Eğer Beyazsa (maskBuffer[idx] > 128) -> Bina (NET KALACAK)
                    // Eğer Siyahsa -> Arka Plan (BULANIKLAŞACAK)

                    if (maskBuffer[idx] > 128)
                    {
                        // Aynen kopyala (Net)
                        dstBuffer[idx] = srcBuffer[idx];
                        dstBuffer[idx + 1] = srcBuffer[idx + 1];
                        dstBuffer[idx + 2] = srcBuffer[idx + 2];
                    }
                    else
                    {
                        // Bulanıklaştır (Mean Filter)
                        int rSum = 0, gSum = 0, bSum = 0, count = 0;

                        // Kernel Döngüsü (Sadece siyah alandaysak çalışır)
                        for (int ky = -radius; ky <= radius; ky++)
                        {
                            int nY = y + ky;
                            if (nY < 0 || nY >= h) continue;

                            int rowOffset = nY * stride;

                            for (int kx = -radius; kx <= radius; kx++)
                            {
                                int nX = x + kx;
                                if (nX < 0 || nX >= w) continue;

                                int kIdx = rowOffset + (nX * 3);

                                bSum += srcBuffer[kIdx];
                                gSum += srcBuffer[kIdx + 1];
                                rSum += srcBuffer[kIdx + 2];
                                count++;
                            }
                        }

                        dstBuffer[idx] = (byte)(bSum / count);
                        dstBuffer[idx + 1] = (byte)(gSum / count);
                        dstBuffer[idx + 2] = (byte)(rSum / count);
                    }
                }
            }

            // Sonucu geri yaz
            Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);

            srcImage.UnlockBits(srcData);
            maskImage.UnlockBits(maskData);
            dstImage.UnlockBits(dstData);

            return dstImage;
        }

        private PointF ConvertPointToImage(Point pcbPoint)
        {
            if (pcbResim.Image == null) return pcbPoint;
            float pcbAspect = (float)pcbResim.ClientSize.Width / pcbResim.ClientSize.Height;
            float imgAspect = (float)pcbResim.Image.Width / pcbResim.Image.Height;

            if (pcbAspect > imgAspect)
            {
                float scale = (float)pcbResim.ClientSize.Height / pcbResim.Image.Height;
                float offsetX = (pcbResim.ClientSize.Width - (pcbResim.Image.Width * scale)) / 2;
                return new PointF((pcbPoint.X - offsetX) / scale, pcbPoint.Y / scale);
            }
            else
            {
                float scale = (float)pcbResim.ClientSize.Width / pcbResim.Image.Width;
                float offsetY = (pcbResim.ClientSize.Height - (pcbResim.Image.Height * scale)) / 2;
                return new PointF(pcbPoint.X / scale, (pcbPoint.Y - offsetY) / scale);
            }
        }

        private void btnGeri_Click(object sender, EventArgs e) { this.Close(); }
        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }
    }
}