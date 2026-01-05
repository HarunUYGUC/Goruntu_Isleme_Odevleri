using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging; // Hızlı Erişim (LockBits)
using System.Runtime.InteropServices; // Marshal
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_MorfolojiForm : Form
    {
        // UI Elemanları
        private PictureBox pcbOriginal, pcbSonuc;
        private Label lblBaslik, lblRenkAyari;
        private Button btnYukle, btnGurultuEkle, btnUygula, btnGeri;
        private NumericUpDown numRMin, numRMax, numGMin, numGMax, numBMin, numBMax;

        // Seçim Grupları
        private GroupBox grpSablon, grpIslem;
        private RadioButton rbKare9, rbArti5;
        private RadioButton rbGurultuGider, rbAciklikKapat, rbAciklikAc; // Median, Dilation, Erosion
        private CheckBox chkRenkFiltresi;

        private Bitmap originalBitmap;
        private Form haftaFormu;

        public Proje3_MorfolojiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Morfolojik İşlemler ve Gürültü Giderme";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();
        }

        private void InitializeComponent() { this.Name = "Proje3_MorfolojiForm"; this.FormClosed += (s, e) => haftaFormu.Show(); }

        private void SetupUI()
        {
            int margin = 20;
            int pcbSize = 350;

            // Sol Taraf (Resimler)
            lblBaslik = new Label() { Text = "Orijinal Görüntü vs İşlenmiş Görüntü", Location = new Point(margin, 15), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold) };
            pcbOriginal = CreatePictureBox(margin, 50, pcbSize);
            pcbSonuc = CreatePictureBox(margin + pcbSize + 20, 50, pcbSize);

            // Sağ Taraf (Kontroller)
            int ctrlX = margin * 2 + pcbSize * 2 + 30;
            int ctrlY = 50;
            int grpWidth = 280;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(ctrlX, ctrlY), Size = new Size(130, 40) };
            btnYukle.Click += BtnYukle_Click;

            btnGurultuEkle = new Button() { Text = "Tuz-Biber Ekle", Location = new Point(ctrlX + 140, ctrlY), Size = new Size(140, 40), BackColor = Color.LightGray };
            btnGurultuEkle.Click += BtnGurultuEkle_Click;

            ctrlY += 60;

            // 1. Şablon Seçimi
            grpSablon = new GroupBox() { Text = "1. Şablon Seçimi (Kernel)", Location = new Point(ctrlX, ctrlY), Size = new Size(grpWidth, 80), Font = new Font("Arial", 9, FontStyle.Bold) };
            rbKare9 = new RadioButton() { Text = "9'luk Kare (3x3 Tam)", Location = new Point(15, 25), Checked = true, AutoSize = true };
            rbArti5 = new RadioButton() { Text = "5'lik Artı (+ Şekli)", Location = new Point(15, 50), AutoSize = true };
            grpSablon.Controls.AddRange(new Control[] { rbKare9, rbArti5 });

            ctrlY += 90;

            // 2. İşlem Seçimi (Soruya Göre Düzenlendi)
            grpIslem = new GroupBox() { Text = "2. Yapılacak İşlem", Location = new Point(ctrlX, ctrlY), Size = new Size(grpWidth, 110), Font = new Font("Arial", 9, FontStyle.Bold) };
            rbGurultuGider = new RadioButton() { Text = "Gürültü Yok Et (Median)", Location = new Point(15, 25), Checked = true, AutoSize = true };
            rbAciklikKapat = new RadioButton() { Text = "Açıklıkları Kapat (Genişlet/Dilation)", Location = new Point(15, 50), AutoSize = true };
            rbAciklikAc = new RadioButton() { Text = "Gürültü Azalt (Daralt/Erosion)", Location = new Point(15, 75), AutoSize = true }; // Ekstra özellik
            grpIslem.Controls.AddRange(new Control[] { rbGurultuGider, rbAciklikKapat, rbAciklikAc });

            ctrlY += 120;

            // 3. Renk Aralığı (Soru b maddesi)
            chkRenkFiltresi = new CheckBox() { Text = "Renk Aralığı Filtresini Aktif Et", Location = new Point(ctrlX, ctrlY), AutoSize = true, ForeColor = Color.Red };

            ctrlY += 30;
            lblRenkAyari = new Label() { Text = "R, G, B Renk Aralıkları:", Location = new Point(ctrlX, ctrlY), AutoSize = true };

            ctrlY += 25;
            CreateRangeControl("Kırmızı (R):", ref numRMin, ref numRMax, ctrlX, ctrlY);
            ctrlY += 30;
            CreateRangeControl("Yeşil (G):", ref numGMin, ref numGMax, ctrlX, ctrlY);
            ctrlY += 30;
            CreateRangeControl("Mavi (B):", ref numBMin, ref numBMax, ctrlX, ctrlY);

            ctrlY += 40;
            btnUygula = new Button() { Text = "İŞLEMİ UYGULA", Location = new Point(ctrlX, ctrlY), Size = new Size(grpWidth, 50), BackColor = Color.ForestGreen, ForeColor = Color.White, Font = new Font("Arial", 11, FontStyle.Bold) };
            btnUygula.Click += BtnUygula_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX, ctrlY + 60), Size = new Size(grpWidth, 40), BackColor = Color.IndianRed, ForeColor = Color.White };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblBaslik, pcbOriginal, pcbSonuc,
                btnYukle, btnGurultuEkle, grpSablon, grpIslem,
                chkRenkFiltresi, lblRenkAyari, btnUygula, btnGeri,
                numRMin, numRMax, numGMin, numGMax, numBMin, numBMax
            });
        }

        private PictureBox CreatePictureBox(int x, int y, int size) => new PictureBox() { Location = new Point(x, y), Size = new Size(size, size), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };

        private void CreateRangeControl(string text, ref NumericUpDown min, ref NumericUpDown max, int x, int y)
        {
            Label l = new Label() { Text = text, Location = new Point(x, y + 2), Size = new Size(70, 20), Font = new Font("Arial", 8) };
            min = new NumericUpDown() { Minimum = 0, Maximum = 255, Value = 0, Location = new Point(x + 80, y), Size = new Size(50, 20) };
            Label l2 = new Label() { Text = "-", Location = new Point(x + 135, y), Size = new Size(10, 20) };
            max = new NumericUpDown() { Minimum = 0, Maximum = 255, Value = 255, Location = new Point(x + 150, y), Size = new Size(50, 20) };
            this.Controls.Add(l); this.Controls.Add(l2);
        }

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap temp = new Bitmap(ofd.FileName))
                {
                    int boxW = pcbOriginal.Width;
                    int boxH = pcbOriginal.Height;

                    float scale = Math.Min((float)boxW / temp.Width, (float)boxH / temp.Height);

                    if (scale > 1) scale = 1;

                    int newW = (int)(temp.Width * scale);
                    int newH = (int)(temp.Height * scale);

                    originalBitmap = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);

                    using (Graphics g = Graphics.FromImage(originalBitmap))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(temp, 0, 0, newW, newH);
                    }
                }


                pcbOriginal.SizeMode = PictureBoxSizeMode.CenterImage;
                pcbOriginal.Image = originalBitmap;

                pcbSonuc.Image = null;
                pcbSonuc.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void BtnGurultuEkle_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            // Orijinali bozmamak için kopyasını al
            Bitmap noisy = new Bitmap(originalBitmap);
            Random rnd = new Random();
            int amount = (noisy.Width * noisy.Height) / 20; // %5 Gürültü

            // LockBits ile hızlı gürültü ekleme
            BitmapData data = noisy.LockBits(new Rectangle(0, 0, noisy.Width, noisy.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bytes = data.Stride * noisy.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(data.Scan0, buffer, 0, bytes);

            for (int i = 0; i < amount; i++)
            {
                int x = rnd.Next(noisy.Width);
                int y = rnd.Next(noisy.Height);
                int idx = y * data.Stride + x * 3;

                byte val = rnd.Next(2) == 0 ? (byte)0 : (byte)255; // Siyah veya Beyaz
                buffer[idx] = val; buffer[idx + 1] = val; buffer[idx + 2] = val;
            }

            Marshal.Copy(buffer, 0, data.Scan0, bytes);
            noisy.UnlockBits(data);
            originalBitmap = noisy; // Gürültülü hali yeni orijinal yap
            pcbOriginal.Image = originalBitmap;
        }

        private void BtnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            bool isSquare = rbKare9.Checked;
            bool useColorRange = chkRenkFiltresi.Checked;

            Bitmap result = null;

            if (rbGurultuGider.Checked) // Median Filtresi (Gürültü Yok Etme)
            {
                result = ApplyMedianFilter(originalBitmap, isSquare, useColorRange);
            }
            else if (rbAciklikKapat.Checked) // Dilation (Genişletme - Açıklık Kapatma)
            {
                result = ApplyMorphology(originalBitmap, true, isSquare, useColorRange);
            }
            else // Erosion (Daraltma)
            {
                result = ApplyMorphology(originalBitmap, false, isSquare, useColorRange);
            }

            pcbSonuc.Image = result;
            this.Cursor = Cursors.Default;
        }

        // --- GÜRÜLTÜ YOK ETME (MEDIAN FILTRESI) ---
        private Bitmap ApplyMedianFilter(Bitmap src, bool isSquare, bool useColorFilter)
        {
            int w = src.Width; int h = src.Height;
            Bitmap dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            byte[] srcBuf = new byte[stride * h];
            byte[] dstBuf = new byte[stride * h];
            Marshal.Copy(srcData.Scan0, srcBuf, 0, srcBuf.Length);

            // Renk Aralıkları
            int rMin = (int)numRMin.Value; int rMax = (int)numRMax.Value;
            int gMin = (int)numGMin.Value; int gMax = (int)numGMax.Value;
            int bMin = (int)numBMin.Value; int bMax = (int)numBMax.Value;

            // 1 piksel içeriden başla (Sınır kontrolü yapmamak için hızlandırma)
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int centerIdx = y * stride + x * 3;
                    byte b = srcBuf[centerIdx];
                    byte g = srcBuf[centerIdx + 1];
                    byte r = srcBuf[centerIdx + 2];

                    // Renk Filtresi Kontrolü
                    if (useColorFilter)
                    {
                        if (!(r >= rMin && r <= rMax && g >= gMin && g <= gMax && b >= bMin && b <= bMax))
                        {
                            // Aralığa uymuyorsa piksere dokunma, olduğu gibi kopyala
                            dstBuf[centerIdx] = b; dstBuf[centerIdx + 1] = g; dstBuf[centerIdx + 2] = r;
                            continue;
                        }
                    }

                    List<byte> listB = new List<byte>();
                    List<byte> listG = new List<byte>();
                    List<byte> listR = new List<byte>();

                    // Kernel Döngüsü (3x3)
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            // 5'lik Artı Şablon Kontrolü: Köşeleri (Çaprazları) atla
                            if (!isSquare && Math.Abs(ky) == 1 && Math.Abs(kx) == 1) continue;

                            int idx = (y + ky) * stride + (x + kx) * 3;
                            listB.Add(srcBuf[idx]);
                            listG.Add(srcBuf[idx + 1]);
                            listR.Add(srcBuf[idx + 2]);
                        }
                    }

                    listB.Sort(); listG.Sort(); listR.Sort();
                    int mid = listB.Count / 2;

                    dstBuf[centerIdx] = listB[mid];
                    dstBuf[centerIdx + 1] = listG[mid];
                    dstBuf[centerIdx + 2] = listR[mid];
                }
            }

            Marshal.Copy(dstBuf, 0, dstData.Scan0, dstBuf.Length);
            src.UnlockBits(srcData); dst.UnlockBits(dstData);
            return dst;
        }

        // --- AÇIKLIK KAPATMA (MORFOLOJİ: DILATION / EROSION) ---
        private Bitmap ApplyMorphology(Bitmap src, bool isDilation, bool isSquare, bool useColorFilter)
        {
            int w = src.Width; int h = src.Height;
            Bitmap dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            byte[] srcBuf = new byte[stride * h];
            byte[] dstBuf = new byte[stride * h];
            Marshal.Copy(srcData.Scan0, srcBuf, 0, srcBuf.Length);

            int rMin = (int)numRMin.Value; int rMax = (int)numRMax.Value;
            int gMin = (int)numGMin.Value; int gMax = (int)numGMax.Value;
            int bMin = (int)numBMin.Value; int bMax = (int)numBMax.Value;

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int centerIdx = y * stride + x * 3;
                    byte bVal = srcBuf[centerIdx];
                    byte gVal = srcBuf[centerIdx + 1];
                    byte rVal = srcBuf[centerIdx + 2];

                    if (useColorFilter)
                    {
                        if (!(rVal >= rMin && rVal <= rMax && gVal >= gMin && gVal <= gMax && bVal >= bMin && bVal <= bMax))
                        {
                            dstBuf[centerIdx] = bVal; dstBuf[centerIdx + 1] = gVal; dstBuf[centerIdx + 2] = rVal;
                            continue;
                        }
                    }

                    // Dilation (Genişletme): Çevredeki EN BÜYÜK (En parlak) değeri al -> Açıklıkları (Siyahları) kapatır.
                    // Erosion (Daraltma): Çevredeki EN KÜÇÜK (En koyu) değeri al -> Beyaz gürültüyü yok eder.
                    byte bestB = isDilation ? (byte)0 : (byte)255;
                    byte bestG = isDilation ? (byte)0 : (byte)255;
                    byte bestR = isDilation ? (byte)0 : (byte)255;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            if (!isSquare && Math.Abs(ky) == 1 && Math.Abs(kx) == 1) continue; // Artı şablonu

                            int idx = (y + ky) * stride + (x + kx) * 3;
                            byte nb = srcBuf[idx];
                            byte ng = srcBuf[idx + 1];
                            byte nr = srcBuf[idx + 2];

                            if (isDilation) // MAX bul
                            {
                                if (nb > bestB) bestB = nb;
                                if (ng > bestG) bestG = ng;
                                if (nr > bestR) bestR = nr;
                            }
                            else // MIN bul
                            {
                                if (nb < bestB) bestB = nb;
                                if (ng < bestG) bestG = ng;
                                if (nr < bestR) bestR = nr;
                            }
                        }
                    }

                    dstBuf[centerIdx] = bestB;
                    dstBuf[centerIdx + 1] = bestG;
                    dstBuf[centerIdx + 2] = bestR;
                }
            }

            Marshal.Copy(dstBuf, 0, dstData.Scan0, dstBuf.Length);
            src.UnlockBits(srcData); dst.UnlockBits(dstData);
            return dst;
        }
    }
}