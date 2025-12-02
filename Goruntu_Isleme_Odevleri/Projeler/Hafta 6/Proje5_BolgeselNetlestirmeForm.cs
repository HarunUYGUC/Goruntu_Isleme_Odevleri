using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_BolgeselNetlestirmeForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnGeri;
        private Label lblBoyut, lblSekil, lblKenar, lblYogunluk, lblBilgi;
        private TrackBar tbBoyut, tbKenar, tbYogunluk;
        private ComboBox cmbSekil, cmbIslemTipi, cmbAlgoritma;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Point mousePos = Point.Empty;
        private bool isMouseOver = false;

        public Proje5_BolgeselNetlestirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: Gelişmiş Bölgesel İşlem (Focus & Blur Tool)";

            this.DoubleBuffered = true;

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };
            pcbResim.MouseMove += PcbResim_MouseMove;
            pcbResim.MouseEnter += (s, e) => isMouseOver = true;
            pcbResim.MouseLeave += (s, e) => { isMouseOver = false; pcbResim.Invalidate(); };
            pcbResim.Paint += PcbResim_Paint;

            int controlsX = 750;
            int startY = 25;
            int gap = 60;

            // 1. İşlem Tipi (Netleştirme mi, Bulanıklaştırma mı?)
            Label lblIslem = new Label() { Text = "İşlem Tipi:", Location = new Point(controlsX, startY), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            cmbIslemTipi = new ComboBox() { Location = new Point(controlsX, startY + 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbIslemTipi.Items.AddRange(new object[] { "Bölgeyi Netleştir (Focus)", "Bölgeyi Bulanıklaştır (Blur)" });
            cmbIslemTipi.SelectedIndex = 0;
            cmbIslemTipi.SelectedIndexChanged += (s, e) => pcbResim.Invalidate();

            // 2. Algoritma Seçimi
            Label lblAlgo = new Label() { Text = "Algoritma:", Location = new Point(controlsX, startY + gap), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            cmbAlgoritma = new ComboBox() { Location = new Point(controlsX, startY + gap + 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAlgoritma.Items.AddRange(new object[] { "Mean (Ortalama)", "Gauss (Gaussian)", "Kenar Bulma (Laplacian)" });
            cmbAlgoritma.SelectedIndex = 0;
            cmbAlgoritma.SelectedIndexChanged += (s, e) => pcbResim.Invalidate();

            startY += gap * 2;

            // 3. Boyut (Size)
            lblBoyut = new Label() { Text = "Boyut (Size): 100", Location = new Point(controlsX, startY), AutoSize = true };
            tbBoyut = new TrackBar() { Location = new Point(controlsX, startY + 20), Size = new Size(200, 45), Minimum = 20, Maximum = 300, Value = 100, TickFrequency = 20 };
            tbBoyut.Scroll += (s, e) => { lblBoyut.Text = $"Boyut (Size): {tbBoyut.Value}"; pcbResim.Invalidate(); };

            // 4. Şekil (Shape)
            lblSekil = new Label() { Text = "Şekil (Shape):", Location = new Point(controlsX, startY + gap), AutoSize = true };
            cmbSekil = new ComboBox() { Location = new Point(controlsX, startY + gap + 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSekil.Items.AddRange(new object[] { "Daire (Circle)", "Kare (Square)" });
            cmbSekil.SelectedIndex = 0;
            cmbSekil.SelectedIndexChanged += (s, e) => pcbResim.Invalidate();

            // 5. Kenar Yumuşatma (Edge)
            lblKenar = new Label() { Text = "Yumuşak Geçiş (Edge %): 50", Location = new Point(controlsX, startY + gap * 2), AutoSize = true };
            tbKenar = new TrackBar() { Location = new Point(controlsX, startY + gap * 2 + 20), Size = new Size(200, 45), Minimum = 0, Maximum = 100, Value = 50, TickFrequency = 10 };
            tbKenar.Scroll += (s, e) => { lblKenar.Text = $"Yumuşak Geçiş (Edge %): {tbKenar.Value}"; pcbResim.Invalidate(); };

            // 6. Yoğunluk (Intensity)
            lblYogunluk = new Label() { Text = "Yoğunluk (Intensity): 5", Location = new Point(controlsX, startY + gap * 3), AutoSize = true };
            tbYogunluk = new TrackBar() { Location = new Point(controlsX, startY + gap * 3 + 20), Size = new Size(200, 45), Minimum = 1, Maximum = 20, Value = 5, TickFrequency = 2 };
            tbYogunluk.Scroll += (s, e) => { lblYogunluk.Text = $"Yoğunluk (Intensity): {tbYogunluk.Value}"; pcbResim.Invalidate(); };

            lblBilgi = new Label() { Text = "Mouse ile resim üzerinde gezinin.", Location = new Point(25, 540), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 580), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(575, 580), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] {
                pcbResim, lblIslem, cmbIslemTipi, lblAlgo, cmbAlgoritma,
                lblBoyut, tbBoyut, lblSekil, cmbSekil, lblKenar, tbKenar, lblYogunluk, tbYogunluk,
                lblBilgi, btnYukle, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje5_BolgeselNetlestirmeForm";
            this.Size = new Size(1000, 700);
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

            // 1. Taban Resmi Çiz (İşlem tipine göre değişir)
            // Eğer "Bölgeyi Netleştir" seçiliyse, arka plan BULANIK olmalı.
            // Eğer "Bölgeyi Bulanıklaştır" seçiliyse, arka plan NET (Orijinal) olmalı.

            bool isFocusTool = cmbIslemTipi.SelectedIndex == 0;
            Bitmap baseImage = isFocusTool ? GetFilteredImage(originalBitmap, true) : originalBitmap; // Arka plan

            e.Graphics.DrawImage(originalBitmap, imgRect);

            if (!isMouseOver) return;

            Point imgPoint = ConvertPointToImage(mousePos, imgRect);
            if (imgPoint.X < 0 || imgPoint.X >= originalBitmap.Width || imgPoint.Y < 0 || imgPoint.Y >= originalBitmap.Height) return;

            int size = tbBoyut.Value;
            Rectangle roi = new Rectangle(imgPoint.X - size / 2, imgPoint.Y - size / 2, size, size);
            Rectangle clippedRoi = Rectangle.Intersect(roi, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height));

            if (clippedRoi.Width <= 0 || clippedRoi.Height <= 0) return;

            Point roiCenter = new Point(roi.X + size / 2, roi.Y + size / 2);

            // ROI için işlenmiş parçayı al
            Bitmap processedPatch = GenerateProcessedPatch(clippedRoi, roiCenter, size);

            Rectangle screenRect = ConvertRectToScreen(clippedRoi, imgRect);
            e.Graphics.DrawImage(processedPatch, screenRect);

            // Çerçeve çiz
            using (Pen p = new Pen(Color.FromArgb(100, 255, 255, 255), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
            {
                if (cmbSekil.SelectedIndex == 1) e.Graphics.DrawRectangle(p, screenRect);
                else e.Graphics.DrawEllipse(p, screenRect);
            }
        }

        private Bitmap GenerateProcessedPatch(Rectangle roi, Point center, int size)
        {
            Bitmap patch = new Bitmap(roi.Width, roi.Height);
            int intensity = tbYogunluk.Value;
            bool isFocus = cmbIslemTipi.SelectedIndex == 0;
            int algoIndex = cmbAlgoritma.SelectedIndex; // 0: Mean, 1: Gauss, 2: Kenar

            // Şekil ve Geçiş Ayarları
            bool isCircle = cmbSekil.SelectedIndex == 0;
            float maxDist = size / 2.0f;
            float fadeStart = maxDist * (1.0f - (tbKenar.Value / 100.0f));

            // Kernel/Matris Hazırlığı (Eğer gerekliyse)
            double[,] kernel = null;
            if (algoIndex == 1) // Gauss
                kernel = CalculateGaussianKernel(intensity * 2 + 1, intensity / 2.0);
            else if (algoIndex == 2) // Kenar Bulma (Laplacian) - Netleştirme için
                kernel = new double[,] { { 0, -1, 0 }, { -1, 4 + intensity, -1 }, { 0, -1, 0 } }; // Basit keskinleştirme çekirdeği

            for (int y = 0; y < roi.Height; y++)
            {
                for (int x = 0; x < roi.Width; x++)
                {
                    int globalX = roi.X + x;
                    int globalY = roi.Y + y;

                    // Uzaklık ve Maske
                    float dist = 0;
                    float dx = Math.Abs(globalX - center.X);
                    float dy = Math.Abs(globalY - center.Y);

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

                    // Piksel İşleme
                    Color resultColor;
                    if (isFocus) // Netleştirme Modu
                    {
                        if (algoIndex == 2) // Kenar Bulma Matrisi ile Netleştirme
                        {
                            resultColor = ApplyConvolution(globalX, globalY, kernel);
                        }
                        else // Mean/Gauss ile Unsharp Masking
                        {
                            Color cOrg = originalBitmap.GetPixel(globalX, globalY);
                            Color cBlur;
                            if (algoIndex == 1) cBlur = ApplyPixelGauss(globalX, globalY, intensity * 2 + 1, kernel);
                            else cBlur = ApplyPixelMean(globalX, globalY, intensity * 2 + 1);

                            // Basit Unsharp Mask
                            int r = (int)(cOrg.R + (cOrg.R - cBlur.R) * (intensity / 5.0)); // Yoğunluk çarpanı
                            int g = (int)(cOrg.G + (cOrg.G - cBlur.G) * (intensity / 5.0));
                            int b = (int)(cOrg.B + (cOrg.B - cBlur.B) * (intensity / 5.0));
                            resultColor = Color.FromArgb(Clamp(r), Clamp(g), Clamp(b));
                        }
                    }
                    else // Bulanıklaştırma Modu
                    {
                        if (algoIndex == 1) resultColor = ApplyPixelGauss(globalX, globalY, intensity * 2 + 1, kernel);
                        else if (algoIndex == 2) resultColor = ApplyConvolution(globalX, globalY, kernel); // Kenar bulma (Blur değil ama efekt olarak)
                        else resultColor = ApplyPixelMean(globalX, globalY, intensity * 2 + 1);
                    }

                    patch.SetPixel(x, y, Color.FromArgb(alpha, resultColor));
                }
            }
            return patch;
        }

        // Yardımcı Filtre Metotları

        private Color ApplyPixelMean(int x, int y, int kSize)
        {
            int r = kSize / 2;
            int rSum = 0, gSum = 0, bSum = 0, count = 0;
            for (int ky = -r; ky <= r; ky += 2)
            { // Hız için adım atla
                for (int kx = -r; kx <= r; kx += 2)
                {
                    int px = Math.Max(0, Math.Min(originalBitmap.Width - 1, x + kx));
                    int py = Math.Max(0, Math.Min(originalBitmap.Height - 1, y + ky));
                    Color c = originalBitmap.GetPixel(px, py);
                    rSum += c.R; gSum += c.G; bSum += c.B; count++;
                }
            }
            return Color.FromArgb(rSum / count, gSum / count, bSum / count);
        }

        private Color ApplyPixelGauss(int x, int y, int kSize, double[,] kernel)
        {
            int r = kSize / 2;
            double rSum = 0, gSum = 0, bSum = 0;
            for (int ky = -r; ky <= r; ky++)
            {
                for (int kx = -r; kx <= r; kx++)
                {
                    int px = Math.Max(0, Math.Min(originalBitmap.Width - 1, x + kx));
                    int py = Math.Max(0, Math.Min(originalBitmap.Height - 1, y + ky));
                    Color c = originalBitmap.GetPixel(px, py);
                    double w = kernel[ky + r, kx + r];
                    rSum += c.R * w; gSum += c.G * w; bSum += c.B * w;
                }
            }
            return Color.FromArgb(Clamp((int)rSum), Clamp((int)gSum), Clamp((int)bSum));
        }

        private Color ApplyConvolution(int x, int y, double[,] kernel)
        {
            int r = 1; // 3x3 varsayalım
            double rSum = 0, gSum = 0, bSum = 0;
            for (int ky = -r; ky <= r; ky++)
            {
                for (int kx = -r; kx <= r; kx++)
                {
                    int px = Math.Max(0, Math.Min(originalBitmap.Width - 1, x + kx));
                    int py = Math.Max(0, Math.Min(originalBitmap.Height - 1, y + ky));
                    Color c = originalBitmap.GetPixel(px, py);
                    double w = kernel[ky + r, kx + r];
                    rSum += c.R * w; gSum += c.G * w; bSum += c.B * w;
                }
            }
            return Color.FromArgb(Clamp((int)rSum), Clamp((int)gSum), Clamp((int)bSum));
        }

        private double[,] CalculateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int r = size / 2;
            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    double val = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    kernel[y + r, x + r] = val;
                    sum += val;
                }
            }
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) kernel[y, x] /= sum;
            return kernel;
        }

        private Bitmap GetFilteredImage(Bitmap src, bool blur)
        {
            return new Bitmap(src);
        }

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

        private int Clamp(int value) => Math.Max(0, Math.Min(255, value));

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