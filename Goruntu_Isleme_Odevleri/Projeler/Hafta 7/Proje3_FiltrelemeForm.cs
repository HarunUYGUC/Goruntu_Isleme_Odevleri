using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_FiltrelemeForm : Form
    {
        // UI Elemanları
        private PictureBox pcbKaynak, pcbSonuc;
        private Label lblKaynak, lblSonuc, lblIslem;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbFiltreler;
        private CheckBox chkZincir; // Üst üste işlem yapma özelliği

        private Form haftaFormu;
        private Bitmap originalBitmap;

        public Proje3_FiltrelemeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Gelişmiş Filtreleme ve Kenar Bulma";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();

            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void InitializeComponent() { this.Name = "Proje3_FiltrelemeForm"; }

        private void SetupUI()
        {
            int margin = 20;
            int pcbSize = 400;

            // --- Sol Taraf (Kaynak) ---
            lblKaynak = new Label() { Text = "Kaynak Görüntü", Location = new Point(margin, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbKaynak = new PictureBox() { Location = new Point(margin, 40), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(50, 50, 50) };

            // --- Sağ Taraf (Sonuç) ---
            lblSonuc = new Label() { Text = "İşlenmiş Görüntü", Location = new Point(margin + pcbSize + 20, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbSonuc = new PictureBox() { Location = new Point(margin + pcbSize + 20, 40), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(50, 50, 50) };

            // --- Alt Kontroller ---
            int ctrlY = 460;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, ctrlY), Size = new Size(120, 40), BackColor = Color.LightBlue };
            btnYukle.Click += BtnYukle_Click;

            lblIslem = new Label() { Text = "Filtre Seçimi:", Location = new Point(150, ctrlY + 12), AutoSize = true };

            cmbFiltreler = new ComboBox() { Location = new Point(240, ctrlY + 10), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFiltreler.Items.Add("Mean Filtresi (Bulanıklaştırma)");
            cmbFiltreler.Items.Add("Gauss Filtresi (Yumuşatma)");
            cmbFiltreler.Items.Add("Median Filtresi (Gürültü Giderme)");
            cmbFiltreler.Items.Add("Netleştirme (Unsharp Masking)");
            cmbFiltreler.Items.Add("Keskinleştirme Matrisi");
            cmbFiltreler.Items.Add("Sobel Kenar Bulma");
            cmbFiltreler.Items.Add("Prewitt Kenar Bulma");
            cmbFiltreler.Items.Add("Roberts Cross Kenar Bulma");
            cmbFiltreler.Items.Add("Canny (Simüle Edilmiş)");
            cmbFiltreler.SelectedIndex = 0;

            chkZincir = new CheckBox() { Text = "Zincirleme İşlem (Sonuç üzerinden devam et)", Location = new Point(500, ctrlY + 10), AutoSize = true, ForeColor = Color.DarkRed };

            btnUygula = new Button() { Text = "UYGULA", Location = new Point(760, ctrlY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false, Font = new Font("Arial", 9, FontStyle.Bold) };
            btnUygula.Click += BtnUygula_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(760, ctrlY + 50), Size = new Size(120, 30), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { pcbKaynak, pcbSonuc, lblKaynak, lblSonuc, btnYukle, lblIslem, cmbFiltreler, chkZincir, btnUygula, btnGeri });
        }

        // --- OLAYLAR ---

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(ofd.FileName);
                pcbKaynak.Image = originalBitmap;
                pcbSonuc.Image = null;
                btnUygula.Enabled = true;
            }
        }

        private void BtnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // Zincirleme işlem kontrolü: Eğer seçiliyse ve sonuçta resim varsa onu kaynak al
            Bitmap sourceMap;
            if (chkZincir.Checked && pcbSonuc.Image != null)
                sourceMap = new Bitmap(pcbSonuc.Image);
            else
                sourceMap = new Bitmap(originalBitmap);

            Bitmap result = null;

            switch (cmbFiltreler.SelectedIndex)
            {
                case 0: result = ApplyMeanFilter(sourceMap); break;
                case 1: result = ApplyGaussFilter(sourceMap); break;
                case 2: result = ApplyMedianFilter(sourceMap); break;
                case 3: result = ApplyUnsharpMasking(sourceMap); break;
                case 4: result = ApplySharpenMatrix(sourceMap); break;
                case 5: result = ApplyConvolution(sourceMap, "Sobel"); break;
                case 6: result = ApplyConvolution(sourceMap, "Prewitt"); break;
                case 7: result = ApplyRobertsCross(sourceMap); break;
                case 8: result = ApplyCannySimulated(sourceMap); break;
            }

            pcbSonuc.Image = result;
            this.Cursor = Cursors.Default;
        }

        // --- ALGORİTMALAR ---

        private Bitmap MakeGrayscale(Bitmap original)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color c = original.GetPixel(x, y);
                    int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
                    newBitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            return newBitmap;
        }

        private Bitmap ApplyMeanFilter(Bitmap img)
        {
            Bitmap result = new Bitmap(img.Width, img.Height);
            for (int x = 1; x < img.Width - 1; x++)
            {
                for (int y = 1; y < img.Height - 1; y++)
                {
                    int sumR = 0, sumG = 0, sumB = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color c = img.GetPixel(x + i, y + j);
                            sumR += c.R; sumG += c.G; sumB += c.B;
                        }
                    }
                    result.SetPixel(x, y, Color.FromArgb(sumR / 9, sumG / 9, sumB / 9));
                }
            }
            return result;
        }

        private Bitmap ApplyGaussFilter(Bitmap img)
        {
            Bitmap result = new Bitmap(img.Width, img.Height);
            int[,] kernel = { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } }; // Toplam ağırlık 16
            for (int x = 1; x < img.Width - 1; x++)
            {
                for (int y = 1; y < img.Height - 1; y++)
                {
                    int sumR = 0, sumG = 0, sumB = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color c = img.GetPixel(x + i, y + j);
                            int w = kernel[i + 1, j + 1];
                            sumR += c.R * w;
                            sumG += c.G * w;
                            sumB += c.B * w;
                        }
                    }
                    result.SetPixel(x, y, Color.FromArgb(sumR / 16, sumG / 16, sumB / 16));
                }
            }
            return result;
        }

        private Bitmap ApplyMedianFilter(Bitmap img)
        {
            Bitmap result = new Bitmap(img.Width, img.Height);
            for (int x = 1; x < img.Width - 1; x++)
            {
                for (int y = 1; y < img.Height - 1; y++)
                {
                    List<int> r = new List<int>(), g = new List<int>(), b = new List<int>();
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color c = img.GetPixel(x + i, y + j);
                            r.Add(c.R); g.Add(c.G); b.Add(c.B);
                        }
                    }
                    r.Sort(); g.Sort(); b.Sort();
                    result.SetPixel(x, y, Color.FromArgb(r[4], g[4], b[4])); // Ortanca
                }
            }
            return result;
        }

        private Bitmap ApplyUnsharpMasking(Bitmap img)
        {
            Bitmap blurred = ApplyMeanFilter(img);
            Bitmap result = new Bitmap(img.Width, img.Height);
            double k = 2.0; // Keskinleştirme faktörü

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color orig = img.GetPixel(x, y);
                    Color blur = blurred.GetPixel(x, y);

                    int r = (int)(orig.R + k * (orig.R - blur.R));
                    int g = (int)(orig.G + k * (orig.G - blur.G));
                    int b = (int)(orig.B + k * (orig.B - blur.B));

                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));

                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return result;
        }

        private Bitmap ApplySharpenMatrix(Bitmap img)
        {
            Bitmap result = new Bitmap(img.Width, img.Height);
            int[,] kernel = { { 0, -2, 0 }, { -2, 11, -2 }, { 0, -2, 0 } };
            int kernelSum = 3;

            for (int x = 1; x < img.Width - 1; x++)
            {
                for (int y = 1; y < img.Height - 1; y++)
                {
                    int r = 0, g = 0, b = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color c = img.GetPixel(x + i, y + j);
                            int w = kernel[i + 1, j + 1];
                            r += c.R * w; g += c.G * w; b += c.B * w;
                        }
                    }
                    r = Math.Min(255, Math.Max(0, r / kernelSum));
                    g = Math.Min(255, Math.Max(0, g / kernelSum));
                    b = Math.Min(255, Math.Max(0, b / kernelSum));
                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return result;
        }

        private Bitmap ApplyConvolution(Bitmap img, string type)
        {
            Bitmap grayImg = MakeGrayscale(img);
            Bitmap result = new Bitmap(img.Width, img.Height);
            int[,] gx, gy;

            if (type == "Sobel")
            {
                gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
                gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            }
            else // Prewitt
            {
                gx = new int[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
                gy = new int[,] { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };
            }

            for (int x = 1; x < img.Width - 1; x++)
            {
                for (int y = 1; y < img.Height - 1; y++)
                {
                    int sumX = 0, sumY = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int val = grayImg.GetPixel(x + i, y + j).R;
                            sumX += val * gx[i + 1, j + 1];
                            sumY += val * gy[i + 1, j + 1];
                        }
                    }
                    int total = Math.Min(255, Math.Abs(sumX) + Math.Abs(sumY));
                    result.SetPixel(x, y, Color.FromArgb(total, total, total));
                }
            }
            return result;
        }

        private Bitmap ApplyRobertsCross(Bitmap img)
        {
            Bitmap grayImg = MakeGrayscale(img);
            Bitmap result = new Bitmap(img.Width, img.Height);

            for (int x = 0; x < img.Width - 1; x++)
            {
                for (int y = 0; y < img.Height - 1; y++)
                {
                    int p1 = grayImg.GetPixel(x, y).R;
                    int p2 = grayImg.GetPixel(x + 1, y).R;
                    int p3 = grayImg.GetPixel(x, y + 1).R;
                    int p4 = grayImg.GetPixel(x + 1, y + 1).R;

                    int Gx = Math.Abs(p1 - p4);
                    int Gy = Math.Abs(p2 - p3);
                    int total = Math.Min(255, Gx + Gy);
                    result.SetPixel(x, y, Color.FromArgb(total, total, total));
                }
            }
            return result;
        }

        private Bitmap ApplyCannySimulated(Bitmap img)
        {
            // Canny çok aşamalıdır (Gauss -> Sobel -> NonMaxSuppression -> Hysteresis).
            // Burada basit bir simülasyon yapıyoruz: Önce Sobel, sonra Eşikleme.
            Bitmap sobel = ApplyConvolution(img, "Sobel");
            Bitmap result = new Bitmap(img.Width, img.Height);

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    // Basit eşikleme ile Canny kenar inceltmesini taklit et
                    if (sobel.GetPixel(x, y).R > 100)
                        result.SetPixel(x, y, Color.White);
                    else
                        result.SetPixel(x, y, Color.Black);
                }
            }
            return result;
        }
    }
}