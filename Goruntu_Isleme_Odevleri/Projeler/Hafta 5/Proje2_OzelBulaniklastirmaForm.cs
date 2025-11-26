using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_OzelBulaniklastirmaForm : Form
    {
        private PictureBox pcbOriginal, pcbMean, pcbMedian, pcbGauss, pcbCustom;
        private Label lblOriginal, lblMean, lblMedian, lblGauss, lblCustom;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbKernelSize;
        private Label lblKernel;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje2_OzelBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Özel Bulanıklaştırma Algoritması";

            int pcbSize = 250;
            int margin = 15;
            int labelOffset = 20;

            // 1. Satır
            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 80, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblCustom = new Label() { Text = "Özel Algoritma (Conservative Smoothing)", Location = new Point(margin * 2 + pcbSize + 10, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Blue };
            pcbCustom = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.Fixed3D, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // 2. Satır
            int row2Y = margin + labelOffset + pcbSize + 30;

            lblMean = new Label() { Text = "Mean (Ortalama)", Location = new Point(margin + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMean = new PictureBox() { Location = new Point(margin, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblMedian = new Label() { Text = "Median (Ortanca)", Location = new Point(margin * 2 + pcbSize + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMedian = new PictureBox() { Location = new Point(margin * 2 + pcbSize, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblGauss = new Label() { Text = "Gauss (Gaussian)", Location = new Point(margin * 3 + pcbSize * 2 + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbGauss = new PictureBox() { Location = new Point(margin * 3 + pcbSize * 2, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // Kontroller (Sağ Üst Köşe)
            int controlsX = margin * 3 + pcbSize * 2;
            int controlsY = margin + 50;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(controlsX, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblKernel = new Label() { Text = "Matris Boyutu:", Location = new Point(controlsX, controlsY + 60), AutoSize = true };
            cmbKernelSize = new ComboBox() { Location = new Point(controlsX + 90, controlsY + 58), DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 };
            cmbKernelSize.Items.AddRange(new object[] { "3x3", "5x5", "7x7", "9x9" });
            cmbKernelSize.SelectedIndex = 0;

            btnUygula = new Button() { Text = "Filtreleri Uygula", Location = new Point(controlsX, controlsY + 100), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(controlsX, controlsY + 160), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblCustom, pcbCustom,
                lblMean, pcbMean, lblMedian, pcbMedian, lblGauss, pcbGauss,
                btnYukle, lblKernel, cmbKernelSize, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_OzelBulaniklastirmaForm";
            this.Size = new Size(850, 700);
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
                // Diğer kutuları temizle
                pcbMean.Image = null; pcbMedian.Image = null; pcbGauss.Image = null; pcbCustom.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;
            int kernelSize = int.Parse(cmbKernelSize.SelectedItem.ToString().Substring(0, 1));

            // Özel Algoritma (Conservative Smoothing)
            pcbCustom.Image = ApplyConservativeSmoothing(originalBitmap, kernelSize);

            // Mean (Ortalama)
            pcbMean.Image = ApplyMeanFilter(originalBitmap, kernelSize);

            // Median (Ortanca)
            pcbMedian.Image = ApplyMedianFilter(originalBitmap, kernelSize);

            // Gauss
            pcbGauss.Image = ApplyGaussianFilter(originalBitmap, kernelSize);

            this.Cursor = Cursors.Default;
        }

        // ÖZEL ALGORİTMA: Conservative Smoothing
        private Bitmap ApplyConservativeSmoothing(Bitmap srcImage, int kernelSize)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int radius = kernelSize / 2;

            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    List<int> rValues = new List<int>();
                    List<int> gValues = new List<int>();
                    List<int> bValues = new List<int>();
                    Color centerPixel = srcImage.GetPixel(x, y);

                    // Komşuları topla (Merkez piksel HARİÇ)
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            if (kx == 0 && ky == 0) continue; // Merkeze bakma

                            int pX = x + kx;
                            int pY = y + ky;

                            if (pX >= 0 && pX < srcImage.Width && pY >= 0 && pY < srcImage.Height)
                            {
                                Color p = srcImage.GetPixel(pX, pY);
                                rValues.Add(p.R);
                                gValues.Add(p.G);
                                bValues.Add(p.B);
                            }
                        }
                    }

                    if (rValues.Count > 0)
                    {
                        // Her kanal için min ve max değerleri bul
                        int rMin = rValues.Min(); int rMax = rValues.Max();
                        int gMin = gValues.Min(); int gMax = gValues.Max();
                        int bMin = bValues.Min(); int bMax = bValues.Max();

                        // Merkez pikseli sınırla (Clamp)
                        int newR = Math.Max(rMin, Math.Min(rMax, centerPixel.R));
                        int newG = Math.Max(gMin, Math.Min(gMax, centerPixel.G));
                        int newB = Math.Max(bMin, Math.Min(bMax, centerPixel.B));

                        dstImage.SetPixel(x, y, Color.FromArgb(newR, newG, newB));
                    }
                    else
                    {
                        dstImage.SetPixel(x, y, centerPixel);
                    }
                }
            }
            return dstImage;
        }

        // DİĞER ALGORİTMALAR
        // Mean
        private Bitmap ApplyMeanFilter(Bitmap srcImage, int kernelSize)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int radius = kernelSize / 2;

            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, count = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = x + kx;
                            int pY = y + ky;

                            if (pX >= 0 && pX < srcImage.Width && pY >= 0 && pY < srcImage.Height)
                            {
                                Color p = srcImage.GetPixel(pX, pY);
                                rSum += p.R; gSum += p.G; bSum += p.B;
                                count++;
                            }
                        }
                    }
                    dstImage.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                }
            }
            return dstImage;
        }

        // Median
        private Bitmap ApplyMedianFilter(Bitmap srcImage, int kernelSize)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int radius = kernelSize / 2;

            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    List<int> rValues = new List<int>();
                    List<int> gValues = new List<int>();
                    List<int> bValues = new List<int>();

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = x + kx;
                            int pY = y + ky;

                            if (pX >= 0 && pX < srcImage.Width && pY >= 0 && pY < srcImage.Height)
                            {
                                Color p = srcImage.GetPixel(pX, pY);
                                rValues.Add(p.R); gValues.Add(p.G); bValues.Add(p.B);
                            }
                        }
                    }
                    rValues.Sort(); gValues.Sort(); bValues.Sort();
                    int medianIndex = rValues.Count / 2;
                    dstImage.SetPixel(x, y, Color.FromArgb(rValues[medianIndex], gValues[medianIndex], bValues[medianIndex]));
                }
            }
            return dstImage;
        }

        // Gaussian
        private Bitmap ApplyGaussianFilter(Bitmap srcImage, int kernelSize)
        {
            double[,] kernel = CalculateGaussianKernel(kernelSize, kernelSize / 3.0);
            return ApplyConvolution(srcImage, kernel);
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
                    double value = (1.0 / (2.0 * Math.PI * sigma * sigma)) * Math.Exp(-(x * x + y * y) / (2.0 * sigma * sigma));
                    kernel[y + radius, x + radius] = value;
                    sum += value;
                }
            }
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    kernel[y, x] /= sum;
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
                            rSum += p.R * weight; gSum += p.G * weight; bSum += p.B * weight;
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