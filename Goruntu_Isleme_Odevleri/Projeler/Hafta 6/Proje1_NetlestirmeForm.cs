using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_NetlestirmeForm : Form
    {
        private PictureBox pcbOriginal, pcbMeanSharp, pcbMedianSharp, pcbGaussSharp;
        private Label lblOriginal, lblMean, lblMedian, lblGauss;
        private Button btnYukle, btnUygula, btnGeri;
        private TrackBar tbAmount; // Keskinlik miktarı
        private Label lblAmount;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje1_NetlestirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 1: Netleştirme Yöntemleri (Unsharp Masking)";

            int pcbSize = 250;
            int margin = 15;
            int labelOffset = 20;

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 80, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblMean = new Label() { Text = "Mean ile Netleştirme", Location = new Point(margin * 2 + pcbSize + 60, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbMeanSharp = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int row2Y = margin + labelOffset + pcbSize + 30;

            lblMedian = new Label() { Text = "Median ile Netleştirme", Location = new Point(margin + 60, row2Y), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbMedianSharp = new PictureBox() { Location = new Point(margin, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblGauss = new Label() { Text = "Gauss ile Netleştirme", Location = new Point(margin * 2 + pcbSize + 60, row2Y), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbGaussSharp = new PictureBox() { Location = new Point(margin * 2 + pcbSize, row2Y + labelOffset), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsX = margin * 3 + pcbSize * 2;
            int controlsY = margin + 50;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(controlsX, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblAmount = new Label() { Text = "Keskinlik Miktarı: 1.0", Location = new Point(controlsX, controlsY + 60), AutoSize = true };
            tbAmount = new TrackBar() { Location = new Point(controlsX, controlsY + 80), Size = new Size(150, 45), Minimum = 1, Maximum = 50, Value = 10, TickFrequency = 5 };
            tbAmount.Scroll += (s, e) => { lblAmount.Text = $"Keskinlik Miktarı: {tbAmount.Value / 10.0:0.0}"; };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(controlsX, controlsY + 130), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(controlsX, controlsY + 190), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblMean, pcbMeanSharp, lblMedian, pcbMedianSharp, lblGauss, pcbGaussSharp,
                btnYukle, lblAmount, tbAmount, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje1_NetlestirmeForm";
            this.Size = new Size(750, 650);
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
                pcbMeanSharp.Image = null;
                pcbMedianSharp.Image = null;
                pcbGaussSharp.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;
            double amount = tbAmount.Value / 10.0;

            // Mean ile Netleştirme
            pcbMeanSharp.Image = SharpenImage(originalBitmap, ApplyMeanFilter(originalBitmap), amount);

            // Median ile Netleştirme
            pcbMedianSharp.Image = SharpenImage(originalBitmap, ApplyMedianFilter(originalBitmap), amount);

            // Gauss ile Netleştirme
            pcbGaussSharp.Image = SharpenImage(originalBitmap, ApplyGaussianFilter(originalBitmap), amount);

            this.Cursor = Cursors.Default;
        }

        // Netleştirme (Unsharp Masking) Formülü
        private Bitmap SharpenImage(Bitmap original, Bitmap blurred, double amount)
        {
            Bitmap result = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color cOrg = original.GetPixel(x, y);
                    Color cBlur = blurred.GetPixel(x, y);

                    // Formül: Orijinal + (Orijinal - Bulanık) * Amount
                    int r = (int)(cOrg.R + (cOrg.R - cBlur.R) * amount);
                    int g = (int)(cOrg.G + (cOrg.G - cBlur.G) * amount);
                    int b = (int)(cOrg.B + (cOrg.B - cBlur.B) * amount);

                    // Sınırla (Clamp)
                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return result;
        }

        // BULANIKLAŞTIRMA ALGORİTMALARI

        private Bitmap ApplyMeanFilter(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            int r = 1; // Kernel size 3x3
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, count = 0;
                    for (int ky = -r; ky <= r; ky++)
                    {
                        for (int kx = -r; kx <= r; kx++)
                        {
                            int pX = Math.Max(0, Math.Min(src.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(src.Height - 1, y + ky));
                            Color p = src.GetPixel(pX, pY);
                            rSum += p.R; gSum += p.G; bSum += p.B; count++;
                        }
                    }
                    dst.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                }
            }
            return dst;
        }

        private Bitmap ApplyMedianFilter(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            int r = 1;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    List<int> rList = new List<int>(), gList = new List<int>(), bList = new List<int>();
                    for (int ky = -r; ky <= r; ky++)
                    {
                        for (int kx = -r; kx <= r; kx++)
                        {
                            int pX = Math.Max(0, Math.Min(src.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(src.Height - 1, y + ky));
                            Color p = src.GetPixel(pX, pY);
                            rList.Add(p.R); gList.Add(p.G); bList.Add(p.B);
                        }
                    }
                    rList.Sort(); gList.Sort(); bList.Sort();
                    dst.SetPixel(x, y, Color.FromArgb(rList[4], gList[4], bList[4]));
                }
            }
            return dst;
        }

        private Bitmap ApplyGaussianFilter(Bitmap src)
        {
            double[,] kernel = { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } }; // Basit 3x3 Gauss
            double kernelSum = 16;
            Bitmap dst = new Bitmap(src.Width, src.Height);
            int r = 1;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;
                    for (int ky = -r; ky <= r; ky++)
                    {
                        for (int kx = -r; kx <= r; kx++)
                        {
                            int pX = Math.Max(0, Math.Min(src.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(src.Height - 1, y + ky));
                            Color p = src.GetPixel(pX, pY);
                            double w = kernel[ky + 1, kx + 1];
                            rSum += p.R * w; gSum += p.G * w; bSum += p.B * w;
                        }
                    }
                    dst.SetPixel(x, y, Color.FromArgb(
                        Math.Min(255, (int)(rSum / kernelSum)),
                        Math.Min(255, (int)(gSum / kernelSum)),
                        Math.Min(255, (int)(bSum / kernelSum))));
                }
            }
            return dst;
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