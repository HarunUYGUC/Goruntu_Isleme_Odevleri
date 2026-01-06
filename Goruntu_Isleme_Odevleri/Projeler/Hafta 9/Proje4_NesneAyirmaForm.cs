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
        private Label lblBg, lblObj, lblRes, lblThreshold;
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

            this.Controls.AddRange(new Control[] {
                lblBg, pcbBackground, btnLoadBg,
                lblObj, pcbObject, btnLoadObj,
                lblRes, pcbResult,
                lblThreshold, tbThreshold, btnProcess, btnBack
            });
        }

        private void LoadImage(ref Bitmap targetBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap sourceFile = new Bitmap(ofd.FileName))
                {
                    Bitmap safeFormatBmp;

                    if (sourceFile.PixelFormat != PixelFormat.Format24bppRgb)
                    {
                        safeFormatBmp = new Bitmap(sourceFile.Width, sourceFile.Height, PixelFormat.Format24bppRgb);
                        using (Graphics g = Graphics.FromImage(safeFormatBmp))
                        {
                            g.Clear(Color.Black);
                            g.DrawImage(sourceFile, 0, 0, sourceFile.Width, sourceFile.Height);
                        }
                    }
                    else
                    {
                        safeFormatBmp = (Bitmap)sourceFile.Clone();
                    }

                    using (safeFormatBmp)
                    {
                        float scaleW = (float)pcb.Width / safeFormatBmp.Width;
                        float scaleH = (float)pcb.Height / safeFormatBmp.Height;
                        float scale = Math.Min(scaleW, scaleH);

                        if (scale > 1) scale = 1;

                        int newW = (int)(safeFormatBmp.Width * scale);
                        int newH = (int)(safeFormatBmp.Height * scale);

                        if (targetBmp != null) targetBmp.Dispose();

                        targetBmp = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);

                        using (Graphics g = Graphics.FromImage(targetBmp))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(safeFormatBmp, 0, 0, newW, newH);
                        }
                    }
                }

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

            if (bmpObject.Width != bmpBackground.Width || bmpObject.Height != bmpBackground.Height)
            {
                Bitmap resizedObj = new Bitmap(bmpBackground.Width, bmpBackground.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(resizedObj)) g.DrawImage(bmpObject, 0, 0, resizedObj.Width, resizedObj.Height);
                bmpObject = resizedObj;
                pcbObject.Image = bmpObject;
            }

            Bitmap result = SubtractAndThreshold(bmpBackground, bmpObject, tbThreshold.Value);

            pcbResult.Image = result;
            this.Cursor = Cursors.Default;
        }

        private Bitmap SubtractAndThreshold(Bitmap bg, Bitmap obj, int threshold)
        {
            int w = bg.Width;
            int h = bg.Height;
            Bitmap result = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData bgData = bg.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData objData = obj.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData resData = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = bgData.Stride;
            int bytes = stride * h;

            byte[] bgBuffer = new byte[bytes];
            byte[] objBuffer = new byte[bytes];
            byte[] resBuffer = new byte[bytes];

            Marshal.Copy(bgData.Scan0, bgBuffer, 0, bytes);
            Marshal.Copy(objData.Scan0, objBuffer, 0, bytes);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = (y * stride) + (x * 3);

                    if (idx + 2 >= bytes) continue;

                    int bgB = bgBuffer[idx];
                    int objB = objBuffer[idx];

                    int bgG = bgBuffer[idx + 1];
                    int objG = objBuffer[idx + 1];

                    int bgR = bgBuffer[idx + 2];
                    int objR = objBuffer[idx + 2];

                    int bgGray = (bgB + bgG + bgR) / 3;
                    int objGray = (objB + objG + objR) / 3;

                    int diff = Math.Abs(objGray - bgGray);

                    byte val = (diff > threshold) ? (byte)255 : (byte)0;

                    resBuffer[idx] = val;
                    resBuffer[idx + 1] = val;
                    resBuffer[idx + 2] = val;
                }
            }

            Marshal.Copy(resBuffer, 0, resData.Scan0, bytes);
            bg.UnlockBits(bgData);
            obj.UnlockBits(objData);
            result.UnlockBits(resData);

            return result;
        }
    }
}