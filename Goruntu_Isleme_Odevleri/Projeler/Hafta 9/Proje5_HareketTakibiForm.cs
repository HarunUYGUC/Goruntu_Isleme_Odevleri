using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_HareketTakibiForm : Form
    {
        // UI Elemanları
        private PictureBox pcbArkaPlan, pcbHareketli, pcbSonuc;
        private Label lblArkaPlan, lblHareketli, lblSonuc, lblBilgi, lblSapma;
        private Button btnArkaPlanYukle, btnHareketliYukle, btnHesapla, btnGeri;
        private NumericUpDown nudEsik; // Hassasiyet ayarı

        private Bitmap bmpArkaPlan;
        private Bitmap bmpHareketli;
        private Form haftaFormu;

        public Proje5_HareketTakibiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: Hareket Sensörü ve Hedef Takibi";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();
        }

        private void InitializeComponent()
        {
            this.Name = "Proje5_HareketTakibiForm";
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void SetupUI()
        {
            int pcbW = 320; int pcbH = 240; int gap = 20; int startX = 20; int startY = 40;

            // 1. Arka Plan (Referans)
            lblArkaPlan = new Label() { Text = "1. Referans (Arka Plan)", Location = new Point(startX, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbArkaPlan = CreateBox(startX, startY, pcbW, pcbH);
            btnArkaPlanYukle = new Button() { Text = "Arka Plan Yükle", Location = new Point(startX, startY + pcbH + 10), Size = new Size(120, 35) };
            btnArkaPlanYukle.Click += (s, e) => LoadImage(ref bmpArkaPlan, pcbArkaPlan);

            // 2. Hareketli Görüntü (Hedef)
            int x2 = startX + pcbW + gap;
            lblHareketli = new Label() { Text = "2. Anlık Görüntü (Hedefli)", Location = new Point(x2, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbHareketli = CreateBox(x2, startY, pcbW, pcbH);
            btnHareketliYukle = new Button() { Text = "Görüntü Yükle", Location = new Point(x2, startY + pcbH + 10), Size = new Size(120, 35) };
            btnHareketliYukle.Click += (s, e) => LoadImage(ref bmpHareketli, pcbHareketli);

            // 3. Sonuç (Binary + Crosshair)
            int x3 = x2 + pcbW + gap;
            lblSonuc = new Label() { Text = "3. Tespit Edilen Hedef", Location = new Point(x3, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbSonuc = CreateBox(x3, startY, pcbW, pcbH);
            pcbSonuc.BackColor = Color.Black;

            // Kontroller ve Bilgi Paneli
            int ctrlY = startY + pcbH + 60;

            Label lblEsik = new Label() { Text = "Hareket Hassasiyeti (Eşik):", Location = new Point(startX, ctrlY), AutoSize = true };
            nudEsik = new NumericUpDown() { Location = new Point(startX + 160, ctrlY - 2), Value = 30, Minimum = 1, Maximum = 255 };

            btnHesapla = new Button() { Text = "HEDEFİ BUL VE KİLİTLE", Location = new Point(startX, ctrlY + 30), Size = new Size(200, 50), BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnHesapla.Click += BtnHesapla_Click;

            lblSapma = new Label()
            {
                Text = "Hedef Sapması:\nX: 0 px (Yatay)\nY: 0 px (Dikey)",
                Location = new Point(x2, ctrlY),
                AutoSize = true,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            lblBilgi = new Label()
            {
                Text = "Mantık: İki resim arasındaki farkı alır.\nBeyaz piksellerin ağırlık merkezini (Hedef) bulur.\nKırmızı Artı: Hedef Merkezi\nMavi Artı: Kamera (Silah) Ekseni",
                Location = new Point(x3, ctrlY),
                AutoSize = true,
                Font = new Font("Arial", 9),
                ForeColor = Color.DimGray
            };

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(this.Width - 140, this.Height - 80), Size = new Size(100, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblArkaPlan, pcbArkaPlan, btnArkaPlanYukle,
                lblHareketli, pcbHareketli, btnHareketliYukle,
                lblSonuc, pcbSonuc,
                lblEsik, nudEsik, btnHesapla, lblSapma, lblBilgi, btnGeri
            });
        }

        private PictureBox CreateBox(int x, int y, int w, int h)
        {
            return new PictureBox()
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderStyle = BorderStyle.Fixed3D,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.WhiteSmoke
            };
        }

        private void LoadImage(ref Bitmap targetBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap temp = new Bitmap(ofd.FileName);
                // İşlem kolaylığı için resimleri standart boyuta (320x240) çekiyoruz
                targetBmp = new Bitmap(temp, new Size(320, 240));
                temp.Dispose();

                // Formatı standartlaştır
                if (targetBmp.PixelFormat != PixelFormat.Format24bppRgb)
                {
                    Bitmap clone = new Bitmap(targetBmp.Width, targetBmp.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(clone)) g.DrawImage(targetBmp, 0, 0, 320, 240);
                    targetBmp = clone;
                }
                pcb.Image = targetBmp;
            }
        }

        private void BtnHesapla_Click(object sender, EventArgs e)
        {
            if (bmpArkaPlan == null || bmpHareketli == null)
            {
                MessageBox.Show("Lütfen her iki resmi de yükleyiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int w = bmpArkaPlan.Width;
            int h = bmpArkaPlan.Height;
            int threshold = (int)nudEsik.Value;

            // Sonuç bitmap'i (Binary görüntü için)
            Bitmap resultBmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            // Verileri Kilitle (Hızlı Erişim)
            BitmapData data1 = bmpArkaPlan.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData data2 = bmpHareketli.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dataRes = resultBmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = data1.Stride * h;
            byte[] buffer1 = new byte[bytes];
            byte[] buffer2 = new byte[bytes];
            byte[] bufferRes = new byte[bytes]; // Varsayılan siyah (0)

            Marshal.Copy(data1.Scan0, buffer1, 0, bytes);
            Marshal.Copy(data2.Scan0, buffer2, 0, bytes);

            long totalX = 0;
            long totalY = 0;
            long pixelCount = 0;

            int stride = data1.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = (y * stride) + (x * 3);

                    // Piksel Farkını Hesapla (Manhattan Distance: |R1-R2| + |G1-G2| + |B1-B2|)
                    int diffB = Math.Abs(buffer1[idx] - buffer2[idx]);
                    int diffG = Math.Abs(buffer1[idx + 1] - buffer2[idx + 1]);
                    int diffR = Math.Abs(buffer1[idx + 2] - buffer2[idx + 2]);

                    int totalDiff = diffB + diffG + diffR;

                    // Eğer fark eşik değerden büyükse -> HAREKET VAR (BEYAZ YAP)
                    if (totalDiff > threshold)
                    {
                        bufferRes[idx] = 255;     // B
                        bufferRes[idx + 1] = 255; // G
                        bufferRes[idx + 2] = 255; // R

                        // Koordinatları topla (Ağırlık merkezi için)
                        totalX += x;
                        totalY += y;
                        pixelCount++;
                    }
                    else
                    {
                        // Hareket yok -> SİYAH
                        bufferRes[idx] = 0;
                        bufferRes[idx + 1] = 0;
                        bufferRes[idx + 2] = 0;
                    }
                }
            }

            Marshal.Copy(bufferRes, 0, dataRes.Scan0, bytes);

            bmpArkaPlan.UnlockBits(data1);
            bmpHareketli.UnlockBits(data2);
            resultBmp.UnlockBits(dataRes);

            // --- MERKEZ VE SAPMA HESABI ---

            // Eğer hareketli piksel bulunduysa
            if (pixelCount > 0)
            {
                int centerX = (int)(totalX / pixelCount);
                int centerY = (int)(totalY / pixelCount);

                // Resmin Merkezi (Kamera Ekseni)
                int camCenterX = w / 2;
                int camCenterY = h / 2;

                // Sapma Miktarı
                int deviationX = centerX - camCenterX; // (+) ise sağda, (-) ise solda
                int deviationY = centerY - camCenterY; // (+) ise aşağıda, (-) ise yukarıda

                lblSapma.Text = $"Hedef Sapması:\nX: {deviationX} px ({(deviationX > 0 ? "Sağ" : "Sol")})\nY: {deviationY} px ({(deviationY > 0 ? "Aşağı" : "Yukarı")})";

                // --- ÇİZİM İŞLEMLERİ (Crosshair) ---
                using (Graphics g = Graphics.FromImage(resultBmp))
                {
                    // 1. Kamera Ekseni (MAVİ - Sabit Merkez)
                    Pen bluePen = new Pen(Color.Cyan, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
                    g.DrawLine(bluePen, camCenterX, 0, camCenterX, h);
                    g.DrawLine(bluePen, 0, camCenterY, w, camCenterY);

                    // 2. Hedef Merkezi (KIRMIZI - Hareketli)
                    int crossSize = 20;
                    Pen redPen = new Pen(Color.Red, 3);
                    g.DrawLine(redPen, centerX - crossSize, centerY, centerX + crossSize, centerY);
                    g.DrawLine(redPen, centerX, centerY - crossSize, centerX, centerY + crossSize);

                    // Hedef dairesi
                    g.DrawEllipse(redPen, centerX - 15, centerY - 15, 30, 30);
                }
            }
            else
            {
                lblSapma.Text = "Hedef Sapması:\nHareket Algılanamadı!";
            }

            pcbSonuc.Image = resultBmp;
        }
    }
}