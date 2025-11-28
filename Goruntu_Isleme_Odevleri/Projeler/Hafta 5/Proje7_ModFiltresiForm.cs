using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje7_ModFiltresiForm : Form
    {
        private PictureBox pcbOriginal, pcbMode, pcbMean, pcbMedian;
        private Label lblOriginal, lblMode, lblMean, lblMedian, lblKernel;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbKernelSize;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje7_ModFiltresiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 7: En Sık Tekrar Eden Renk (Mode) Filtresi";

            int pcbSize = 300;
            int margin = 20;

            // 1. Orijinal Resim
            lblOriginal = new Label() { Text = "1. Orijinal Resim", Location = new Point(margin + 100, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // 2. Mode (Bizim Algoritmamız)
            lblMode = new Label() { Text = "2. Mod Filtresi (En Sık Geçen Renk)", Location = new Point(margin * 2 + pcbSize + 50, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Blue };
            pcbMode = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.Fixed3D, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int row2Y = margin + 25 + pcbSize + 30;

            // 3. Mean (Karşılaştırma)
            lblMean = new Label() { Text = "3. Mean (Ortalama) - Karşılaştırma", Location = new Point(margin + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMean = new PictureBox() { Location = new Point(margin, row2Y + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // 4. Median (Karşılaştırma)
            lblMedian = new Label() { Text = "4. Median (Ortanca) - Karşılaştırma", Location = new Point(margin * 2 + pcbSize + 80, row2Y), AutoSize = true, Font = new Font("Arial", 9) };
            pcbMedian = new PictureBox() { Location = new Point(margin * 2 + pcbSize, row2Y + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // Kontroller (Sağ Taraf)
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
                lblOriginal, pcbOriginal, lblMode, pcbMode,
                lblMean, pcbMean, lblMedian, pcbMedian,
                btnYukle, lblKernel, cmbKernelSize, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje7_ModFiltresiForm";
            this.Size = new Size(850, 750);
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
                pcbMode.Image = null; pcbMean.Image = null; pcbMedian.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;
            int kernelSize = int.Parse(cmbKernelSize.SelectedItem.ToString().Substring(0, 1));

            // 1. Mod Filtresi (Bizim Algoritmamız)
            pcbMode.Image = ApplyModeFilter(originalBitmap, kernelSize);

            // 2. Karşılaştırma için Mean ve Median
            pcbMean.Image = ApplyMeanFilter(originalBitmap, kernelSize);
            pcbMedian.Image = ApplyMedianFilter(originalBitmap, kernelSize);

            this.Cursor = Cursors.Default;
        }

        // --- ÖDEVİN İSTEDİĞİ ALGORİTMA: MODE (EN SIK TEKRAR EDEN RENK) ---
        private Bitmap ApplyModeFilter(Bitmap srcImage, int kernelSize)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int radius = kernelSize / 2;

            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    // Renklerin frekansını tutmak için bir sözlük (Dictionary) kullanıyoruz
                    // Anahtar: Renk, Değer: Kaç kere geçtiği
                    Dictionary<Color, int> colorFrequency = new Dictionary<Color, int>();

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = Math.Max(0, Math.Min(srcImage.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(srcImage.Height - 1, y + ky));

                            Color c = srcImage.GetPixel(pX, pY);

                            if (colorFrequency.ContainsKey(c))
                                colorFrequency[c]++;
                            else
                                colorFrequency.Add(c, 1);
                        }
                    }

                    // En çok tekrar eden rengi bul
                    // OrderByDescending ile sayıya göre sırala ve ilkini al
                    Color modeColor = colorFrequency.OrderByDescending(k => k.Value).First().Key;

                    dstImage.SetPixel(x, y, modeColor);
                }
            }
            return dstImage;
        }

        // --- Karşılaştırma Algoritmaları ---

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
                            int pX = Math.Max(0, Math.Min(srcImage.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(srcImage.Height - 1, y + ky));
                            Color c = srcImage.GetPixel(pX, pY);
                            rSum += c.R; gSum += c.G; bSum += c.B;
                            count++;
                        }
                    }
                    dstImage.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                }
            }
            return dstImage;
        }

        private Bitmap ApplyMedianFilter(Bitmap srcImage, int kernelSize)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int radius = kernelSize / 2;
            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    List<int> r = new List<int>(), g = new List<int>(), b = new List<int>();
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = Math.Max(0, Math.Min(srcImage.Width - 1, x + kx));
                            int pY = Math.Max(0, Math.Min(srcImage.Height - 1, y + ky));
                            Color c = srcImage.GetPixel(pX, pY);
                            r.Add(c.R); g.Add(c.G); b.Add(c.B);
                        }
                    }
                    r.Sort(); g.Sort(); b.Sort();
                    int mid = r.Count / 2;
                    dstImage.SetPixel(x, y, Color.FromArgb(r[mid], g[mid], b[mid]));
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