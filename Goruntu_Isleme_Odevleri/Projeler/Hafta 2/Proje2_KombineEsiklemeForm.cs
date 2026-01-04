using System;
using System.Drawing;
using System.Drawing.Imaging; 
using System.Runtime.InteropServices; 
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_KombineEsiklemeForm : Form
    {
        private PictureBox pcbOriginal, pcbResult, pcbHistogram;
        private TrackBar tbGrayLower, tbGrayUpper, tbRedLower, tbRedUpper, tbGreenLower, tbGreenUpper, tbBlueLower, tbBlueUpper;
        private Button btnYukle, btnGeri;
        private Label lblGrayLower, lblGrayUpper, lblRed, lblGreen, lblBlue, lblRedLower, lblRedUpper, lblGreenLower, lblGreenUpper, lblBlueLower, lblBlueUpper;
        private Label lblHistogramBaslik;
        private Panel panelGrayControls, panelColorControls;
        private CheckBox chkColorMode;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap grayBitmap;

        public Proje2_KombineEsiklemeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Kombine Eşikleme ve Histogram Analizi";
            this.Size = new Size(1200, 700);

            SetupUI(); 
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_KombineEsiklemeForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void SetupUI()
        {
            pcbOriginal = new PictureBox() { Location = new Point(25, 25), Size = new Size(380, 350), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbResult = new PictureBox() { Location = new Point(420, 25), Size = new Size(380, 350), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblHistogramBaslik = new Label() { Text = "Renk Histogramı & Eşik Çizgileri", Location = new Point(815, 5), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbHistogram = new PictureBox() { Location = new Point(815, 25), Size = new Size(350, 350), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, SizeMode = PictureBoxSizeMode.StretchImage };

            panelGrayControls = new Panel() { Location = new Point(0, 390), Size = new Size(800, 100) };
            lblGrayLower = new Label() { Text = "Alt Eşik Değeri: 50", Location = new Point(25, 20), AutoSize = true, Font = new Font("Arial", 10) };
            tbGrayLower = new TrackBar() { Location = new Point(20, 40), Size = new Size(360, 45), Maximum = 255, Value = 50, TickFrequency = 10 };
            lblGrayUpper = new Label() { Text = "Üst Eşik Değeri: 200", Location = new Point(420, 20), AutoSize = true, Font = new Font("Arial", 10) };
            tbGrayUpper = new TrackBar() { Location = new Point(415, 40), Size = new Size(360, 45), Maximum = 255, Value = 200, TickFrequency = 10 };
            panelGrayControls.Controls.AddRange(new Control[] { lblGrayLower, tbGrayLower, lblGrayUpper, tbGrayUpper });

            panelColorControls = new Panel() { Location = new Point(0, 390), Size = new Size(1150, 200), Visible = false };
            int trackBarWidth = 256;
            int gap = 30;

            lblRed = new Label() { Text = "Kırmızı Kanal", Location = new Point(25 + 80, 0), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Red };
            lblRedLower = new Label() { Text = "Alt: 0", Location = new Point(25, 30), AutoSize = true };
            tbRedLower = new TrackBar() { Location = new Point(20, 50), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 0 };
            lblRedUpper = new Label() { Text = "Üst: 255", Location = new Point(25, 95), AutoSize = true };
            tbRedUpper = new TrackBar() { Location = new Point(20, 115), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 255 };

            int greenX = 25 + trackBarWidth + gap;
            lblGreen = new Label() { Text = "Yeşil Kanal", Location = new Point(greenX + 90, 0), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Green };
            lblGreenLower = new Label() { Text = "Alt: 0", Location = new Point(greenX, 30), AutoSize = true };
            tbGreenLower = new TrackBar() { Location = new Point(greenX - 5, 50), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 0 };
            lblGreenUpper = new Label() { Text = "Üst: 255", Location = new Point(greenX, 95), AutoSize = true };
            tbGreenUpper = new TrackBar() { Location = new Point(greenX - 5, 115), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 255 };

            int blueX = greenX + trackBarWidth + gap;
            lblBlue = new Label() { Text = "Mavi Kanal", Location = new Point(blueX + 90, 0), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Blue };
            lblBlueLower = new Label() { Text = "Alt: 0", Location = new Point(blueX, 30), AutoSize = true };
            tbBlueLower = new TrackBar() { Location = new Point(blueX - 5, 50), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 0 };
            lblBlueUpper = new Label() { Text = "Üst: 255", Location = new Point(blueX, 95), AutoSize = true };
            tbBlueUpper = new TrackBar() { Location = new Point(blueX - 5, 115), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 255 };

            panelColorControls.Controls.AddRange(new Control[] { lblRed, lblRedLower, tbRedLower, lblRedUpper, tbRedUpper, lblGreen, lblGreenLower, tbGreenLower, lblGreenUpper, tbGreenUpper, lblBlue, lblBlueLower, tbBlueLower, lblBlueUpper, tbBlueUpper });

            TrackBar[] allTrackBars = { tbGrayLower, tbGrayUpper, tbRedLower, tbRedUpper, tbGreenLower, tbGreenUpper, tbBlueLower, tbBlueUpper };
            foreach (var tb in allTrackBars) { tb.Scroll += new EventHandler(tb_Scroll); tb.Enabled = false; }

            int buttonsY = 600;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, buttonsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            chkColorMode = new CheckBox() { Text = "Renkli Eşikleme Modu", Location = new Point(200, buttonsY + 12), AutoSize = true, Font = new Font("Arial", 10) };
            chkColorMode.CheckedChanged += new EventHandler(chkColorMode_CheckedChanged);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(1015, buttonsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbOriginal, pcbResult, pcbHistogram, lblHistogramBaslik, panelGrayControls, panelColorControls, btnYukle, chkColorMode, btnGeri });
        }

        private void btnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(dialog.FileName);
                pcbOriginal.Image = originalBitmap;
                ConvertToGray();

                TrackBar[] allTrackBars = { tbGrayLower, tbGrayUpper, tbRedLower, tbRedUpper, tbGreenLower, tbGreenUpper, tbBlueLower, tbBlueUpper };
                foreach (var tb in allTrackBars) tb.Enabled = true;

                ApplyThresholding();
                DrawHistogram();
            }
        }

        private void ConvertToGray()
        {
            if (originalBitmap == null) return;
            grayBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            using (Graphics g = Graphics.FromImage(grayBitmap))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
                       new float[] {.3f, .3f, .3f, 0, 0},
                       new float[] {.59f, .59f, .59f, 0, 0},
                       new float[] {.11f, .11f, .11f, 0, 0},
                       new float[] {0, 0, 0, 1, 0},
                       new float[] {0, 0, 0, 0, 1}
                   });
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(originalBitmap, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), 0, 0, originalBitmap.Width, originalBitmap.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        private void chkColorMode_CheckedChanged(object sender, EventArgs e)
        {
            panelGrayControls.Visible = !chkColorMode.Checked;
            panelColorControls.Visible = chkColorMode.Checked;
            ApplyThresholding();
            DrawHistogram();
        }

        private void tb_Scroll(object sender, EventArgs e)
        {
            if (tbGrayLower.Value > tbGrayUpper.Value) tbGrayUpper.Value = tbGrayLower.Value;
            if (tbGrayUpper.Value < tbGrayLower.Value) tbGrayLower.Value = tbGrayUpper.Value;
            if (tbRedLower.Value > tbRedUpper.Value) tbRedUpper.Value = tbRedLower.Value;
            if (tbRedUpper.Value < tbRedLower.Value) tbRedLower.Value = tbRedUpper.Value;
            if (tbGreenLower.Value > tbGreenUpper.Value) tbGreenUpper.Value = tbGreenLower.Value;
            if (tbGreenUpper.Value < tbGreenLower.Value) tbGreenLower.Value = tbGreenUpper.Value;
            if (tbBlueLower.Value > tbBlueUpper.Value) tbBlueUpper.Value = tbBlueLower.Value;
            if (tbBlueUpper.Value < tbBlueLower.Value) tbBlueLower.Value = tbBlueUpper.Value;

            lblGrayLower.Text = $"Alt Eşik Değeri: {tbGrayLower.Value}"; lblGrayUpper.Text = $"Üst Eşik Değeri: {tbGrayUpper.Value}";
            lblRedLower.Text = $"Alt: {tbRedLower.Value}"; lblRedUpper.Text = $"Üst: {tbRedUpper.Value}";
            lblGreenLower.Text = $"Alt: {tbGreenLower.Value}"; lblGreenUpper.Text = $"Üst: {tbGreenUpper.Value}";
            lblBlueLower.Text = $"Alt: {tbBlueLower.Value}"; lblBlueUpper.Text = $"Üst: {tbBlueUpper.Value}";

            ApplyThresholding();
            DrawHistogram();
        }

        private void DrawHistogram()
        {
            if (originalBitmap == null) return;

            long[] histGray = new long[256];
            long[] histR = new long[256];
            long[] histG = new long[256];
            long[] histB = new long[256];
            long maxVal = 0;

            Bitmap source = chkColorMode.Checked ? originalBitmap : grayBitmap;

            BitmapData data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(data.Stride) * source.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(data.Scan0, rgbValues, 0, bytes);
            source.UnlockBits(data);

            int width = source.Width;
            int height = source.Height;
            int stride = data.Stride;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int i = rowStart + x * 3;
                    byte b = rgbValues[i];
                    byte g = rgbValues[i + 1];
                    byte r = rgbValues[i + 2];

                    if (chkColorMode.Checked)
                    {
                        histB[b]++;
                        histG[g]++;
                        histR[r]++;
                    }
                    else
                    {
                        histGray[b]++; // Gri resimde R=G=B olduğu için herhangi birini alabiliriz
                    }
                }
            }

            // Maksimum değeri bul
            if (chkColorMode.Checked)
            {
                for (int i = 0; i < 256; i++)
                {
                    if (histR[i] > maxVal) maxVal = histR[i];
                    if (histG[i] > maxVal) maxVal = histG[i];
                    if (histB[i] > maxVal) maxVal = histB[i];
                }
            }
            else
            {
                for (int i = 0; i < 256; i++) if (histGray[i] > maxVal) maxVal = histGray[i];
            }

            // Çizim
            Bitmap histImg = new Bitmap(pcbHistogram.Width, pcbHistogram.Height);
            using (Graphics g = Graphics.FromImage(histImg))
            {
                g.Clear(Color.White);
                if (maxVal == 0) maxVal = 1; // Sıfıra bölme hatası önlemi

                float scaleX = (float)pcbHistogram.Width / 256;
                float scaleY = (float)pcbHistogram.Height / maxVal;

                if (chkColorMode.Checked)
                {
                    for (int i = 0; i < 256; i++)
                    {
                        g.DrawLine(new Pen(Color.FromArgb(100, 255, 0, 0)), i * scaleX, pcbHistogram.Height, i * scaleX, pcbHistogram.Height - (histR[i] * scaleY));
                        g.DrawLine(new Pen(Color.FromArgb(100, 0, 255, 0)), i * scaleX, pcbHistogram.Height, i * scaleX, pcbHistogram.Height - (histG[i] * scaleY));
                        g.DrawLine(new Pen(Color.FromArgb(100, 0, 0, 255)), i * scaleX, pcbHistogram.Height, i * scaleX, pcbHistogram.Height - (histB[i] * scaleY));
                    }
                    DrawThresholdLine(g, tbRedLower.Value, Color.Red, scaleX);
                    DrawThresholdLine(g, tbRedUpper.Value, Color.Red, scaleX);
                    DrawThresholdLine(g, tbGreenLower.Value, Color.Green, scaleX);
                    DrawThresholdLine(g, tbGreenUpper.Value, Color.Green, scaleX);
                    DrawThresholdLine(g, tbBlueLower.Value, Color.Blue, scaleX);
                    DrawThresholdLine(g, tbBlueUpper.Value, Color.Blue, scaleX);
                }
                else
                {
                    for (int i = 0; i < 256; i++)
                    {
                        g.DrawLine(Pens.Black, i * scaleX, pcbHistogram.Height, i * scaleX, pcbHistogram.Height - (histGray[i] * scaleY));
                    }
                    DrawThresholdLine(g, tbGrayLower.Value, Color.DarkGray, scaleX);
                    DrawThresholdLine(g, tbGrayUpper.Value, Color.DarkGray, scaleX);
                }
            }
            pcbHistogram.Image = histImg;
        }

        private void ApplyThresholding()
        {
            if (originalBitmap == null) return;

            Bitmap resultBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            Bitmap source = chkColorMode.Checked ? originalBitmap : grayBitmap;

            // Verileri kilitle
            BitmapData srcData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(srcData.Stride) * source.Height;
            byte[] srcValues = new byte[bytes];
            byte[] dstValues = new byte[bytes];

            // Kaynaktan kopyala
            Marshal.Copy(srcData.Scan0, srcValues, 0, bytes);

            int width = source.Width;
            int height = source.Height;
            int stride = srcData.Stride;

            // Eşik değerlerini önbelleğe al (Hız için)
            int rL = tbRedLower.Value, rU = tbRedUpper.Value;
            int gL = tbGreenLower.Value, gU = tbGreenUpper.Value;
            int bL = tbBlueLower.Value, bU = tbBlueUpper.Value;
            int grayL = tbGrayLower.Value, grayU = tbGrayUpper.Value;
            bool isColor = chkColorMode.Checked;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int i = rowStart + x * 3;
                    byte b = srcValues[i];
                    byte g = srcValues[i + 1];
                    byte r = srcValues[i + 2];

                    if (isColor)
                    {
                        byte newB = (b >= bL && b <= bU) ? b : (byte)0;
                        byte newG = (g >= gL && g <= gU) ? g : (byte)0;
                        byte newR = (r >= rL && r <= rU) ? r : (byte)0;
                        dstValues[i] = newB;
                        dstValues[i + 1] = newG;
                        dstValues[i + 2] = newR;
                    }
                    else
                    {
                        // Gri modda sadece r'ye bakmak yeterli (r=g=b)
                        if (r >= grayL && r <= grayU)
                        {
                            dstValues[i] = 255; dstValues[i + 1] = 255; dstValues[i + 2] = 255;
                        }
                        else
                        {
                            dstValues[i] = 0; dstValues[i + 1] = 0; dstValues[i + 2] = 0;
                        }
                    }
                }
            }

            // Hedefe kopyala
            Marshal.Copy(dstValues, 0, dstData.Scan0, bytes);

            source.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            pcbResult.Image = resultBitmap;
        }

        private void DrawThresholdLine(Graphics g, int value, Color color, float scaleX)
        {
            Pen p = new Pen(color, 2);
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            g.DrawLine(p, value * scaleX, 0, value * scaleX, pcbHistogram.Height);
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