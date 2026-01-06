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
        private Label lblMetin, lblZemin, lblSonuc, lblEsik;
        private Button btnMetinYukle, btnZeminYukle, btnIsle, btnGeri;
        private TrackBar tbEsik;
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
            pcbSonuc.BackColor = Color.Gray;

            // Ayarlar
            int ctrlY = 45 + boxSize + 50;

            lblEsik = new Label() { Text = "Siyah/Beyaz Eşiği: 180", Location = new Point(margin, ctrlY), AutoSize = true };
            tbEsik = new TrackBar() { Location = new Point(margin + 150, ctrlY - 5), Size = new Size(200, 45), Maximum = 255, Value = 180, TickStyle = TickStyle.None };
            tbEsik.Scroll += (s, e) => lblEsik.Text = $"Siyah/Beyaz Eşiği: {tbEsik.Value}";

            chkSadeceDuzelt = new CheckBox() { Text = "Eşikleme Yapma (Sadece Aydınlatmayı Düzelt)", Location = new Point(x2, ctrlY), AutoSize = true };

            btnIsle = new Button() { Text = "GÖLGELERİ YOK ET VE NETLEŞTİR", Location = new Point(x3, ctrlY - 10), Size = new Size(boxSize, 50), BackColor = Color.SteelBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnIsle.Click += BtnIsle_Click;

            btnGeri = new Button() { Text = "Çıkış", Location = new Point(this.Width - 100, this.Height - 80), Size = new Size(80, 30), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblMetin, pcbMetin, btnMetinYukle,
                lblZemin, pcbZemin, btnZeminYukle,
                lblSonuc, pcbSonuc,
                lblEsik, tbEsik, chkSadeceDuzelt, btnIsle, btnGeri
            });
        }

        private PictureBox CreateBox(int x, int y, int w, int h)
        {
            return new PictureBox() { Location = new Point(x, y), Size = new Size(w, h), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
        }

        private void LoadImageSafe(ref Bitmap targetBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap src = new Bitmap(ofd.FileName))
                {
                    int maxSize = 800;
                    float scale = 1.0f;

                    if (src.Width > maxSize || src.Height > maxSize)
                    {
                        float scaleW = (float)maxSize / src.Width;
                        float scaleH = (float)maxSize / src.Height;
                        scale = Math.Min(scaleW, scaleH);
                    }

                    int newW = (int)(src.Width * scale);
                    int newH = (int)(src.Height * scale);

                    Bitmap resizedBmp = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);

                    using (Graphics g = Graphics.FromImage(resizedBmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.DrawImage(src, 0, 0, newW, newH);
                    }

                    if (targetBmp != null) targetBmp.Dispose();
                    targetBmp = (Bitmap)resizedBmp.Clone();
                    resizedBmp.Dispose();
                }

                pcb.SizeMode = PictureBoxSizeMode.Zoom;
                pcb.Image = targetBmp;
            }
        }

        private void BtnIsle_Click(object sender, EventArgs e)
        {
            if (bmpMetin == null || bmpZemin == null) { MessageBox.Show("Lütfen iki resmi de yükleyin."); return; }

            this.Cursor = Cursors.WaitCursor;

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

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * stride + x * 3;
                    if (idx + 2 >= bytes) continue;

                    double grayMetin = bufMetin[idx] * 0.11 + bufMetin[idx + 1] * 0.59 + bufMetin[idx + 2] * 0.3;
                    double grayZemin = bufZemin[idx] * 0.11 + bufZemin[idx + 1] * 0.59 + bufZemin[idx + 2] * 0.3;

                    if (grayZemin < 1) grayZemin = 1;

                    double normalized = (grayMetin / grayZemin) * 255.0;

                    if (normalized > 255) normalized = 255;
                    if (normalized < 0) normalized = 0;

                    byte finalVal;

                    if (onlyCorrection)
                    {
                        finalVal = (byte)normalized;
                    }
                    else
                    {
                        finalVal = (normalized < threshold) ? (byte)0 : (byte)255;
                    }

                    bufRes[idx] = finalVal;
                    bufRes[idx + 1] = finalVal;
                    bufRes[idx + 2] = finalVal;
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