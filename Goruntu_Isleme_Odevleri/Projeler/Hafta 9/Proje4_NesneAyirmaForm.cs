using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_NesneAyirmaForm : Form
    {
        // UI Elemanları
        private PictureBox pcbBackground, pcbObject, pcbResult;
        private Label lblBg, lblObj, lblRes, lblThreshold, lblInfo;
        private Button btnLoadBg, btnLoadObj, btnProcess, btnBack;
        private TrackBar tbThreshold;

        // Resim Değişkenleri
        private Bitmap bmpBackground, bmpObject;
        private Form haftaFormu;

        public Proje4_NesneAyirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Değişken Işıkta Nesne Ayırma (Arka Plan Çıkarma)";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void InitializeComponent() { this.Name = "Proje4_NesneAyirmaForm"; }

        private void SetupUI()
        {
            int margin = 20;
            int pcbSize = 320;

            // 1. Arka Plan (Boş Kağıt)
            lblBg = new Label() { Text = "1. Arka Plan (Boş/Gölgeli)", Location = new Point(margin, 20), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            pcbBackground = new PictureBox() { Location = new Point(margin, 45), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom };
            btnLoadBg = new Button() { Text = "Arka Plan Yükle", Location = new Point(margin, 45 + pcbSize + 10), Size = new Size(pcbSize, 35) };
            btnLoadBg.Click += (s, e) => LoadImage(ref bmpBackground, pcbBackground);

            // 2. Nesneli Görüntü
            int x2 = margin + pcbSize + 20;
            lblObj = new Label() { Text = "2. Nesneli Görüntü", Location = new Point(x2, 20), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            pcbObject = new PictureBox() { Location = new Point(x2, 45), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom };
            btnLoadObj = new Button() { Text = "Nesne Yükle", Location = new Point(x2, 45 + pcbSize + 10), Size = new Size(pcbSize, 35) };
            btnLoadObj.Click += (s, e) => LoadImage(ref bmpObject, pcbObject);

            // 3. Sonuç (Binary)
            int x3 = x2 + pcbSize + 20;
            lblRes = new Label() { Text = "3. Tespit Sonucu (Siyah/Beyaz)", Location = new Point(x3, 20), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(x3, 45), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };

            // Kontroller
            int ctrlY = 45 + pcbSize + 10;

            lblThreshold = new Label() { Text = "Hassasiyet Eşiği: 30", Location = new Point(x3, ctrlY - 25), AutoSize = true };
            tbThreshold = new TrackBar() { Location = new Point(x3, ctrlY), Size = new Size(pcbSize, 45), Minimum = 5, Maximum = 150, Value = 30, TickFrequency = 10 };
            tbThreshold.Scroll += (s, e) => lblThreshold.Text = $"Hassasiyet Eşiği: {tbThreshold.Value}";

            btnProcess = new Button() { Text = "NESNEYİ AYIKLA", Location = new Point(x3, ctrlY + 50), Size = new Size(pcbSize, 40), BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnProcess.Click += BtnProcess_Click;

            btnBack = new Button() { Text = "Geri Dön", Location = new Point(x3, ctrlY + 100), Size = new Size(pcbSize, 30), BackColor = Color.LightCoral };
            btnBack.Click += (s, e) => this.Close();

            lblInfo = new Label()
            {
                Text = "İPUCU: Önce boş kağıdı (mum ışığında), sonra cisim konmuş halini yükleyin. \nİki resim arasındaki fark alınarak gölgeler yok edilecek.",
                Location = new Point(margin, ctrlY + 60),
                Size = new Size(x2, 60),
                ForeColor = Color.Blue,
                Font = new Font("Arial", 10)
            };

            this.Controls.AddRange(new Control[] {
                lblBg, pcbBackground, btnLoadBg,
                lblObj, pcbObject, btnLoadObj,
                lblRes, pcbResult,
                lblThreshold, tbThreshold, btnProcess, btnBack, lblInfo
            });
        }

        // BU FONKSİYONU MEVCUT LoadImage YERİNE YAPIŞTIRIN
        private void LoadImage(ref Bitmap targetBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // 1. Resmi diskten geçici olarak yükle
                using (Bitmap sourceFile = new Bitmap(ofd.FileName))
                {
                    // 2. FORMAT DÖNÜŞÜMÜ (KRİTİK ADIM)
                    // Yüklenen resim gri tonlamalı (8bpp) veya indekslenmiş olabilir.
                    // Bizim algoritmamız KESİNLİKLE Format24bppRgb (3 byte) istiyor.
                    // Bu yüzden, önce resmi güvenli bir şekilde bu formata çeviriyoruz.

                    Bitmap safeFormatBmp;

                    // Eğer format zaten 24bppRgb değilse dönüştür
                    if (sourceFile.PixelFormat != PixelFormat.Format24bppRgb)
                    {
                        // İstediğimiz formatta boş bir resim oluştur
                        safeFormatBmp = new Bitmap(sourceFile.Width, sourceFile.Height, PixelFormat.Format24bppRgb);

                        // Orijinal resmi bu yeni formatın üzerine çiz (GDI+ dönüşümü otomatik yapar)
                        using (Graphics g = Graphics.FromImage(safeFormatBmp))
                        {
                            // Şeffaflık sorunlarını önlemek için arka planı siyaha boya
                            g.Clear(Color.Black);
                            // Resmi çiz
                            g.DrawImage(sourceFile, 0, 0, sourceFile.Width, sourceFile.Height);
                        }
                    }
                    else
                    {
                        // Zaten format doğruysa klonunu al
                        safeFormatBmp = (Bitmap)sourceFile.Clone();
                    }

                    // 3. ÖLÇEKLEME (Picturebox'a sığdırma)
                    // Artık safeFormatBmp değişkeni kesinlikle 24bppRgb formatında.
                    // Şimdi bunu PictureBox boyutlarına göre ölçekleyebiliriz.
                    using (safeFormatBmp)
                    {
                        float scaleW = (float)pcb.Width / safeFormatBmp.Width;
                        float scaleH = (float)pcb.Height / safeFormatBmp.Height;
                        float scale = Math.Min(scaleW, scaleH);

                        if (scale > 1) scale = 1;

                        int newW = (int)(safeFormatBmp.Width * scale);
                        int newH = (int)(safeFormatBmp.Height * scale);

                        // Eski bitmap varsa temizle
                        if (targetBmp != null) targetBmp.Dispose();

                        // Hedef bitmap'i oluştur (Yine 24bppRgb olarak)
                        targetBmp = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);

                        // Ölçeklenmiş resmi çiz
                        using (Graphics g = Graphics.FromImage(targetBmp))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(safeFormatBmp, 0, 0, newW, newH);
                        }
                    }
                }

                // 4. Sonucu PictureBox'a ata
                pcb.SizeMode = PictureBoxSizeMode.CenterImage;
                pcb.Image = targetBmp;
            }
        }

        private void BtnProcess_Click(object sender, EventArgs e)
        {
            if (bmpBackground == null || bmpObject == null)
            {
                MessageBox.Show("Lütfen hem arka plan hem de nesne resimlerini yükleyin.");
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            // Boyut uyuşmazlığı varsa nesne resmini arka plana uydur (Resize)
            if (bmpObject.Width != bmpBackground.Width || bmpObject.Height != bmpBackground.Height)
            {
                Bitmap resizedObj = new Bitmap(bmpBackground.Width, bmpBackground.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(resizedObj)) g.DrawImage(bmpObject, 0, 0, resizedObj.Width, resizedObj.Height);
                bmpObject = resizedObj; // Referansı güncelle
                pcbObject.Image = bmpObject;
            }

            // İşlemi Başlat
            Bitmap result = SubtractAndThreshold(bmpBackground, bmpObject, tbThreshold.Value);

            pcbResult.Image = result;
            this.Cursor = Cursors.Default;
        }

        // --- ARKA PLAN ÇIKARMA VE EŞİKLEME ALGORİTMASI (Hızlı - Marshal) ---
        // --- GÜVENLİ VE HATASIZ VERSİYON ---
        private Bitmap SubtractAndThreshold(Bitmap bg, Bitmap obj, int threshold)
        {
            int w = bg.Width;
            int h = bg.Height;
            Bitmap result = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            // 1. Verileri Kilitle (LockBits)
            BitmapData bgData = bg.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData objData = obj.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData resData = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            // Stride: Bir satırın bellekte kapladığı gerçek genişlik (Padding dahil)
            int stride = bgData.Stride;
            int bytes = stride * h; // Toplam bayt sayısı

            byte[] bgBuffer = new byte[bytes];
            byte[] objBuffer = new byte[bytes];
            byte[] resBuffer = new byte[bytes];

            // 2. Hafızadan güvenli kopyalama
            Marshal.Copy(bgData.Scan0, bgBuffer, 0, bytes);
            Marshal.Copy(objData.Scan0, objBuffer, 0, bytes);

            // 3. Piksel İşlemleri (Satır Satır Gezme - En Güvenli Yöntem)
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Her pikselin dizideki tam yerini hesapla
                    // y * stride -> Hangi satırdayız
                    // x * 3      -> Satırda kaçıncı pikseldeyiz (3 bayt olduğu için)
                    int idx = (y * stride) + (x * 3);

                    // Güvenlik Kontrolü: Eğer hesaplanan index dizi boyutunu aşarsa atla
                    if (idx + 2 >= bytes) continue;

                    // Mavi (B)
                    int bgB = bgBuffer[idx];
                    int objB = objBuffer[idx];

                    // Yeşil (G)
                    int bgG = bgBuffer[idx + 1];
                    int objG = objBuffer[idx + 1];

                    // Kırmızı (R)
                    int bgR = bgBuffer[idx + 2];
                    int objR = objBuffer[idx + 2];

                    // Gri Tonlama (Basit Ortalama)
                    int bgGray = (bgB + bgG + bgR) / 3;
                    int objGray = (objB + objG + objR) / 3;

                    // Mutlak Fark
                    int diff = Math.Abs(objGray - bgGray);

                    // Eşikleme
                    byte val = (diff > threshold) ? (byte)255 : (byte)0;

                    // Sonucu Yaz
                    resBuffer[idx] = val;     // Blue
                    resBuffer[idx + 1] = val; // Green
                    resBuffer[idx + 2] = val; // Red
                }
            }

            // 4. Sonucu Geri Kopyala ve Kilitleri Aç
            Marshal.Copy(resBuffer, 0, resData.Scan0, bytes);
            bg.UnlockBits(bgData);
            obj.UnlockBits(objData);
            result.UnlockBits(resData);

            return result;
        }
    }
}