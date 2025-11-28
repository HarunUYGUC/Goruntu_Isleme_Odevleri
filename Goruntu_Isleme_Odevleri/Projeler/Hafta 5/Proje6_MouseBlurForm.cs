using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje6_MouseBlurForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnGeri;
        private Label lblBoyut, lblSekil, lblKenar, lblYogunluk, lblBilgi, lblAlgoritma;
        private TrackBar tbBoyut, tbKenar, tbYogunluk;
        private ComboBox cmbSekil;
        private RadioButton rbMean, rbMedian, rbGauss;
        private GroupBox grpAlgoritma;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Point mousePos = Point.Empty;
        private bool isMouseOver = false;

        public Proje6_MouseBlurForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 6: Mouse Takip Eden Dinamik Blur";

            this.DoubleBuffered = true;

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };
            pcbResim.MouseMove += PcbResim_MouseMove;
            pcbResim.MouseEnter += (s, e) => { isMouseOver = true; };
            pcbResim.MouseLeave += (s, e) => { isMouseOver = false; pcbResim.Invalidate(); };
            pcbResim.Paint += PcbResim_Paint;

            int controlsX = 650;
            int startY = 25;
            int gap = 60;

            // Algoritma Seçimi (Radio Buttons)
            grpAlgoritma = new GroupBox() { Text = "Bulanıklaştırma Algoritması", Location = new Point(controlsX, startY), Size = new Size(200, 100) };
            rbMean = new RadioButton() { Text = "Mean (Ortalama)", Location = new Point(10, 20), AutoSize = true, Checked = true };
            rbMedian = new RadioButton() { Text = "Median (Ortanca)", Location = new Point(10, 45), AutoSize = true };
            rbGauss = new RadioButton() { Text = "Gauss (Gaussian)", Location = new Point(10, 70), AutoSize = true };

            // Algoritma değişince yeniden çiz
            rbMean.CheckedChanged += (s, e) => pcbResim.Invalidate();
            rbMedian.CheckedChanged += (s, e) => pcbResim.Invalidate();
            rbGauss.CheckedChanged += (s, e) => pcbResim.Invalidate();

            grpAlgoritma.Controls.AddRange(new Control[] { rbMean, rbMedian, rbGauss });

            startY += 110;

            lblBoyut = new Label() { Text = "Boyut (Size): 50", Location = new Point(controlsX, startY), AutoSize = true };
            tbBoyut = new TrackBar() { Location = new Point(controlsX, startY + 20), Size = new Size(200, 45), Minimum = 20, Maximum = 200, Value = 50, TickFrequency = 20 };
            tbBoyut.Scroll += (s, e) => { lblBoyut.Text = $"Boyut (Size): {tbBoyut.Value}"; pcbResim.Invalidate(); };

            lblSekil = new Label() { Text = "Şekil (Shape):", Location = new Point(controlsX, startY + gap), AutoSize = true };
            cmbSekil = new ComboBox() { Location = new Point(controlsX, startY + gap + 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSekil.Items.AddRange(new object[] { "Kare (Square)", "Daire (Circle)" });
            cmbSekil.SelectedIndex = 1;
            cmbSekil.SelectedIndexChanged += (s, e) => { pcbResim.Invalidate(); };

            lblKenar = new Label() { Text = "Kenar Yumuşatma (Edge %): 50", Location = new Point(controlsX, startY + gap * 2), AutoSize = true };
            tbKenar = new TrackBar() { Location = new Point(controlsX, startY + gap * 2 + 20), Size = new Size(200, 45), Minimum = 0, Maximum = 100, Value = 50, TickFrequency = 10 };
            tbKenar.Scroll += (s, e) => { lblKenar.Text = $"Kenar Yumuşatma (Edge %): {tbKenar.Value}"; pcbResim.Invalidate(); };

            lblYogunluk = new Label() { Text = "Yoğunluk (Intensity): 3", Location = new Point(controlsX, startY + gap * 3), AutoSize = true };
            tbYogunluk = new TrackBar() { Location = new Point(controlsX, startY + gap * 3 + 20), Size = new Size(200, 45), Minimum = 1, Maximum = 15, Value = 3, TickFrequency = 2 };
            tbYogunluk.Scroll += (s, e) => {
                int kernelSize = tbYogunluk.Value * 2 + 1;
                lblYogunluk.Text = $"Yoğunluk (Intensity): {kernelSize}x{kernelSize}";
                pcbResim.Invalidate();
            };

            lblBilgi = new Label() { Text = "Mouse ile resim üzerinde gezinin.", Location = new Point(25, 540), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(475, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, grpAlgoritma, lblBoyut, tbBoyut, lblSekil, cmbSekil, lblKenar, tbKenar, lblYogunluk, tbYogunluk, lblBilgi, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje6_MouseBlurForm";
            this.Size = new Size(900, 700); 
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
                originalBitmap = new Bitmap(dialog.FileName);
                pcbResim.Image = null;
                pcbResim.Invalidate();
            }
        }

        private void PcbResim_MouseMove(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;
            mousePos = e.Location;
            pcbResim.Invalidate();
        }

        private void PcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (originalBitmap == null) return;

            Rectangle imgRect = GetImageRectangle();
            e.Graphics.DrawImage(originalBitmap, imgRect);

            if (!isMouseOver) return;

            Point imgPoint = ConvertPointToImage(mousePos, imgRect);

            if (imgPoint.X < 0 || imgPoint.X >= originalBitmap.Width || imgPoint.Y < 0 || imgPoint.Y >= originalBitmap.Height) return;

            int size = tbBoyut.Value;
            Rectangle roi = new Rectangle(imgPoint.X - size / 2, imgPoint.Y - size / 2, size, size);

            Rectangle clippedRoi = Rectangle.Intersect(roi, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height));
            if (clippedRoi.Width <= 0 || clippedRoi.Height <= 0) return;

            Point roiCenter = new Point(roi.X + size / 2, roi.Y + size / 2);

            Bitmap blurPatch = GenerateBlendedPatch(clippedRoi, roiCenter, size);

            Rectangle screenRect = ConvertRectToScreen(clippedRoi, imgRect);
            e.Graphics.DrawImage(blurPatch, screenRect);
        }

        private Bitmap GenerateBlendedPatch(Rectangle roi, Point originalCenter, int originalSize)
        {
            Bitmap patch = new Bitmap(roi.Width, roi.Height);

            // Yoğunluk ayarı:
            // Mean/Median için Kernel Boyutu (3, 5, 7...)
            // Gauss için Sigma değeri (0.5, 1.0, 1.5...)
            int intensityLevel = tbYogunluk.Value;
            int kernelSize = intensityLevel * 2 + 1;
            int radius = kernelSize / 2;
            double sigma = intensityLevel / 2.0; // Gauss için sigma

            bool isCircle = cmbSekil.SelectedIndex == 1;
            float maxDist = originalSize / 2.0f;
            float fadeStart = maxDist * (1.0f - (tbKenar.Value / 100.0f));

            // Gauss Kernelini Önceden Hesapla (Eğer Gauss seçiliyse)
            double[,] gaussianKernel = null;
            if (rbGauss.Checked)
            {
                gaussianKernel = CalculateGaussianKernel(kernelSize, sigma);
            }

            for (int y = 0; y < roi.Height; y++)
            {
                for (int x = 0; x < roi.Width; x++)
                {
                    int globalX = roi.X + x;
                    int globalY = roi.Y + y;

                    // Şekil ve Maskeleme Kontrolü
                    float dist = 0;
                    float dx = Math.Abs(globalX - originalCenter.X);
                    float dy = Math.Abs(globalY - originalCenter.Y);

                    if (isCircle) dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    else dist = Math.Max(dx, dy);

                    if (dist > maxDist)
                    {
                        patch.SetPixel(x, y, Color.Transparent);
                        continue;
                    }

                    int alpha = 255;
                    if (dist > fadeStart && maxDist > fadeStart)
                    {
                        float factor = 1.0f - ((dist - fadeStart) / (maxDist - fadeStart));
                        alpha = (int)(255 * factor);
                        if (alpha < 0) alpha = 0;
                    }

                    // ALGORİTMA SEÇİMİ 
                    Color blurredColor;

                    if (rbMean.Checked)
                    {
                        blurredColor = ApplyPixelMean(globalX, globalY, radius);
                    }
                    else if (rbMedian.Checked)
                    {
                        blurredColor = ApplyPixelMedian(globalX, globalY, radius);
                    }
                    else // Gauss
                    {
                        blurredColor = ApplyPixelGauss(globalX, globalY, radius, gaussianKernel);
                    }

                    patch.SetPixel(x, y, Color.FromArgb(alpha, blurredColor));
                }
            }
            return patch;
        }

        // Mean (Ortalama) Filtresi (Tek Piksel İçin)
        private Color ApplyPixelMean(int x, int y, int radius)
        {
            int rSum = 0, gSum = 0, bSum = 0, count = 0;
            for (int ky = -radius; ky <= radius; ky++)
            {
                for (int kx = -radius; kx <= radius; kx++)
                {
                    int pX = x + kx;
                    int pY = y + ky;
                    if (pX >= 0 && pX < originalBitmap.Width && pY >= 0 && pY < originalBitmap.Height)
                    {
                        Color c = originalBitmap.GetPixel(pX, pY);
                        rSum += c.R; gSum += c.G; bSum += c.B;
                        count++;
                    }
                }
            }
            return Color.FromArgb(rSum / count, gSum / count, bSum / count);
        }

        // Median (Ortanca) Filtresi (Tek Piksel İçin)
        private Color ApplyPixelMedian(int x, int y, int radius)
        {
            List<int> r = new List<int>();
            List<int> g = new List<int>();
            List<int> b = new List<int>();

            for (int ky = -radius; ky <= radius; ky++)
            {
                for (int kx = -radius; kx <= radius; kx++)
                {
                    int pX = x + kx;
                    int pY = y + ky;
                    if (pX >= 0 && pX < originalBitmap.Width && pY >= 0 && pY < originalBitmap.Height)
                    {
                        Color c = originalBitmap.GetPixel(pX, pY);
                        r.Add(c.R); g.Add(c.G); b.Add(c.B);
                    }
                }
            }
            r.Sort(); g.Sort(); b.Sort();
            int mid = r.Count / 2;
            return Color.FromArgb(r[mid], g[mid], b[mid]);
        }

        // Gauss Filtresi (Tek Piksel İçin)
        private Color ApplyPixelGauss(int x, int y, int radius, double[,] kernel)
        {
            double rSum = 0, gSum = 0, bSum = 0;
            for (int ky = -radius; ky <= radius; ky++)
            {
                for (int kx = -radius; kx <= radius; kx++)
                {
                    int pX = Math.Max(0, Math.Min(originalBitmap.Width - 1, x + kx));
                    int pY = Math.Max(0, Math.Min(originalBitmap.Height - 1, y + ky));

                    Color c = originalBitmap.GetPixel(pX, pY);
                    double w = kernel[ky + radius, kx + radius];

                    rSum += c.R * w; gSum += c.G * w; bSum += c.B * w;
                }
            }
            return Color.FromArgb(
                Math.Min(255, (int)rSum),
                Math.Min(255, (int)gSum),
                Math.Min(255, (int)bSum));
        }

        private double[,] CalculateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int radius = size / 2;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    double val = (1 / (2 * Math.PI * sigma * sigma)) * Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    kernel[y + radius, x + radius] = val;
                    sum += val;
                }
            }
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    kernel[y, x] /= sum;
            return kernel;
        }

        // Yardımcı Dönüşüm Metotları 
        private Rectangle GetImageRectangle()
        {
            if (originalBitmap == null) return Rectangle.Empty;
            Size pcbSize = pcbResim.ClientSize;
            Size imgSize = originalBitmap.Size;
            float scale = Math.Min((float)pcbSize.Width / imgSize.Width, (float)pcbSize.Height / imgSize.Height);
            int w = (int)(imgSize.Width * scale);
            int h = (int)(imgSize.Height * scale);
            return new Rectangle((pcbSize.Width - w) / 2, (pcbSize.Height - h) / 2, w, h);
        }

        private Point ConvertPointToImage(Point pcbPoint, Rectangle imgRect)
        {
            float scale = (float)originalBitmap.Width / imgRect.Width;
            int x = (int)((pcbPoint.X - imgRect.X) * scale);
            int y = (int)((pcbPoint.Y - imgRect.Y) * scale);
            return new Point(x, y);
        }

        private Rectangle ConvertRectToScreen(Rectangle imgRectPart, Rectangle displayRect)
        {
            float scale = (float)displayRect.Width / originalBitmap.Width;
            int x = (int)(imgRectPart.X * scale) + displayRect.X;
            int y = (int)(imgRectPart.Y * scale) + displayRect.Y;
            int w = (int)(imgRectPart.Width * scale);
            int h = (int)(imgRectPart.Height * scale);
            return new Rectangle(x, y, w + 1, h + 1);
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