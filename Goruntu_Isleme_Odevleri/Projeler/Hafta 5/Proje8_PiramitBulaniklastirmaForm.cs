using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje8_PiramitBulaniklastirmaForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblOriginal, lblResult, lblKernelSize, lblStep, lblKernelInfo;
        private NumericUpDown nudKernelSize, nudStep;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje8_PiramitBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 8: Piramit (Lineer) Bulanıklaştırma";

            int pcbSize = 350;
            int margin = 25;

            // Resim Kutuları
            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Piramit Filtresi Sonucu", Location = new Point(margin * 2 + pcbSize + 80, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 20;

            // Kontroller
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblKernelSize = new Label() { Text = "Matris Boyutu (Tek Sayı):", Location = new Point(200, controlsY + 12), AutoSize = true };
            nudKernelSize = new NumericUpDown()
            {
                Location = new Point(340, controlsY + 10),
                Width = 60,
                Minimum = 3,
                Maximum = 51,
                Value = 7,
                Increment = 2
            };

            lblStep = new Label() { Text = "Artış Adımı:", Location = new Point(420, controlsY + 12), AutoSize = true };
            nudStep = new NumericUpDown()
            {
                Location = new Point(500, controlsY + 10),
                Width = 60,
                Minimum = 1,
                Maximum = 100,
                Value = 2
            };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(580, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            lblKernelInfo = new Label() { Text = "Kernel Önizleme: (Hesaplanıyor...)", Location = new Point(200, controlsY + 50), AutoSize = true, Font = new Font("Consolas", 9), ForeColor = Color.DarkBlue };

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblKernelSize, nudKernelSize, lblStep, nudStep, btnUygula, lblKernelInfo, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje8_PiramitBulaniklastirmaForm";
            this.Size = new Size(850, 600); // Form boyutu
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
                pcbResult.Image = null;
                btnUygula.Enabled = true;
                ShowKernelPreview(); // Kerneli göster
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;

            int size = (int)nudKernelSize.Value;
            if (size % 2 == 0) size++; // Tek sayı garantisi
            int step = (int)nudStep.Value;

            ShowKernelPreview(); // Güncel kerneli göster

            // Piramit Kerneli Oluştur
            double[,] kernel = CreatePyramidKernel(size, step);

            // Konvolüsyon Uygula
            pcbResult.Image = ApplyConvolution(originalBitmap, kernel);

            this.Cursor = Cursors.Default;
        }

        private void ShowKernelPreview()
        {
            int size = (int)nudKernelSize.Value;
            if (size % 2 == 0) size++;
            int step = (int)nudStep.Value;

            // Sadece göstermek için küçük bir string oluştur
            if (size > 9)
            {
                lblKernelInfo.Text = $"Kernel çok büyük ({size}x{size}). Önizleme gösterilemiyor.";
                return;
            }

            double[,] kernel = CreatePyramidKernel(size, step, false); // Normalize etmeden ham değerleri al

            string preview = "Kernel (Merkez):\n";
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    preview += ((int)kernel[y, x]).ToString("00") + " ";
                }
                preview += "\n";
            }
            lblKernelInfo.Text = preview;
        }

        // Piramit Kernel Oluşturma Metodu
        private double[,] CreatePyramidKernel(int size, int step, bool normalize = true)
        {
            double[,] kernel = new double[size, size];
            double sum = 0;
            int radius = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Merkeze olan uzaklık (Chebyshev Distance - Satranç tahtası uzaklığı)
                    // Bu uzaklık kare halkalar oluşturur.
                    int distX = Math.Abs(x - radius);
                    int distY = Math.Abs(y - radius);
                    int dist = Math.Max(distX, distY);

                    // Piramit Formülü:
                    // En dış halka (dist = radius) -> Değer = 1
                    // İçeri gittikçe (dist azalır) -> Değer artar
                    // Değer = 1 + (ToplamHalkaSayısı - MevcutHalka) * Adım
                    // ToplamHalkaSayısı aslında radius'a eşittir.
                    // Örn: Radius 3. Dist 3 (Dış) -> (3-3)*step + 1 = 1.
                    //      Dist 0 (Merkez) -> (3-0)*step + 1 = 3*step + 1.

                    double value = 1 + (radius - dist) * step;

                    kernel[y, x] = value;
                    sum += value;
                }
            }

            // Normalize et (Matris toplamını 1 yap)
            if (normalize)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        kernel[y, x] /= sum;
                    }
                }
            }
            return kernel;
        }

        private Bitmap ApplyConvolution(Bitmap srcImage, double[,] kernel)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = Math.Max(0, Math.Min(srcImage.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(srcImage.Height - 1, y + ky));

                            Color p = srcImage.GetPixel(pX, pY);
                            double weight = kernel[ky + radius, kx + radius];

                            rSum += p.R * weight;
                            gSum += p.G * weight;
                            bSum += p.B * weight;
                        }
                    }

                    int r = Math.Max(0, Math.Min(255, (int)rSum));
                    int g = Math.Max(0, Math.Min(255, (int)gSum));
                    int b = Math.Max(0, Math.Min(255, (int)bSum));

                    dstImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return dstImage;
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