using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_KenarBulmaForm : Form
    {
        // UI Elemanları
        private PictureBox pcbKaynak, pcbSonuc;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbAlgoritma;
        private Label lblAlgo;

        // Değişkenler
        private Bitmap orijinalResim;
        private Form haftaFormu; // Geri dönülecek menü formu

        public Proje1_KenarBulmaForm(Form parent)
        {
            InitializeComponent();
            haftaFormu = parent; // Geri dönülecek formu kaydet
            this.Text = "Proje 1: Kenar Bulma Algoritmaları";
        }

        private void InitializeComponent()
        {
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed); // X'e basınca da geri dön

            // --- UI OLUŞTURMA ---

            // 1. Kaynak Resim Kutusu
            pcbKaynak = new PictureBox();
            pcbKaynak.Location = new Point(20, 20);
            pcbKaynak.Size = new Size(350, 350);
            pcbKaynak.SizeMode = PictureBoxSizeMode.Zoom;
            pcbKaynak.BorderStyle = BorderStyle.FixedSingle;
            pcbKaynak.BackColor = Color.LightGray;
            this.Controls.Add(pcbKaynak);

            // 2. Sonuç Resim Kutusu
            pcbSonuc = new PictureBox();
            pcbSonuc.Location = new Point(390, 20);
            pcbSonuc.Size = new Size(350, 350);
            pcbSonuc.SizeMode = PictureBoxSizeMode.Zoom;
            pcbSonuc.BorderStyle = BorderStyle.FixedSingle;
            pcbSonuc.BackColor = Color.LightGray;
            this.Controls.Add(pcbSonuc);

            // Kontrol Paneli (Sağ Taraf)
            int panelX = 760;

            // Resim Yükle
            btnYukle = new Button();
            btnYukle.Text = "Resim Yükle";
            btnYukle.Location = new Point(panelX, 20);
            btnYukle.Size = new Size(110, 40);
            btnYukle.Click += BtnYukle_Click;
            this.Controls.Add(btnYukle);

            // Algoritma Seçimi
            lblAlgo = new Label();
            lblAlgo.Text = "Algoritma:";
            lblAlgo.Location = new Point(panelX, 80);
            this.Controls.Add(lblAlgo);

            cmbAlgoritma = new ComboBox();
            cmbAlgoritma.Location = new Point(panelX, 105);
            cmbAlgoritma.Size = new Size(110, 30);
            cmbAlgoritma.Items.Add("Sobel");
            cmbAlgoritma.Items.Add("Prewitt");
            cmbAlgoritma.Items.Add("Canny (Manuel)");
            cmbAlgoritma.Items.Add("Bulanık Farkı");
            cmbAlgoritma.Items.Add("Aşındırma Farkı");
            cmbAlgoritma.SelectedIndex = 0;
            this.Controls.Add(cmbAlgoritma);

            // Uygula Butonu
            btnUygula = new Button();
            btnUygula.Text = "Uygula";
            btnUygula.Location = new Point(panelX, 150);
            btnUygula.Size = new Size(110, 50);
            btnUygula.BackColor = Color.LightBlue;
            btnUygula.Click += BtnUygula_Click;
            this.Controls.Add(btnUygula);

            // Geri Dön Butonu
            btnGeri = new Button();
            btnGeri.Text = "Hafta Menüsüne Dön";
            btnGeri.Location = new Point(20, 480); // Sol alt köşe
            btnGeri.Size = new Size(150, 40);
            btnGeri.BackColor = Color.LightCoral;
            btnGeri.Click += BtnGeri_Click;
            this.Controls.Add(btnGeri);
        }

        // --- OLAYLAR (EVENTS) ---

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                orijinalResim = new Bitmap(ofd.FileName);
                pcbKaynak.Image = orijinalResim;
            }
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            this.Close(); // Bu formu kapatır, FormClosed eventi tetiklenir
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (haftaFormu != null)
                haftaFormu.Show(); // Ana menüyü tekrar göster
        }

        private void BtnUygula_Click(object sender, EventArgs e)
        {
            if (orijinalResim == null) return;

            Bitmap griResim = GriTonlamayaCevir(orijinalResim);
            Bitmap sonuc = null;

            switch (cmbAlgoritma.SelectedIndex)
            {
                case 0: sonuc = ApplyConvolution(griResim, "Sobel"); break;
                case 1: sonuc = ApplyConvolution(griResim, "Prewitt"); break;
                case 2: sonuc = ApplyCanny(griResim); break;
                case 3: sonuc = ApplyBlurEdge(griResim); break;
                case 4: sonuc = ApplyErosionEdge(griResim); break;
            }
            pcbSonuc.Image = sonuc;
        }

        // --- ALGORİTMALAR ---

        private Bitmap GriTonlamayaCevir(Bitmap bmp)
        {
            Bitmap gri = new Bitmap(bmp.Width, bmp.Height);
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color c = bmp.GetPixel(i, j);
                    int val = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
                    gri.SetPixel(i, j, Color.FromArgb(val, val, val));
                }
            }
            return gri;
        }

        // Sobel ve Prewitt
        private Bitmap ApplyConvolution(Bitmap bmp, string type)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            Bitmap sonuc = new Bitmap(w, h);

            int[,] gx, gy;
            if (type == "Sobel")
            {
                gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
                gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
            }
            else
            { // Prewitt
                gx = new int[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
                gy = new int[,] { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };
            }

            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    int valX = 0, valY = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int px = bmp.GetPixel(x + i, y + j).R;
                            valX += px * gx[i + 1, j + 1];
                            valY += px * gy[i + 1, j + 1];
                        }
                    }
                    int total = (int)Math.Sqrt(valX * valX + valY * valY);
                    total = Math.Min(255, Math.Max(0, total));
                    sonuc.SetPixel(x, y, Color.FromArgb(total, total, total));
                }
            }
            return sonuc;
        }

        // Canny (Manuel)
        private Bitmap ApplyCanny(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            // 1. Gauss Blur
            Bitmap blurred = new Bitmap(w, h);
            int[,] gauss = { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    int sum = 0;
                    for (int i = -1; i <= 1; i++)
                        for (int j = -1; j <= 1; j++) sum += bmp.GetPixel(x + i, y + j).R * gauss[i + 1, j + 1];
                    int v = sum / 16;
                    blurred.SetPixel(x, y, Color.FromArgb(v, v, v));
                }
            }

            // 2. Gradyan ve Açı
            double[,] grad = new double[w, h];
            double[,] ang = new double[w, h];
            int[,] gx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    double vx = 0, vy = 0;
                    for (int i = -1; i <= 1; i++)
                        for (int j = -1; j <= 1; j++)
                        {
                            int p = blurred.GetPixel(x + i, y + j).R;
                            vx += p * gx[i + 1, j + 1]; vy += p * gy[i + 1, j + 1];
                        }
                    grad[x, y] = Math.Sqrt(vx * vx + vy * vy);
                    ang[x, y] = (Math.Atan2(vy, vx) * 180.0 / Math.PI);
                }
            }

            // 3. Non-Max Suppression
            Bitmap res = new Bitmap(w, h);
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    double angle = ang[x, y];
                    if (angle < 0) angle += 180;
                    double q = 255, r = 255;

                    if ((angle >= 0 && angle < 22.5) || (angle >= 157.5)) { q = grad[x, y + 1]; r = grad[x, y - 1]; }
                    else if (angle >= 22.5 && angle < 67.5) { q = grad[x + 1, y - 1]; r = grad[x - 1, y + 1]; }
                    else if (angle >= 67.5 && angle < 112.5) { q = grad[x + 1, y]; r = grad[x - 1, y]; }
                    else if (angle >= 112.5 && angle < 157.5) { q = grad[x - 1, y - 1]; r = grad[x + 1, y + 1]; }

                    if (grad[x, y] >= q && grad[x, y] >= r)
                    {
                        int val = (int)grad[x, y];
                        val = val > 50 ? (val > 255 ? 255 : val) : 0; // Threshold
                        res.SetPixel(x, y, Color.FromArgb(val, val, val));
                    }
                    else res.SetPixel(x, y, Color.Black);
                }
            }
            return res;
        }

        // Blur Farkı
        private Bitmap ApplyBlurEdge(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            Bitmap sonuc = new Bitmap(w, h);
            // Mean Filter
            Bitmap blur = new Bitmap(w, h);
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    int sum = 0;
                    for (int i = -1; i <= 1; i++) for (int j = -1; j <= 1; j++) sum += bmp.GetPixel(x + i, y + j).R;
                    int avg = sum / 9;
                    blur.SetPixel(x, y, Color.FromArgb(avg, avg, avg));
                }
            }
            // Difference
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int diff = Math.Abs(bmp.GetPixel(x, y).R - blur.GetPixel(x, y).R) * 3;
                    diff = Math.Min(255, diff);
                    sonuc.SetPixel(x, y, Color.FromArgb(diff, diff, diff));
                }
            }
            return sonuc;
        }

        // Erosion Farkı
        private Bitmap ApplyErosionEdge(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            Bitmap eroded = new Bitmap(w, h);
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    int min = 255;
                    for (int i = -1; i <= 1; i++) for (int j = -1; j <= 1; j++)
                        {
                            int v = bmp.GetPixel(x + i, y + j).R;
                            if (v < min) min = v;
                        }
                    eroded.SetPixel(x, y, Color.FromArgb(min, min, min));
                }
            }
            Bitmap sonuc = new Bitmap(w, h);
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    int diff = bmp.GetPixel(x, y).R - eroded.GetPixel(x, y).R;
                    diff = Math.Min(255, Math.Max(0, diff));
                    sonuc.SetPixel(x, y, Color.FromArgb(diff, diff, diff));
                }
            }
            return sonuc;
        }
    }
}