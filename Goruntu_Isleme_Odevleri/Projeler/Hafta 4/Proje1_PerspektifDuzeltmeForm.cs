using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_PerspektifDuzeltmeForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnDuzelt, btnAliasDuzelt, btnGeri, btnTemizle;
        private Label lblOriginal, lblResult, lblInfo;
        private Panel pnlResult;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap transformedBitmap;
        private List<Point> sourcePoints = new List<Point>(); // Seçilen 4 nokta

        // Matris Katsayıları (a, b, c, d, e, f, g, h) - 8 parametreli
        private double[] matrixParams;

        public Proje1_PerspektifDuzeltmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 1: Perspektif Düzeltme ve Alias Giderme";

            lblOriginal = new Label() { Text = "1. Orijinal Resim (4 Köşe Seçin)", Location = new Point(25, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox()
            {
                Location = new Point(25, 45),
                Size = new Size(500, 400),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };
            pcbOriginal.MouseClick += PcbOriginal_MouseClick;
            pcbOriginal.Paint += PcbOriginal_Paint;

            lblResult = new Label() { Text = "2. Sonuç (Perspektif Düzeltilmiş)", Location = new Point(550, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            pnlResult = new Panel()
            {
                Location = new Point(550, 45),
                Size = new Size(500, 400),
                AutoScroll = true, // Resim büyükse kaydırma çubuğu çıkar
                BorderStyle = BorderStyle.Fixed3D
            };

            pcbResult = new PictureBox()
            {
                Location = new Point(0, 0), // Panel içinde 0,0
                SizeMode = PictureBoxSizeMode.AutoSize 
            };
            pnlResult.Controls.Add(pcbResult);

            lblInfo = new Label() { Text = "Sırasıyla: Sol-Üst, Sağ-Üst, Sağ-Alt, Sol-Alt", Location = new Point(25, 450), AutoSize = true, ForeColor = Color.Blue };

            int btnY = 480;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, btnY), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnTemizle = new Button() { Text = "Noktaları Sıfırla", Location = new Point(155, btnY), Size = new Size(120, 40) };
            btnTemizle.Click += (s, e) => { sourcePoints.Clear(); pcbOriginal.Invalidate(); lblInfo.Text = "Noktalar temizlendi."; btnDuzelt.Enabled = false; };

            btnDuzelt = new Button() { Text = "Perspektifi Düzelt", Location = new Point(285, btnY), Size = new Size(150, 40), BackColor = Color.LightBlue, Enabled = false };
            btnDuzelt.Click += new EventHandler(btnDuzelt_Click);

            btnAliasDuzelt = new Button() { Text = "Alias (Boşluk) Gider", Location = new Point(445, btnY), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnAliasDuzelt.Click += new EventHandler(btnAliasDuzelt_Click);

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(900, btnY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pnlResult, lblInfo, btnYukle, btnTemizle, btnDuzelt, btnAliasDuzelt, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje1_PerspektifDuzeltmeForm";
            this.Size = new Size(1100, 600);
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
                pcbOriginal.Image = originalBitmap;

                sourcePoints.Clear();
                pcbResult.Image = null;
                btnDuzelt.Enabled = false;
                btnAliasDuzelt.Enabled = false;
                lblInfo.Text = "Lütfen sırasıyla 4 köşe seçin: Sol-Üst, Sağ-Üst, Sağ-Alt, Sol-Alt";
            }
        }

        private void PcbOriginal_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || sourcePoints.Count >= 4) return;

            // PictureBox Zoom modunda olduğu için, tıklanan ekran koordinatını
            // gerçek resim koordinatına çevirmemiz gerekir.
            Point imgPoint = ConvertPointToImage(e.Location);
            sourcePoints.Add(imgPoint);
            pcbOriginal.Invalidate(); // Paint olayını tetikle

            if (sourcePoints.Count == 4)
            {
                lblInfo.Text = "4 nokta seçildi. 'Perspektifi Düzelt' butonuna basın.";
                btnDuzelt.Enabled = true;
            }
            else
            {
                lblInfo.Text = $"{sourcePoints.Count}. nokta seçildi...";
            }
        }

        private void PcbOriginal_Paint(object sender, PaintEventArgs e)
        {
            if (sourcePoints.Count > 0)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                using (Brush brush = new SolidBrush(Color.Yellow))
                {
                    for (int i = 0; i < sourcePoints.Count; i++)
                    {
                        // Gerçek resim koordinatını tekrar ekran koordinatına çevirip çiz
                        Point p = ConvertPointFromImage(sourcePoints[i]);
                        e.Graphics.FillEllipse(brush, p.X - 5, p.Y - 5, 10, 10);
                        e.Graphics.DrawString((i + 1).ToString(), new Font("Arial", 12), Brushes.Red, p.X + 5, p.Y + 5);
                    }

                    // Noktaları birleştiren dörtgeni çiz
                    if (sourcePoints.Count == 4)
                    {
                        Point[] pts = new Point[4];
                        for (int i = 0; i < 4; i++) pts[i] = ConvertPointFromImage(sourcePoints[i]);
                        e.Graphics.DrawPolygon(pen, pts);
                    }
                }
            }
        }

        // PERSPEKTİF DÜZELTME (FORWARD MAPPING)
        private void btnDuzelt_Click(object sender, EventArgs e)
        {
            if (sourcePoints.Count != 4) return;

            // Hedef boyutları (En geniş kenara göre) hesapla
            double w1 = Distance(sourcePoints[0], sourcePoints[1]); // Üst
            double w2 = Distance(sourcePoints[2], sourcePoints[3]); // Alt
            double h1 = Distance(sourcePoints[0], sourcePoints[3]); // Sol
            double h2 = Distance(sourcePoints[1], sourcePoints[2]); // Sağ

            int targetWidth = (int)Math.Max(w1, w2);
            int targetHeight = (int)Math.Max(h1, h2);

            // Hedef Noktalar (Düzeltilmiş Dikdörtgen Koordinatları)
            double[,] targetPoints = {
                { 0, 0 },                       // Sol-Üst
                { targetWidth, 0 },             // Sağ-Üst
                { 0, targetHeight },            // Sol-Alt
                { targetWidth, targetHeight }   // Sağ-Alt
            };

            double[,] correctedTargetPoints = {
                 { 0, 0 },                      
                 { targetWidth, 0 },            
                 { targetWidth, targetHeight }, 
                 { 0, targetHeight }           
            };


            // Matris Katsayılarını Hesapla (Gauss-Jordan ile 8x8 sistem)
            matrixParams = CalculatePerspectiveMatrix(sourcePoints, correctedTargetPoints);

            // Forward Mapping ile Dönüştür
            transformedBitmap = new Bitmap(targetWidth, targetHeight);

            // Arka planı SİYAH yap (Böylece alias/boşluk noktaları görünür olur)
            using (Graphics g = Graphics.FromImage(transformedBitmap)) g.Clear(Color.Black);

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    // Perspektif Formülü:
                    // X = (ax + by + c) / (gx + hy + 1)
                    // Y = (dx + ey + f) / (gx + hy + 1)

                    double denominator = matrixParams[6] * x + matrixParams[7] * y + 1.0;

                    if (Math.Abs(denominator) > 0.00001)
                    {
                        double X_new = (matrixParams[0] * x + matrixParams[1] * y + matrixParams[2]) / denominator;
                        double Y_new = (matrixParams[3] * x + matrixParams[4] * y + matrixParams[5]) / denominator;

                        int targetX = (int)Math.Round(X_new);
                        int targetY = (int)Math.Round(Y_new);

                        // Eğer hesaplanan nokta hedef resim sınırları içindeyse boya
                        if (targetX >= 0 && targetX < targetWidth && targetY >= 0 && targetY < targetHeight)
                        {
                            transformedBitmap.SetPixel(targetX, targetY, originalBitmap.GetPixel(x, y));
                        }
                    }
                }
            }

            pcbResult.Image = transformedBitmap;
            btnAliasDuzelt.Enabled = true;
            lblInfo.Text = "Perspektif düzeltildi. Siyah noktalar (Alias) oluştu. Düzeltmek için butona basın.";
        }

        // ALIAS (BOŞLUK) DOLDURMA
        private void btnAliasDuzelt_Click(object sender, EventArgs e)
        {
            if (transformedBitmap == null) return;

            Bitmap fixedBitmap = new Bitmap(transformedBitmap);
            int w = fixedBitmap.Width;
            int h = fixedBitmap.Height;

            // Siyah (Boş) Noktaları Tespit Et ve Doldur
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    Color c = fixedBitmap.GetPixel(x, y);

                    if (c.R == 0 && c.G == 0 && c.B == 0)
                    {
                        int rSum = 0, gSum = 0, bSum = 0, count = 0;

                        // 3x3 Komşuluk (Etraftaki 8 piksel)
                        for (int ky = -1; ky <= 1; ky++)
                        {
                            for (int kx = -1; kx <= 1; kx++)
                            {
                                if (kx == 0 && ky == 0) continue;

                                Color neighbor = transformedBitmap.GetPixel(x + kx, y + ky);

                                if (!(neighbor.R == 0 && neighbor.G == 0 && neighbor.B == 0))
                                {
                                    rSum += neighbor.R;
                                    gSum += neighbor.G;
                                    bSum += neighbor.B;
                                    count++;
                                }
                            }
                        }

                        if (count > 0)
                        {
                            fixedBitmap.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                        }
                    }
                }
            }
            pcbResult.Image = fixedBitmap;
            lblInfo.Text = "Alias (boşluk) düzeltme tamamlandı.";
        }

        private double[] CalculatePerspectiveMatrix(List<Point> src, double[,] dst)
        {
            double[,] A = new double[8, 8];
            double[] B = new double[8];

            for (int i = 0; i < 4; i++)
            {
                double x = src[i].X;
                double y = src[i].Y;
                double X = dst[i, 0];
                double Y = dst[i, 1];

                // Formül türetimi:
                // X(gx + hy + 1) = ax + by + c  =>  ax + by + c - g(xX) - h(yX) = X
                // Y(gx + hy + 1) = dx + ey + f  =>  dx + ey + f - g(xY) - h(yY) = Y

                // X denklemi için satır (2*i)
                A[2 * i, 0] = x;
                A[2 * i, 1] = y;
                A[2 * i, 2] = 1;
                A[2 * i, 3] = 0;
                A[2 * i, 4] = 0;
                A[2 * i, 5] = 0;
                A[2 * i, 6] = -x * X;
                A[2 * i, 7] = -y * X;
                B[2 * i] = X;

                // Y denklemi için satır (2*i + 1)
                A[2 * i + 1, 0] = 0;
                A[2 * i + 1, 1] = 0;
                A[2 * i + 1, 2] = 0;
                A[2 * i + 1, 3] = x;
                A[2 * i + 1, 4] = y;
                A[2 * i + 1, 5] = 1;
                A[2 * i + 1, 6] = -x * Y;
                A[2 * i + 1, 7] = -y * Y;
                B[2 * i + 1] = Y;
            }

            return SolveGaussian(A, B);
        }

        private double[] SolveGaussian(double[,] Matrix, double[] Result)
        {
            int n = 8;
            for (int i = 0; i < n; i++)
            {
                // Pivotlama
                double maxEl = Math.Abs(Matrix[i, i]);
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(Matrix[k, i]) > maxEl)
                    {
                        maxEl = Math.Abs(Matrix[k, i]);
                        maxRow = k;
                    }
                }

                // Satır değiştirme
                for (int k = i; k < n; k++) { double tmp = Matrix[maxRow, k]; Matrix[maxRow, k] = Matrix[i, k]; Matrix[i, k] = tmp; }
                { double tmp = Result[maxRow]; Result[maxRow] = Result[i]; Result[i] = tmp; }

                // Eliminasyon
                for (int k = i + 1; k < n; k++)
                {
                    double c = -Matrix[k, i] / Matrix[i, i];
                    for (int j = i; j < n; j++)
                    {
                        if (i == j) Matrix[k, j] = 0;
                        else Matrix[k, j] += c * Matrix[i, j];
                    }
                    Result[k] += c * Result[i];
                }
            }

            // Geriye doğru yerine koyma
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++) sum += Matrix[i, j] * x[j];
                x[i] = (Result[i] - sum) / Matrix[i, i];
            }
            return x;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        // KOORDİNAT DÖNÜŞÜMLERİ (ZOOM MODU İÇİN)
        private Point ConvertPointToImage(Point pcbPoint)
        {
            if (pcbOriginal.Image == null) return pcbPoint;
            Size pcbSize = pcbOriginal.ClientSize;
            Size imgSize = pcbOriginal.Image.Size;
            float pcbAspect = (float)pcbSize.Width / pcbSize.Height;
            float imgAspect = (float)imgSize.Width / imgSize.Height;

            if (pcbAspect > imgAspect)
            {
                float scale = (float)pcbSize.Height / imgSize.Height;
                float offsetX = (pcbSize.Width - imgSize.Width * scale) / 2;
                return new Point((int)((pcbPoint.X - offsetX) / scale), (int)(pcbPoint.Y / scale));
            }
            else
            {
                float scale = (float)pcbSize.Width / imgSize.Width;
                float offsetY = (pcbSize.Height - imgSize.Height * scale) / 2;
                return new Point((int)(pcbPoint.X / scale), (int)((pcbPoint.Y - offsetY) / scale));
            }
        }

        private Point ConvertPointFromImage(Point imgPoint)
        {
            if (pcbOriginal.Image == null) return imgPoint;
            Size pcbSize = pcbOriginal.ClientSize;
            Size imgSize = pcbOriginal.Image.Size;
            float pcbAspect = (float)pcbSize.Width / pcbSize.Height;
            float imgAspect = (float)imgSize.Width / imgSize.Height;

            if (pcbAspect > imgAspect)
            {
                float scale = (float)pcbSize.Height / imgSize.Height;
                float offsetX = (pcbSize.Width - imgSize.Width * scale) / 2;
                return new Point((int)(imgPoint.X * scale + offsetX), (int)(imgPoint.Y * scale));
            }
            else
            {
                float scale = (float)pcbSize.Width / imgSize.Width;
                float offsetY = (pcbSize.Height - imgSize.Height * scale) / 2;
                return new Point((int)(imgPoint.X * scale), (int)(imgPoint.Y * scale + offsetY));
            }
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