using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje7_EksenTakibiForm : Form
    {
        // UI Elemanları
        private PictureBox pcbReferans, pcbHedef, pcbSistem;
        private Label lblRef, lblHedef, lblSistem, lblVeriler;
        private Button btnYukleRef, btnYukleHedef, btnKilitle, btnGeri;
        private NumericUpDown nudEsik;
        private CheckBox chkGostergeler;

        // Görüntü Değişkenleri
        private Bitmap bmpReferans; // Boş oda
        private Bitmap bmpHedef;    // Odaya giren kişi
        private Form haftaFormu;

        // Kamera Simülasyon Değişkenleri
        // Varsayalım ki kameramızın görüş açısı (FOV) 60 derece.
        private const double CAMERA_FOV = 60.0;

        public Proje7_EksenTakibiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 7: Otomatik Hedef Takibi ve Eksen Kontrolü";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void InitializeComponent() { this.Name = "Proje7_EksenTakibiForm"; }

        private void SetupUI()
        {
            int margin = 20;
            int boxSize = 300;

            // 1. Referans Görüntü (Arka Plan)
            lblRef = new Label() { Text = "1. Referans (Boş Ortam)", Location = new Point(margin, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbReferans = CreateBox(margin, 45, boxSize, boxSize);
            btnYukleRef = new Button() { Text = "Referans Yükle", Location = new Point(margin, 45 + boxSize + 5), Size = new Size(boxSize, 35) };
            btnYukleRef.Click += (s, e) => LoadImageSafe(ref bmpReferans, pcbReferans);

            // 2. Hedef Görüntü (Hareketli)
            int x2 = margin + boxSize + 20;
            lblHedef = new Label() { Text = "2. Anlık Görüntü (Hedef)", Location = new Point(x2, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbHedef = CreateBox(x2, 45, boxSize, boxSize);
            btnYukleHedef = new Button() { Text = "Görüntü Yükle", Location = new Point(x2, 45 + boxSize + 5), Size = new Size(boxSize, 35) };
            btnYukleHedef.Click += (s, e) => LoadImageSafe(ref bmpHedef, pcbHedef);

            // 3. Sistem Ekranı (HUD - Head Up Display)
            int x3 = x2 + boxSize + 20;
            lblSistem = new Label() { Text = "3. Atış Kontrol Sistemi (HUD)", Location = new Point(x3, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbSistem = CreateBox(x3, 45, boxSize, boxSize);
            pcbSistem.BackColor = Color.Black; // Radar ekranı gibi siyah olsun

            // Kontroller
            int ctrlY = 45 + boxSize + 50;

            Label lblEsik = new Label() { Text = "Hassasiyet:", Location = new Point(margin, ctrlY + 5), AutoSize = true };
            nudEsik = new NumericUpDown() { Location = new Point(margin + 80, ctrlY), Value = 40, Maximum = 255, Width = 60 };

            chkGostergeler = new CheckBox() { Text = "Vektörleri Göster", Location = new Point(margin + 160, ctrlY), Checked = true, AutoSize = true };

            btnKilitle = new Button() { Text = "HEDEFİ KİLİTLE VE HESAPLA", Location = new Point(x2, ctrlY - 10), Size = new Size(boxSize, 50), BackColor = Color.DarkRed, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnKilitle.Click += BtnKilitle_Click;

            // Veri Paneli (Simüle edilmiş motor verileri)
            lblVeriler = new Label()
            {
                Text = "SİSTEM BEKLEMEDE...\n------------------\nYatay Sapma (Pan)  : 0 px  [0°]\nDikey Sapma (Tilt) : 0 px  [0°]\nMotor Durumu       : DURUYOR",
                Location = new Point(x3, ctrlY - 10),
                Size = new Size(boxSize, 100),
                Font = new Font("Consolas", 10),
                ForeColor = Color.Lime, // Hacker/Matrix yeşili
                BackColor = Color.Black,
                BorderStyle = BorderStyle.Fixed3D,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnGeri = new Button() { Text = "Çıkış", Location = new Point(this.Width - 100, this.Height - 70), Size = new Size(80, 30), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblRef, pcbReferans, btnYukleRef,
                lblHedef, pcbHedef, btnYukleHedef,
                lblSistem, pcbSistem,
                lblEsik, nudEsik, chkGostergeler, btnKilitle, lblVeriler, btnGeri
            });
        }

        private PictureBox CreateBox(int x, int y, int w, int h)
        {
            return new PictureBox() { Location = new Point(x, y), Size = new Size(w, h), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(30, 30, 30) };
        }

        // --- GÜVENLİ RESİM YÜKLEME ---
        private void LoadImageSafe(ref Bitmap targetBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap src = new Bitmap(ofd.FileName))
                {
                    Bitmap safe = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(safe)) g.DrawImage(src, 0, 0);

                    if (targetBmp != null) targetBmp.Dispose();
                    targetBmp = (Bitmap)safe.Clone();
                }
                pcb.Image = targetBmp;
            }
        }

        // --- ANA ALGORİTMA ---
        private void BtnKilitle_Click(object sender, EventArgs e)
        {
            if (bmpReferans == null || bmpHedef == null) { MessageBox.Show("Lütfen referans ve hedef görüntüleri yükleyin."); return; }

            // Boyut Eşitleme (Kritik)
            if (bmpHedef.Width != bmpReferans.Width || bmpHedef.Height != bmpReferans.Height)
            {
                Bitmap resized = new Bitmap(bmpReferans.Width, bmpReferans.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmpHedef, 0, 0, bmpReferans.Width, bmpReferans.Height);
                }
                bmpHedef.Dispose();
                bmpHedef = resized;
                pcbHedef.Image = bmpHedef;
            }

            int w = bmpReferans.Width;
            int h = bmpReferans.Height;
            int threshold = (int)nudEsik.Value;

            // Sonuç görüntüsü (Siyah ekran üzerine HUD çizimi)
            Bitmap hudScreen = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            // --- GÖRÜNTÜ İŞLEME (LockBits) ---
            BitmapData dRef = bmpReferans.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dHedef = bmpHedef.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dHud = hudScreen.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = dRef.Stride * h;
            byte[] bufRef = new byte[bytes];
            byte[] bufHedef = new byte[bytes];
            byte[] bufHud = new byte[bytes]; // Siyah başlat

            Marshal.Copy(dRef.Scan0, bufRef, 0, bytes);
            Marshal.Copy(dHedef.Scan0, bufHedef, 0, bytes);

            long totalX = 0, totalY = 0, pixelCount = 0;
            int stride = dRef.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * stride + x * 3;
                    if (idx + 2 >= bytes) continue;

                    // Piksel Farkı (Hareket Tespiti)
                    int diff = Math.Abs(bufRef[idx] - bufHedef[idx]) +
                               Math.Abs(bufRef[idx + 1] - bufHedef[idx + 1]) +
                               Math.Abs(bufRef[idx + 2] - bufHedef[idx + 2]);

                    if (diff > threshold * 3)
                    {
                        // Hareketi Beyaz Yap (Termal kamera efekti gibi)
                        bufHud[idx] = 255; bufHud[idx + 1] = 255; bufHud[idx + 2] = 255;
                        totalX += x;
                        totalY += y;
                        pixelCount++;
                    }
                }
            }

            Marshal.Copy(bufHud, 0, dHud.Scan0, bytes);
            bmpReferans.UnlockBits(dRef);
            bmpHedef.UnlockBits(dHedef);
            hudScreen.UnlockBits(dHud);

            // --- HUD ÇİZİMİ VE HESAPLAMALAR ---
            using (Graphics g = Graphics.FromImage(hudScreen))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 1. Silah/Kamera Ekseni (Sabit Merkez)
                int centerX = w / 2;
                int centerY = h / 2;
                Pen axisPen = new Pen(Color.Cyan, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                g.DrawLine(axisPen, centerX, 0, centerX, h);
                g.DrawLine(axisPen, 0, centerY, w, centerY);
                // Nişangah Dairesi
                g.DrawEllipse(new Pen(Color.Cyan, 2), centerX - 20, centerY - 20, 40, 40);

                if (pixelCount > 50) // Gürültü filtresi
                {
                    // 2. Hedef Merkezi (Ağırlık Merkezi)
                    int targetX = (int)(totalX / pixelCount);
                    int targetY = (int)(totalY / pixelCount);

                    // Hedef Kutusu
                    Pen targetPen = new Pen(Color.Red, 3);
                    g.DrawRectangle(targetPen, targetX - 25, targetY - 25, 50, 50);
                    // Kilitlenme Çizgileri
                    g.DrawLine(targetPen, targetX - 10, targetY, targetX + 10, targetY);
                    g.DrawLine(targetPen, targetX, targetY - 10, targetX, targetY + 10);

                    // 3. Vektör Çizimi (Merkezden Hedefe)
                    if (chkGostergeler.Checked)
                    {
                        Pen vectorPen = new Pen(Color.Yellow, 2);
                        // Ekrana ok çizer
                        System.Drawing.Drawing2D.AdjustableArrowCap arrow = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                        vectorPen.CustomEndCap = arrow;
                        g.DrawLine(vectorPen, centerX, centerY, targetX, targetY);
                    }

                    // --- MATEMATİKSEL HESAPLAMALAR (ÖDEVİN ASIL KISMI) ---

                    // a. Piksel Sapması
                    int devX = targetX - centerX; // (+) Sağ, (-) Sol
                    int devY = targetY - centerY; // (+) Aşağı, (-) Yukarı

                    // b. Açısal Sapma (Derece cinsinden simülasyon)
                    // Formül: (Sapma / YarıGenişlik) * (GörüşAçısı / 2)
                    double angleX = ((double)devX / (w / 2.0)) * (CAMERA_FOV / 2.0);
                    double angleY = ((double)devY / (h / 2.0)) * (CAMERA_FOV / 2.0);

                    // Verileri Ekrana Yaz
                    lblVeriler.Text = $"HEDEF KİLİTLENDİ!\n------------------\n" +
                                      $"Yatay Sapma (Pan)  : {devX} px  [{angleX:F1}°]\n" +
                                      $"Dikey Sapma (Tilt) : {devY} px  [{angleY:F1}°]\n" +
                                      $"Motor Komutu       : {(devX > 0 ? "SAĞA" : "SOLA")} {Math.Abs(angleX):F1}° DÖN";

                    lblVeriler.ForeColor = Color.Red; // Alarm durumu
                }
                else
                {
                    lblVeriler.Text = "HEDEF BULUNAMADI\n------------------\nSistem Taramada...";
                    lblVeriler.ForeColor = Color.Lime;
                }
            }

            pcbSistem.Image = hudScreen;
        }
    }
}