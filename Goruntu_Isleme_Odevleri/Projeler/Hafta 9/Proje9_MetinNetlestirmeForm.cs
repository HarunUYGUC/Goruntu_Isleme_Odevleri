using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje9_MetinNetlestirmeForm : Form
    {
        // UI Elemanları
        private PictureBox pcbMetin, pcbZemin, pcbSonuc;
        private Label lblMetin, lblZemin, lblSonuc, lblBilgi;
        private Button btnMetinYukle, btnZeminYukle, btnIsle, btnGeri;
        private TrackBar tbEsik;
        private Label lblEsik;
        private CheckBox chkSadeceDuzelt;

        private Bitmap bmpMetin;
        private Bitmap bmpZemin;
        private Form haftaFormu;

        public Proje9_MetinNetlestirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 9: Gölge Giderme ve Metin Netleştirme";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void InitializeComponent() { this.Name = "Proje9_MetinNetlestirmeForm"; }

        private void SetupUI()
        {
            int margin = 20;
            int boxSize = 300;

            // 1. Metinli Görüntü
            lblMetin = new Label() { Text = "1. Metinli Fotoğraf (Gölgeli)", Location = new Point(margin, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbMetin = CreateBox(margin, 45, boxSize, boxSize);
            btnMetinYukle = new Button() { Text = "Metin Fotosu Yükle", Location = new Point(margin, 45 + boxSize + 5), Size = new Size(boxSize, 35) };
            btnMetinYukle.Click += (s, e) => LoadImageSafe(ref bmpMetin, pcbMetin);

            // 2. Zemin Görüntüsü (Boş Kağıt)
            int x2 = margin + boxSize + 20;
            lblZemin = new Label() { Text = "2. Zemin Fotosu (Aynı Işıkta Boş)", Location = new Point(x2, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbZemin = CreateBox(x2, 45, boxSize, boxSize);
            btnZeminYukle = new Button() { Text = "Zemin Fotosu Yükle", Location = new Point(x2, 45 + boxSize + 5), Size = new Size(boxSize, 35) };
            btnZeminYukle.Click += (s, e) => LoadImageSafe(ref bmpZemin, pcbZemin);

            // 3. Sonuç
            int x3 = x2 + boxSize + 20;
            lblSonuc = new Label() { Text = "3. Netleştirilmiş Sonuç", Location = new Point(x3, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbSonuc = CreateBox(x3, 45, boxSize, boxSize);
            // Sonuç beyaz zemin olacağı için arka planı gri yapalım fark edilsin
            pcbSonuc.BackColor = Color.Gray;

            // Ayarlar
            int ctrlY = 45 + boxSize + 50;

            lblEsik = new Label() { Text = "Siyah/Beyaz Eşiği: 180", Location = new Point(margin, ctrlY), AutoSize = true };
            tbEsik = new TrackBar() { Location = new Point(margin + 150, ctrlY - 5), Size = new Size(200, 45), Maximum = 255, Value = 180, TickStyle = TickStyle.None };
            tbEsik.Scroll += (s, e) => lblEsik.Text = $"Siyah/Beyaz Eşiği: {tbEsik.Value}";

            chkSadeceDuzelt = new CheckBox() { Text = "Eşikleme Yapma (Sadece Aydınlatmayı Düzelt)", Location = new Point(x2, ctrlY), AutoSize = true };

            btnIsle = new Button() { Text = "GÖLGELERİ YOK ET VE NETLEŞTİR", Location = new Point(x3, ctrlY - 10), Size = new Size(boxSize, 50), BackColor = Color.SteelBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnIsle.Click += BtnIsle_Click;

            lblBilgi = new Label()
            {
                Text = "NASIL ÇEKİLİR?\n1. Karanlık bir odaya geçin, telefon flaşını açın.\n2. Yazılı kağıdın fotoğrafını çekin.\n3. Kağıdı kaldırıp sadece zeminin (masanın/boş kağıdın) fotoğrafını çekin.\nAlgoritma ışığı analiz edip gölgeleri silecektir.",
                Location = new Point(margin, ctrlY + 40),
                Size = new Size(x2, 80),
                ForeColor = Color.DimGray,
                Font = new Font("Consolas", 9)
            };

            btnGeri = new Button() { Text = "Çıkış", Location = new Point(this.Width - 100, this.Height - 80), Size = new Size(80, 30), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblMetin, pcbMetin, btnMetinYukle,
                lblZemin, pcbZemin, btnZeminYukle,
                lblSonuc, pcbSonuc,
                lblEsik, tbEsik, chkSadeceDuzelt, btnIsle, lblBilgi, btnGeri
            });
        }

        private PictureBox CreateBox(int x, int y, int w, int h)
        {
            return new PictureBox() { Location = new Point(x, y), Size = new Size(w, h), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
        }

        // BU FONKSİYONU ESKİSİYLE DEĞİŞTİRİN
        private void LoadImageSafe(ref Bitmap targetBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Dosyadan oku (using bloğu ile dosyayı kilitlemeden açıp kapatırız)
                using (Bitmap src = new Bitmap(ofd.FileName))
                {
                    // 1. ÖLÇEKLEME HESABI
                    // Resim çok büyükse işlem uzun sürer ve ekrana sığmaz.
                    // Bu yüzden en uzun kenarı maksimum 800 piksel olacak şekilde küçültelim.
                    int maxSize = 800;
                    float scale = 1.0f;

                    if (src.Width > maxSize || src.Height > maxSize)
                    {
                        float scaleW = (float)maxSize / src.Width;
                        float scaleH = (float)maxSize / src.Height;

                        // En-Boy oranını bozmamak için küçük olan oranı seçiyoruz
                        scale = Math.Min(scaleW, scaleH);
                    }

                    int newW = (int)(src.Width * scale);
                    int newH = (int)(src.Height * scale);

                    // 2. YENİ RESMİ OLUŞTUR (FORMAT DÜZELTME DAHİL)
                    // Hem boyutu küçültüyoruz hem de formatı "24bppRgb" yapıyoruz (Hızlı işlem için şart)
                    Bitmap resizedBmp = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);

                    using (Graphics g = Graphics.FromImage(resizedBmp))
                    {
                        // Kaliteli küçültme ayarları (Pikselleşmeyi önler)
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        // Resmi yeni boyutlarda çiz
                        g.DrawImage(src, 0, 0, newW, newH);
                    }

                    // 3. HEDEFE AKTAR
                    if (targetBmp != null) targetBmp.Dispose(); // Eskisini bellekten sil
                    targetBmp = (Bitmap)resizedBmp.Clone();

                    // Geçici resmi temizle
                    resizedBmp.Dispose();
                }

                // 4. EKRANDA GÖSTER
                pcb.SizeMode = PictureBoxSizeMode.Zoom; // Kutunun içine orantılı sığdır
                pcb.Image = targetBmp;
            }
        }

        // --- GELİŞMİŞ ALGORİTMA: NORMALİZASYON (FLAT FIELD CORRECTION) ---
        private void BtnIsle_Click(object sender, EventArgs e)
        {
            if (bmpMetin == null || bmpZemin == null) { MessageBox.Show("Lütfen iki resmi de yükleyin."); return; }

            this.Cursor = Cursors.WaitCursor;

            // 1. Boyut Eşitleme (Zemin resmini metin resmine uydur)
            if (bmpZemin.Width != bmpMetin.Width || bmpZemin.Height != bmpMetin.Height)
            {
                Bitmap resizedZemin = new Bitmap(bmpMetin.Width, bmpMetin.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(resizedZemin))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmpZemin, 0, 0, bmpMetin.Width, bmpMetin.Height);
                }
                bmpZemin.Dispose();
                bmpZemin = resizedZemin;
                pcbZemin.Image = bmpZemin;
            }

            int w = bmpMetin.Width;
            int h = bmpMetin.Height;
            int threshold = tbEsik.Value;
            bool onlyCorrection = chkSadeceDuzelt.Checked;

            Bitmap result = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            // LockBits
            BitmapData dMetin = bmpMetin.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dZemin = bmpZemin.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dRes = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = dMetin.Stride * h;
            byte[] bufMetin = new byte[bytes];
            byte[] bufZemin = new byte[bytes];
            byte[] bufRes = new byte[bytes];

            Marshal.Copy(dMetin.Scan0, bufMetin, 0, bytes);
            Marshal.Copy(dZemin.Scan0, bufZemin, 0, bytes);

            int stride = dMetin.Stride;

            // --- PİKSEL DÖNGÜSÜ ---
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * stride + x * 3;
                    if (idx + 2 >= bytes) continue;

                    // Griye çevir (İşlem kolaylığı için Luminance formülü)
                    double grayMetin = bufMetin[idx] * 0.11 + bufMetin[idx + 1] * 0.59 + bufMetin[idx + 2] * 0.3;
                    double grayZemin = bufZemin[idx] * 0.11 + bufZemin[idx + 1] * 0.59 + bufZemin[idx + 2] * 0.3;

                    // Sıfıra bölme hatasını önle
                    if (grayZemin < 1) grayZemin = 1;

                    // --- FORMÜL: AYDINLATMA NORMALİZASYONU ---
                    // Teorik Formül: Result = (Original / Background) * Mean
                    // Biz burada Mean yerine sabit 255 (maksimum parlaklık) kullanabiliriz.
                    // Mantık: Metin pikseli Zemin pikseline oranlanır.
                    // Eğer zemin gölgeliyse (karanlıksa) ve metin de o gölgedeyse (karanlıksa),
                    // oran 1'e yakın çıkar -> Sonuç BEYAZ (Kağıt) olur.
                    // Eğer zemin parlaksa ve metin SİYAHSA, oran 0'a yakın çıkar -> Sonuç SİYAH (Yazı) olur.

                    double normalized = (grayMetin / grayZemin) * 255.0;

                    // Değeri Sınırla (Clamp)
                    if (normalized > 255) normalized = 255;
                    if (normalized < 0) normalized = 0;

                    byte finalVal;

                    if (onlyCorrection)
                    {
                        // Sadece aydınlatmayı düzelt, gri tonlamalı bırak
                        finalVal = (byte)normalized;
                    }
                    else
                    {
                        // Eşikleme (Thresholding) yap -> Net Siyah/Beyaz
                        finalVal = (normalized < threshold) ? (byte)0 : (byte)255;
                    }

                    bufRes[idx] = finalVal;     // B
                    bufRes[idx + 1] = finalVal; // G
                    bufRes[idx + 2] = finalVal; // R
                }
            }

            Marshal.Copy(bufRes, 0, dRes.Scan0, bytes);
            bmpMetin.UnlockBits(dMetin);
            bmpZemin.UnlockBits(dZemin);
            result.UnlockBits(dRes);

            pcbSonuc.Image = result;
            this.Cursor = Cursors.Default;
        }
    }
}