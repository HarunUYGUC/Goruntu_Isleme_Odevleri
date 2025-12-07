using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_FarkliMatrislerForm : Form
    {
        private PictureBox pcbOriginal, pcbResult, pcbLegend;
        private Label lblOriginal, lblResult, lblLegend;
        private Button btnYukle, btnUygula, btnGeri;

        // Matris Seçimi İçin Radio Buttonlar
        private GroupBox grpMatrisler;
        private RadioButton rbSobel, rbPrewitt, rbScharr;

        private TrackBar tbThreshold;
        private Label lblThreshold;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje4_FarkliMatrislerForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Compass Algoritması (3 Farklı Matris)";

            int pcbSize = 300;
            int margin = 20;

            // --- Sol Taraf (Orijinal) ---
            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // --- Orta Taraf (Sonuç) ---
            lblResult = new Label() { Text = "Renkli Kenar Sonucu", Location = new Point(margin + pcbSize + 20, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin + pcbSize + 20, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };

            // --- Sağ Taraf (Kontroller ve Lejant) ---
            int ctrlX = margin * 2 + pcbSize * 2 + 20;

            // Matris Seçimi Grubu
            grpMatrisler = new GroupBox() { Text = "Matris Türü Seçiniz", Location = new Point(ctrlX, margin + 25), Size = new Size(200, 120) };

            rbSobel = new RadioButton() { Text = "Sobel (Dengeli)", Location = new Point(15, 25), Checked = true, AutoSize = true };
            rbPrewitt = new RadioButton() { Text = "Prewitt (Basit)", Location = new Point(15, 55), AutoSize = true };
            rbScharr = new RadioButton() { Text = "Scharr (Yüksek Kalite)", Location = new Point(15, 85), AutoSize = true, Font = new Font("Arial", 8, FontStyle.Bold) }; // Kalite vurgusu

            grpMatrisler.Controls.AddRange(new Control[] { rbSobel, rbPrewitt, rbScharr });

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(ctrlX, margin + 160), Size = new Size(200, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblThreshold = new Label() { Text = "Eşik Değeri: 50", Location = new Point(ctrlX, margin + 210), AutoSize = true };
            tbThreshold = new TrackBar() { Location = new Point(ctrlX, margin + 230), Size = new Size(200, 45), Minimum = 0, Maximum = 200, Value = 50, TickFrequency = 10 };
            tbThreshold.Scroll += (s, e) => { lblThreshold.Text = $"Eşik Değeri: {tbThreshold.Value}"; };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(ctrlX, margin + 280), Size = new Size(200, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            // Lejant (Renk Skalası)
            lblLegend = new Label() { Text = "Açı Renkleri", Location = new Point(ctrlX, margin + 340), AutoSize = true };
            pcbLegend = new PictureBox() { Location = new Point(ctrlX, margin + 360), Size = new Size(200, 30), BorderStyle = BorderStyle.FixedSingle };
            pcbLegend.Paint += PcbLegend_Paint; // Çizimi Paint eventinde yapalım

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX, margin + 410), Size = new Size(200, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Size = new Size(ctrlX + 250, 500);

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblResult, pcbResult,
                grpMatrisler, btnYukle, lblThreshold, tbThreshold,
                btnUygula, lblLegend, pcbLegend, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_FarkliMatrislerForm";
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
                btnUygula.Enabled = true;
                pcbLegend.Invalidate(); // Lejantı çiz
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // 1. Hangi Matris Seçili?
            int[,] xKernel = null;
            int[,] yKernel = null;

            if (rbSobel.Checked)
            {
                xKernel = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
                yKernel = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            }
            else if (rbPrewitt.Checked)
            {
                // Prewitt: Sobel'e benzer ama ortadaki katsayılar 1'dir.
                xKernel = new int[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
                yKernel = new int[,] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };
            }
            else if (rbScharr.Checked)
            {
                // Scharr: Açı hesaplamada en kaliteli sonucu verir (Rotasyonel simetri).
                xKernel = new int[,] { { -3, 0, 3 }, { -10, 0, 10 }, { -3, 0, 3 } };
                yKernel = new int[,] { { -3, -10, -3 }, { 0, 0, 0 }, { 3, 10, 3 } };
            }

            // 2. İşlemi Uygula
            pcbResult.Image = ApplyCompassEdge(originalBitmap, xKernel, yKernel, tbThreshold.Value);

            this.Cursor = Cursors.Default;
        }

        private Bitmap ApplyCompassEdge(Bitmap src, int[,] gx, int[,] gy, int threshold)
        {
            int w = src.Width;
            int h = src.Height;
            Bitmap dst = new Bitmap(w, h);

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    double sumX = 0;
                    double sumY = 0;

                    // Konvolüsyon (3x3 Matris ile çarpım)
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color p = src.GetPixel(x + j, y + i);
                            // Gri tonlama
                            int gray = (int)(p.R * 0.3 + p.G * 0.59 + p.B * 0.11);

                            sumX += gray * gx[i + 1, j + 1];
                            sumY += gray * gy[i + 1, j + 1];
                        }
                    }

                    // Magnitude (Büyüklük)
                    double magnitude = Math.Sqrt(sumX * sumX + sumY * sumY);

                    // Eşik kontrolü
                    if (magnitude > threshold)
                    {
                        // Compass (Pusula/Açı) Hesabı
                        // Math.Atan2 y ve x değişiminden açıyı verir.
                        double angleRad = Math.Atan2(sumY, sumX);
                        double angleDeg = angleRad * (180.0 / Math.PI);
                        if (angleDeg < 0) angleDeg += 360;

                        // Açıyı renge çevir ve boya
                        dst.SetPixel(x, y, HsvToRgb(angleDeg, 1.0, 1.0));
                    }
                    else
                    {
                        // Kenar değilse siyah
                        dst.SetPixel(x, y, Color.Black);
                    }
                }
            }
            return dst;
        }

        // Lejant Çizimi (Paint Eventi ile)
        private void PcbLegend_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int w = pcbLegend.Width;
            int h = pcbLegend.Height;

            for (int i = 0; i < w; i++)
            {
                // Genişliğe göre 0-360 dereceyi haritala
                double angle = (double)i / w * 360.0;
                Color c = HsvToRgb(angle, 1.0, 1.0);
                using (Pen p = new Pen(c))
                {
                    g.DrawLine(p, i, 0, i, h);
                }
            }
        }

        // HSV'den RGB'ye
        private Color HsvToRgb(double h, double S, double V)
        {
            double C = V * S;
            double X = C * (1 - Math.Abs((h / 60.0) % 2 - 1));
            double m = V - C;

            double r = 0, g = 0, b = 0;

            if (h >= 0 && h < 60) { r = C; g = X; b = 0; }
            else if (h >= 60 && h < 120) { r = X; g = C; b = 0; }
            else if (h >= 120 && h < 180) { r = 0; g = C; b = X; }
            else if (h >= 180 && h < 240) { r = 0; g = X; b = C; }
            else if (h >= 240 && h < 300) { r = X; g = 0; b = C; }
            else if (h >= 300 && h <= 360) { r = C; g = 0; b = X; }

            return Color.FromArgb(
                (int)((r + m) * 255),
                (int)((g + m) * 255),
                (int)((b + m) * 255));
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