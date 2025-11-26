using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq; // Median filtresi için gerekli (sıralama yapmak için)

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_FiltrelemeKarsilastirmaForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbFiltreTipi, cmbMatrisBoyutu;
        private Label lblOriginal, lblResult, lblFiltre, lblMatris;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje1_FiltrelemeKarsilastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 1: Filtreleme Algoritmaları Karşılaştırması";

            int pcbSize = 350;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Filtrelenmiş Resim", Location = new Point(margin * 2 + pcbSize + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 20;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblFiltre = new Label() { Text = "Filtre Tipi:", Location = new Point(200, controlsY + 10), AutoSize = true };
            cmbFiltreTipi = new ComboBox() { Location = new Point(270, controlsY + 8), DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            cmbFiltreTipi.Items.AddRange(new object[] { "Mean (Ortalama)", "Median (Ortanca)", "Gauss (Bulanıklaştırma)" });
            cmbFiltreTipi.SelectedIndex = 0;

            lblMatris = new Label() { Text = "Matris Boyutu:", Location = new Point(410, controlsY + 10), AutoSize = true };
            cmbMatrisBoyutu = new ComboBox() { Location = new Point(500, controlsY + 8), DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
            cmbMatrisBoyutu.Items.AddRange(new object[] { "3x3", "5x5", "7x7", "9x9" });
            cmbMatrisBoyutu.SelectedIndex = 0;

            btnUygula = new Button() { Text = "Uygula", Location = new Point(600, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Geri", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblFiltre, cmbFiltreTipi, lblMatris, cmbMatrisBoyutu, btnUygula, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje1_FiltrelemeKarsilastirmaForm";
            this.Size = new Size(850, 500);
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
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;

            int kernelSize = int.Parse(cmbMatrisBoyutu.SelectedItem.ToString().Substring(0, 1));
            string filterType = cmbFiltreTipi.SelectedItem.ToString();

            Bitmap resultBitmap = null;

            if (filterType.Contains("Mean"))
            {
                resultBitmap = ApplyMeanFilter(originalBitmap, kernelSize);
            }
            else if (filterType.Contains("Median"))
            {
                resultBitmap = ApplyMedianFilter(originalBitmap, kernelSize);
            }
            else if (filterType.Contains("Gauss"))
            {
                resultBitmap = ApplyGaussianFilter(originalBitmap, kernelSize);
            }

            pcbResult.Image = resultBitmap;
            this.Cursor = Cursors.Default;
        }

        // Mean (Ortalama) Filtresi
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
                                rSum += p.R;
                                gSum += p.G;
                                bSum += p.B;
                                count++;
                            }
                        }
                    }
                    dstImage.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                }
            }
            return dstImage;
        }

        // Median (Ortanca) Filtresi
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
                                rValues.Add(p.R);
                                gValues.Add(p.G);
                                bValues.Add(p.B);
                            }
                        }
                    }

                    rValues.Sort();
                    gValues.Sort();
                    bValues.Sort();

                    int medianIndex = rValues.Count / 2;
                    dstImage.SetPixel(x, y, Color.FromArgb(rValues[medianIndex], gValues[medianIndex], bValues[medianIndex]));
                }
            }
            return dstImage;
        }

        // Gauss (Gaussian) Filtresi
        private Bitmap ApplyGaussianFilter(Bitmap srcImage, int kernelSize)
        {
            // Basit bir Gauss çekirdeği oluşturma (Sigma = kernelSize / 3.0 yaklaşımı)
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

            // Normalize et (Toplamı 1 yap)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
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
                            int pX = x + kx;
                            int pY = y + ky;

                            // Sınır dışı pikseller için kenar pikselini tekrarla (Padding)
                            if (pX < 0) pX = 0;
                            else if (pX >= srcImage.Width) pX = srcImage.Width - 1;

                            if (pY < 0) pY = 0;
                            else if (pY >= srcImage.Height) pY = srcImage.Height - 1;

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