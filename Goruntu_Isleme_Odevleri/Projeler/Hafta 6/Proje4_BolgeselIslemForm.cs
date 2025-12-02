using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_BolgeselIslemForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnUygula, btnTemizle, btnGeri;
        private GroupBox grpIcBolge, grpDisBolge;
        private RadioButton rbIcMean, rbIcMedian, rbIcGauss;
        private RadioButton rbDisMean, rbDisMedian, rbDisGauss;
        private Label lblBilgi;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private List<Point> polygonPoints = new List<Point>();

        // İşlem yoğunluğu için sabit değerler
        private const int KERNEL_SIZE = 5; // Bulanıklaştırma ve Netleştirme matris boyutu
        private const double SHARPEN_AMOUNT = 1.5; // Netleştirme şiddeti

        public Proje4_BolgeselIslemForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Bölgesel Netleştirme ve Bulanıklaştırma";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };
            pcbResim.MouseClick += PcbResim_MouseClick;
            pcbResim.Paint += PcbResim_Paint;

            int controlsX = 750;
            int startY = 25;

            // İç Bölge (Netleştirme) Ayarları
            grpIcBolge = new GroupBox() { Text = "İç Bölge (Netleştirme)", Location = new Point(controlsX, startY), Size = new Size(200, 100) };
            rbIcMean = new RadioButton() { Text = "Mean", Location = new Point(10, 20), AutoSize = true, Checked = true };
            rbIcMedian = new RadioButton() { Text = "Median", Location = new Point(10, 45), AutoSize = true };
            rbIcGauss = new RadioButton() { Text = "Gauss", Location = new Point(10, 70), AutoSize = true };
            grpIcBolge.Controls.AddRange(new Control[] { rbIcMean, rbIcMedian, rbIcGauss });

            // Dış Bölge (Bulanıklaştırma) Ayarları
            grpDisBolge = new GroupBox() { Text = "Dış Bölge (Bulanıklaştırma)", Location = new Point(controlsX, startY + 110), Size = new Size(200, 100) };
            rbDisMean = new RadioButton() { Text = "Mean", Location = new Point(10, 20), AutoSize = true };
            rbDisMedian = new RadioButton() { Text = "Median", Location = new Point(10, 45), AutoSize = true };
            rbDisGauss = new RadioButton() { Text = "Gauss", Location = new Point(10, 70), AutoSize = true, Checked = true };
            grpDisBolge.Controls.AddRange(new Control[] { rbDisMean, rbDisMedian, rbDisGauss });

            // Butonlar
            int btnY = startY + 230;
            btnUygula = new Button() { Text = "Uygula", Location = new Point(controlsX, btnY), Size = new Size(200, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnTemizle = new Button() { Text = "Seçimi Temizle", Location = new Point(controlsX, btnY + 50), Size = new Size(200, 40) };
            btnTemizle.Click += new EventHandler(btnTemizle_Click);

            lblBilgi = new Label()
            {
                Text = "Resim yükleyin ve nokta koyarak bir bölge seçin.",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(575, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, grpIcBolge, grpDisBolge, btnUygula, btnTemizle, lblBilgi, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje4_BolgeselIslemForm";
            this.Size = new Size(1000, 680);
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
                ResetSelection();
            }
        }

        // Fare ile Alan Seçimi
        private void PcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;
            if (e.Button == MouseButtons.Left)
            {
                polygonPoints.Add(e.Location);
                if (polygonPoints.Count >= 3) btnUygula.Enabled = true;
                pcbResim.Invalidate();
            }
        }

        private void PcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (originalBitmap != null && pcbResim.Image == null) // İşlem yapılmamışsa orijinali çiz
            {
                // Burada kolaylık olsun diye Image özelliğini kullanıp çizgileri üzerine çizdiriyoruz.
            }
            else if (pcbResim.Image == null && originalBitmap != null)
            {
                // Resim kutuya sığacak şekilde çizilmeli
                Rectangle imgRect = GetImageRectangle();
                e.Graphics.DrawImage(originalBitmap, imgRect);
            }

            // Noktaları ve Çizgileri Çiz
            if (polygonPoints.Count > 0)
            {
                using (Pen pen = new Pen(Color.Yellow, 2))
                {
                    if (polygonPoints.Count > 1)
                        e.Graphics.DrawLines(pen, polygonPoints.ToArray());

                    // Poligonu kapatan hayali çizgi
                    if (polygonPoints.Count > 2)
                        e.Graphics.DrawLine(Pens.Gray, polygonPoints[polygonPoints.Count - 1], polygonPoints[0]);

                    foreach (Point p in polygonPoints)
                        e.Graphics.FillEllipse(Brushes.Red, p.X - 3, p.Y - 3, 6, 6);
                }
            }
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            ResetSelection();
        }

        private void ResetSelection()
        {
            if (originalBitmap != null) pcbResim.Image = originalBitmap;
            polygonPoints.Clear();
            btnUygula.Enabled = false;
            pcbResim.Invalidate();
        }

        // ANA İŞLEM METODU

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null || polygonPoints.Count < 3) return;

            this.Cursor = Cursors.WaitCursor;
            lblBilgi.Text = "İşleniyor... Lütfen bekleyin.";
            Application.DoEvents();

            // Poligon Yolunu Oluştur (Resim Koordinatlarında)
            GraphicsPath path = new GraphicsPath();
            List<Point> imgPoints = new List<Point>();
            Rectangle imgRect = GetImageRectangle(); // Resmin ekrandaki yeri
            foreach (Point p in polygonPoints)
            {
                imgPoints.Add(ConvertPointToImage(p, imgRect));
            }
            path.AddPolygon(imgPoints.ToArray());

            // Tamamen Bulanık ve Tamamen Net Resimleri Hazırla
            Bitmap blurredImage = null;
            Bitmap sharpenedImage = null;

            // Dış Bölge (Blur) Hazırlığı
            if (rbDisMean.Checked) blurredImage = ApplyMeanFilter(originalBitmap, KERNEL_SIZE);
            else if (rbDisMedian.Checked) blurredImage = ApplyMedianFilter(originalBitmap, KERNEL_SIZE);
            else blurredImage = ApplyGaussianFilter(originalBitmap, KERNEL_SIZE);

            // İç Bölge (Netleştirme) Hazırlığı
            // Netleştirme için önce bir blur kopyası lazım (Unsharp Masking için)
            Bitmap tempBlurForSharp = null;
            if (rbIcMean.Checked) tempBlurForSharp = ApplyMeanFilter(originalBitmap, KERNEL_SIZE);
            else if (rbIcMedian.Checked) tempBlurForSharp = ApplyMedianFilter(originalBitmap, KERNEL_SIZE);
            else tempBlurForSharp = ApplyGaussianFilter(originalBitmap, KERNEL_SIZE);

            sharpenedImage = ApplyUnsharpMask(originalBitmap, tempBlurForSharp, SHARPEN_AMOUNT);


            // Birleştirme (Compositing)
            Bitmap finalImage = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    if (path.IsVisible(x, y))
                    {
                        // İç Bölge -> Netleştirilmiş Piksel
                        finalImage.SetPixel(x, y, sharpenedImage.GetPixel(x, y));
                    }
                    else
                    {
                        // Dış Bölge -> Bulanık Piksel
                        finalImage.SetPixel(x, y, blurredImage.GetPixel(x, y));
                    }
                }
            }

            pcbResim.Image = finalImage;
            lblBilgi.Text = "İşlem tamamlandı.";
            this.Cursor = Cursors.Default;
        }

        // FİLTRE METOTLARI

        private Bitmap ApplyMeanFilter(Bitmap src, int size)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            int r = size / 2;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, count = 0;
                    for (int ky = -r; ky <= r; ky++)
                    {
                        for (int kx = -r; kx <= r; kx++)
                        {
                            int px = Math.Max(0, Math.Min(src.Width - 1, x + kx));
                            int py = Math.Max(0, Math.Min(src.Height - 1, y + ky));
                            Color c = src.GetPixel(px, py);
                            rSum += c.R; gSum += c.G; bSum += c.B; count++;
                        }
                    }
                    dst.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                }
            }
            return dst;
        }

        private Bitmap ApplyMedianFilter(Bitmap src, int size)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            int r = size / 2;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    List<int> R = new List<int>(), G = new List<int>(), B = new List<int>();
                    for (int ky = -r; ky <= r; ky++)
                    {
                        for (int kx = -r; kx <= r; kx++)
                        {
                            int px = Math.Max(0, Math.Min(src.Width - 1, x + kx));
                            int py = Math.Max(0, Math.Min(src.Height - 1, y + ky));
                            Color c = src.GetPixel(px, py);
                            R.Add(c.R); G.Add(c.G); B.Add(c.B);
                        }
                    }
                    R.Sort(); G.Sort(); B.Sort();
                    dst.SetPixel(x, y, Color.FromArgb(R[R.Count / 2], G[G.Count / 2], B[B.Count / 2]));
                }
            }
            return dst;
        }

        private Bitmap ApplyGaussianFilter(Bitmap src, int size)
        {
            double[,] kernel = CalculateGaussianKernel(size, size / 3.0);
            Bitmap dst = new Bitmap(src.Width, src.Height);
            int r = size / 2;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;
                    for (int ky = -r; ky <= r; ky++)
                    {
                        for (int kx = -r; kx <= r; kx++)
                        {
                            int px = Math.Max(0, Math.Min(src.Width - 1, x + kx));
                            int py = Math.Max(0, Math.Min(src.Height - 1, y + ky));
                            Color c = src.GetPixel(px, py);
                            double w = kernel[ky + r, kx + r];
                            rSum += c.R * w; gSum += c.G * w; bSum += c.B * w;
                        }
                    }
                    dst.SetPixel(x, y, Color.FromArgb(
                        Math.Min(255, (int)rSum), Math.Min(255, (int)gSum), Math.Min(255, (int)bSum)));
                }
            }
            return dst;
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
                    double val = (1 / (2 * Math.PI * sigma * sigma)) * Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    kernel[y + r, x + r] = val;
                    sum += val;
                }
            }
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) kernel[y, x] /= sum;
            return kernel;
        }

        private Bitmap ApplyUnsharpMask(Bitmap original, Bitmap blurred, double amount)
        {
            Bitmap dst = new Bitmap(original.Width, original.Height);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color cOrg = original.GetPixel(x, y);
                    Color cBlur = blurred.GetPixel(x, y);
                    int r = (int)(cOrg.R + (cOrg.R - cBlur.R) * amount);
                    int g = (int)(cOrg.G + (cOrg.G - cBlur.G) * amount);
                    int b = (int)(cOrg.B + (cOrg.B - cBlur.B) * amount);
                    dst.SetPixel(x, y, Color.FromArgb(
                        Math.Max(0, Math.Min(255, r)),
                        Math.Max(0, Math.Min(255, g)),
                        Math.Max(0, Math.Min(255, b))));
                }
            }
            return dst;
        }

        // Koordinat Dönüşümleri 

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