using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje6_BolgeselNetlestirmeBulaniklastirmaForm : Form
    {
        // UI Elemanları
        private PictureBox pcbGoruntu;
        private Button btnResimYukle;
        private Button btnUygula;
        private Button btnTersine;
        private Button btnTemizle;
        private Button btnGeri;

        private ComboBox cmbIcAlgoritma;
        private ComboBox cmbDisAlgoritma;

        private Label lblIc;
        private Label lblDis;

        // Değişkenler
        private Form haftaFormu;
        private Bitmap orijinalResim;
        private List<Point> noktalar; 
        private bool tersineModu = false; 

        public Proje6_BolgeselNetlestirmeBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();

            haftaFormu = parentForm;
            this.Text = "Proje 6: Bölgesel Netleştirme ve Bulanıklaştırma";

            noktalar = new List<Point>();

            pcbGoruntu = new PictureBox();
            pcbGoruntu.Location = new Point(20, 20);
            pcbGoruntu.Size = new Size(500, 500);
            pcbGoruntu.BorderStyle = BorderStyle.FixedSingle;
            pcbGoruntu.BackColor = Color.LightGray;
            pcbGoruntu.SizeMode = PictureBoxSizeMode.AutoSize; // Resim boyutuna göre
            pcbGoruntu.MouseClick += new MouseEventHandler(pcbGoruntu_MouseClick);
            pcbGoruntu.Paint += new PaintEventHandler(pcbGoruntu_Paint);

            this.Controls.Add(pcbGoruntu);

            int panelX = 540;
            int currentY = 20;

            btnResimYukle = new Button();
            btnResimYukle.Text = "Resim Yükle";
            btnResimYukle.Location = new Point(panelX, currentY);
            btnResimYukle.Size = new Size(180, 40);
            btnResimYukle.Click += new EventHandler(btnResimYukle_Click);
            this.Controls.Add(btnResimYukle);

            currentY += 50;

            lblIc = new Label();
            lblIc.Text = "Bölge İÇİ Algoritması:";
            lblIc.Location = new Point(panelX, currentY);
            lblIc.AutoSize = true;
            this.Controls.Add(lblIc);

            currentY += 25;

            cmbIcAlgoritma = new ComboBox();
            cmbIcAlgoritma.Location = new Point(panelX, currentY);
            cmbIcAlgoritma.Size = new Size(180, 25);
            cmbIcAlgoritma.Items.AddRange(new object[] { "Orjinal (Yok)", "Mean (Bulanık)", "Median (Gürültü Sil)", "Gauss (Yumuşak)", "Sharpen (Net)" });
            cmbIcAlgoritma.SelectedIndex = 4; // Default: Sharpen
            this.Controls.Add(cmbIcAlgoritma);

            currentY += 40;

            lblDis = new Label();
            lblDis.Text = "Bölge DIŞI Algoritması:";
            lblDis.Location = new Point(panelX, currentY);
            lblDis.AutoSize = true;
            this.Controls.Add(lblDis);

            currentY += 25;

            cmbDisAlgoritma = new ComboBox();
            cmbDisAlgoritma.Location = new Point(panelX, currentY);
            cmbDisAlgoritma.Size = new Size(180, 25);
            cmbDisAlgoritma.Items.AddRange(new object[] { "Orjinal (Yok)", "Mean (Bulanık)", "Median (Gürültü Sil)", "Gauss (Yumuşak)", "Sharpen (Net)" });
            cmbDisAlgoritma.SelectedIndex = 1; // Default: Mean
            this.Controls.Add(cmbDisAlgoritma);

            currentY += 50;

            btnUygula = new Button();
            btnUygula.Text = "Filtreleri Uygula";
            btnUygula.Location = new Point(panelX, currentY);
            btnUygula.Size = new Size(180, 40);
            btnUygula.BackColor = Color.LightGreen;
            btnUygula.Click += new EventHandler(btnUygula_Click);
            this.Controls.Add(btnUygula);

            currentY += 50;

            btnTersine = new Button();
            btnTersine.Text = "Mod: Normal\n(Tersine Çevir)";
            btnTersine.Location = new Point(panelX, currentY);
            btnTersine.Size = new Size(180, 50);
            btnTersine.Click += new EventHandler(btnTersine_Click);
            this.Controls.Add(btnTersine);

            currentY += 60;

            btnTemizle = new Button();
            btnTemizle.Text = "Seçimi Temizle";
            btnTemizle.Location = new Point(panelX, currentY);
            btnTemizle.Size = new Size(180, 35);
            btnTemizle.Click += new EventHandler(btnTemizle_Click);
            this.Controls.Add(btnTemizle);

            currentY += 60;

            btnGeri = new Button();
            btnGeri.Text = "Menüye Dön";
            btnGeri.Location = new Point(panelX, currentY);
            btnGeri.Size = new Size(180, 40);
            btnGeri.BackColor = Color.LightCoral;
            btnGeri.Click += new EventHandler(btnGeri_Click);
            this.Controls.Add(btnGeri);
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_BolgeselIslemeForm";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        // OLAY YÖNETİMİ (EVENTS) 

        private void btnResimYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                orijinalResim = new Bitmap(ofd.FileName);
                pcbGoruntu.Image = new Bitmap(orijinalResim); // Görüntülenen kopya
                noktalar.Clear(); // Yeni resim gelince eski seçimleri sil
            }
        }

        // Mouse ile nokta seçimi
        private void pcbGoruntu_MouseClick(object sender, MouseEventArgs e)
        {
            if (orijinalResim == null) return;

            if (e.Button == MouseButtons.Left)
            {
                noktalar.Add(e.Location);
                pcbGoruntu.Invalidate(); // Paint olayını tetikle (çizgileri çizmek için)
            }
        }

        // Seçilen poligonu ekrana çizme
        private void pcbGoruntu_Paint(object sender, PaintEventArgs e)
        {
            if (noktalar.Count > 0)
            {
                // Noktaları küçük daireler olarak çiz
                foreach (Point p in noktalar)
                {
                    e.Graphics.FillEllipse(Brushes.Red, p.X - 3, p.Y - 3, 6, 6);
                }

                // Noktaları birleştiren çizgileri çiz
                if (noktalar.Count > 1)
                {
                    e.Graphics.DrawLines(Pens.Yellow, noktalar.ToArray());

                    // Poligonu kapatan çizgiyi (sondan başa) çiz
                    if (noktalar.Count > 2)
                    {
                        e.Graphics.DrawLine(Pens.Yellow, noktalar[noktalar.Count - 1], noktalar[0]);
                    }
                }
            }
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            noktalar.Clear();
            if (orijinalResim != null)
                pcbGoruntu.Image = new Bitmap(orijinalResim); // Resmi sıfırla
            pcbGoruntu.Invalidate();
        }

        private void btnTersine_Click(object sender, EventArgs e)
        {
            tersineModu = !tersineModu;
            if (tersineModu)
                btnTersine.Text = "Mod: TERS\n(Normale Dön)";
            else
                btnTersine.Text = "Mod: Normal\n(Tersine Çevir)";
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }

        // ASIL ALGORİTMA

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (orijinalResim == null || noktalar.Count < 3)
            {
                MessageBox.Show("Lütfen önce bir resim yükleyin ve en az 3 nokta seçerek bir bölge oluşturun.");
                return;
            }

            int w = orijinalResim.Width;
            int h = orijinalResim.Height;

            // MASKE OLUŞTURMA
            Bitmap maske = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(maske))
            {
                g.Clear(Color.Black); // Dışarısı Siyah
                g.FillPolygon(Brushes.White, noktalar.ToArray()); // İçerisi Beyaz
            }

            Bitmap sonucResmi = new Bitmap(w, h);

            // Seçilen algoritmaları al
            string icAlgo = cmbIcAlgoritma.SelectedItem.ToString();
            string disAlgo = cmbDisAlgoritma.SelectedItem.ToString();

            // PİKSEL TARAMA VE FİLTRELEME
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    // Maske kontrolü: Bu piksel içeride mi?
                    Color maskeRenk = maske.GetPixel(x, y);
                    bool icerdeMi = (maskeRenk.R == 255); // Beyaz ise içeridedir.

                    // Eğer "Tersine" butonu aktifse durumu tersle
                    if (tersineModu) icerdeMi = !icerdeMi;

                    Color yeniRenk;

                    if (icerdeMi)
                    {
                        yeniRenk = FiltreUygula(orijinalResim, x, y, icAlgo);
                    }
                    else
                    {
                        yeniRenk = FiltreUygula(orijinalResim, x, y, disAlgo);
                    }

                    sonucResmi.SetPixel(x, y, yeniRenk);
                }
            }

            pcbGoruntu.Image = sonucResmi;
            MessageBox.Show("İşlem Tamamlandı!");
        }

        // FİLTRE FONKSİYONLARI 

        private Color FiltreUygula(Bitmap kaynak, int x, int y, string algoritma)
        {
            if (algoritma.StartsWith("Orjinal")) return kaynak.GetPixel(x, y);
            if (algoritma.StartsWith("Median")) return MedianFiltresi(kaynak, x, y);

            // Konvolüsyon çekirdekleri (Kernels)
            double[,] kernel = null;

            if (algoritma.StartsWith("Mean")) // Ortalama (Bulanıklaştırma)
            {
                kernel = new double[,] {
                    { 1, 1, 1 },
                    { 1, 1, 1 },
                    { 1, 1, 1 }
                };
            }
            else if (algoritma.StartsWith("Gauss")) // Gaussian Blur
            {
                kernel = new double[,] {
                    { 1, 2, 1 },
                    { 2, 4, 2 },
                    { 1, 2, 1 }
                };
            }
            else if (algoritma.StartsWith("Sharpen")) // Keskinleştirme
            {
                kernel = new double[,] {
                    {  0, -1,  0 },
                    { -1,  5, -1 },
                    {  0, -1,  0 }
                };
            }

            return KonvolusyonUygula(kaynak, x, y, kernel);
        }

        private Color KonvolusyonUygula(Bitmap bmp, int x, int y, double[,] kernel)
        {
            double rToplam = 0, gToplam = 0, bToplam = 0;
            double kernelToplam = 0;

            // 3x3 Matris taraması
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Color p = bmp.GetPixel(x + i, y + j);
                    double deger = kernel[i + 1, j + 1];

                    rToplam += p.R * deger;
                    gToplam += p.G * deger;
                    bToplam += p.B * deger;

                    kernelToplam += deger;
                }
            }

            // Sharpen gibi filtrelerde kernel toplamı 1 veya 0 olabilir, bölme hatasını önle.
            // Mean ve Gauss için toplam ağırlığa bölüyoruz.
            if (kernelToplam <= 0) kernelToplam = 1;

            int r = (int)(rToplam / kernelToplam);
            int g = (int)(gToplam / kernelToplam);
            int b = (int)(bToplam / kernelToplam);

            // Renk değerlerini 0-255 arasına sıkıştır (Clamp)
            r = Math.Min(255, Math.Max(0, r));
            g = Math.Min(255, Math.Max(0, g));
            b = Math.Min(255, Math.Max(0, b));

            return Color.FromArgb(r, g, b);
        }

        private Color MedianFiltresi(Bitmap bmp, int x, int y)
        {
            List<int> rList = new List<int>();
            List<int> gList = new List<int>();
            List<int> bList = new List<int>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Color p = bmp.GetPixel(x + i, y + j);
                    rList.Add(p.R);
                    gList.Add(p.G);
                    bList.Add(p.B);
                }
            }

            rList.Sort();
            gList.Sort();
            bList.Sort();

            // 9 elemanlı listenin ortası (indeks 4)
            return Color.FromArgb(rList[4], gList[4], bList[4]);
        }
    }
}