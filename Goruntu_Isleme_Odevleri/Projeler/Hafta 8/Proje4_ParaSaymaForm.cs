using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_ParaSaymaForm : Form
    {
        private PictureBox pcbOriginal, pcbProcessed;
        private Label lblSonuc, lblAyarlar;
        private Button btnYukle, btnAnaliz, btnGeri;

        private TrackBar tbEsik; // Siyah/Beyaz ayrımı
        private TrackBar tbBoyut25, tbBoyut50; // Boyut sınırları (Pixel cinsinden çap)
        private Label lblEsik, lblBoyut25, lblBoyut50;

        private Bitmap originalBitmap;
        private Form haftaFormu;

        class CoinObject
        {
            public Point Merkez { get; set; }
            public int Cap { get; set; } // Diameter
            public Bitmap CroppedImage { get; set; } // Paranın kesilmiş görüntüsü
            public string Deger { get; set; } // 1 TL, 50 Kr...
            public string Yuz { get; set; } // Yazı / Tura
            public double DokuSkoru { get; set; } // Yazı tura ayrımı için kenar yoğunluğu
        }

        public Proje4_ParaSaymaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Para Sayma ve Yazı/Tura Tespiti";
            SetupUI();

            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void SetupUI()
        {
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            int margin = 20;

            pcbOriginal = new PictureBox() { Location = new Point(margin, 40), Size = new Size(400, 400), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            pcbProcessed = new PictureBox() { Location = new Point(margin + 420, 40), Size = new Size(400, 400), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            Label lbl1 = new Label() { Text = "Orijinal Görüntü", Location = new Point(margin, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            Label lbl2 = new Label() { Text = "Tespit Edilen Paralar", Location = new Point(margin + 420, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            int ctrlX = 840;
            int ctrlY = 40;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(ctrlX, ctrlY), Size = new Size(200, 40) };
            btnYukle.Click += BtnYukle_Click;

            ctrlY += 60;
            lblEsik = new Label() { Text = "Zemin Ayrımı (Eşik): 128", Location = new Point(ctrlX, ctrlY), AutoSize = true };
            tbEsik = new TrackBar() { Location = new Point(ctrlX, ctrlY + 20), Size = new Size(200, 45), Maximum = 255, Value = 180 }; // Beyaz zemin için yüksek eşik
            tbEsik.Scroll += (s, e) => lblEsik.Text = $"Zemin Ayrımı (Eşik): {tbEsik.Value}";

            ctrlY += 70;
            lblAyarlar = new Label() { Text = "BOYUT SINIRLARI (Çap)", Location = new Point(ctrlX, ctrlY), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold), ForeColor = Color.Red };

            ctrlY += 25;
            lblBoyut25 = new Label() { Text = "25kr Limiti (Max): 60px", Location = new Point(ctrlX, ctrlY), AutoSize = true };
            tbBoyut25 = new TrackBar() { Location = new Point(ctrlX, ctrlY + 20), Size = new Size(200, 45), Maximum = 300, Value = 60 };
            tbBoyut25.Scroll += (s, e) => lblBoyut25.Text = $"25kr Limiti (Max): {tbBoyut25.Value}px";

            ctrlY += 70;
            lblBoyut50 = new Label() { Text = "50kr Limiti (Max): 80px", Location = new Point(ctrlX, ctrlY), AutoSize = true };
            tbBoyut50 = new TrackBar() { Location = new Point(ctrlX, ctrlY + 20), Size = new Size(200, 45), Maximum = 300, Value = 80 };
            tbBoyut50.Scroll += (s, e) => lblBoyut50.Text = $"50kr Limiti (Max): {tbBoyut50.Value}px";

            Label lblInfo = new Label() { Text = "* Bundan büyükler 1 TL sayılır.", Location = new Point(ctrlX, ctrlY + 60), AutoSize = true, Font = new Font("Arial", 8) };

            ctrlY += 90;
            btnAnaliz = new Button() { Text = "PARALARI SAY VE TANI", Location = new Point(ctrlX, ctrlY), Size = new Size(200, 50), BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnAnaliz.Click += BtnAnaliz_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX, ctrlY + 60), Size = new Size(200, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            lblSonuc = new Label() { Text = "Toplam: 0.00 TL", Location = new Point(margin, 460), Size = new Size(800, 100), Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Blue };

            this.Controls.AddRange(new Control[] {
                pcbOriginal, pcbProcessed, lbl1, lbl2, btnYukle,
                lblEsik, tbEsik, lblAyarlar, lblBoyut25, tbBoyut25, lblBoyut50, tbBoyut50, lblInfo,
                btnAnaliz, btnGeri, lblSonuc
            });
        }

        private void InitializeComponent() { this.Name = "Proje4_ParaSaymaForm"; }

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(ofd.FileName);
                pcbOriginal.Image = originalBitmap;
                btnAnaliz.Enabled = true;
            }
        }

        private void BtnAnaliz_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // Orijinal resmi bozmamak için geçici bir kopya oluşturuyoruz.
            Bitmap tempBitmap = new Bitmap(originalBitmap);

            // Resmi İşle
            List<CoinObject> coins = FindCoins(tempBitmap, tbEsik.Value);

            // Boyut ve Doku Analizi
            double toplamTutar = 0;
            int limit25 = tbBoyut25.Value;
            int limit50 = tbBoyut50.Value;

            // Çizim aracı
            Graphics g = Graphics.FromImage(tempBitmap);

            foreach (var coin in coins)
            {
                // DEĞER BELİRLEME
                double deger = 0;
                if (coin.Cap <= limit25) { coin.Deger = "25 Kr"; deger = 0.25; }
                else if (coin.Cap <= limit50) { coin.Deger = "50 Kr"; deger = 0.50; }
                else { coin.Deger = "1 TL"; deger = 1.00; }

                toplamTutar += deger;

                // YAZI / TURA BELİRLEME
                AnalyzeTexture(coin);

                // Çizim
                Pen p = new Pen(Color.Red, 3);
                g.DrawEllipse(p, coin.Merkez.X - coin.Cap / 2, coin.Merkez.Y - coin.Cap / 2, coin.Cap, coin.Cap);

                string info = $"{coin.Deger}\n{coin.Yuz}\n(Doku:{coin.DokuSkoru:0})";
                g.DrawString(info, new Font("Arial", 8, FontStyle.Bold), Brushes.Yellow, coin.Merkez.X - 20, coin.Merkez.Y - 10);
            }

            // Sonucu kutuda göster
            pcbProcessed.Image = tempBitmap;

            lblSonuc.Text = $"Bulunan Para Sayısı: {coins.Count}\n" +
                            $"Toplam Tutar: {toplamTutar:0.00} TL\n" +
                            $"Detay: {coins.FindAll(x => x.Deger == "1 TL").Count}x1TL, {coins.FindAll(x => x.Deger == "50 Kr").Count}x50kr, {coins.FindAll(x => x.Deger == "25 Kr").Count}x25kr";

            this.Cursor = Cursors.Default;
        }


        // Blobları Bulma (Connected Component Labeling - FloodFill)
        private List<CoinObject> FindCoins(Bitmap src, int threshold)
        {
            List<CoinObject> coins = new List<CoinObject>();
            int w = src.Width;
            int h = src.Height;
            bool[,] visited = new bool[w, h];

            for (int x = 0; x < w; x += 2)
            {
                for (int y = 0; y < h; y += 2)
                {
                    if (visited[x, y]) continue;

                    Color c = src.GetPixel(x, y);
                    int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    if (gray < threshold)
                    {
                        var coin = FloodFillCoin(src, visited, x, y, threshold);

                        if (coin.Cap > 20)
                        {
                            coins.Add(coin);
                        }
                    }
                }
            }
            return coins;
        }

        private CoinObject FloodFillCoin(Bitmap bmp, bool[,] visited, int startX, int startY, int thres)
        {
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(startX, startY));
            visited[startX, startY] = true;

            int minX = startX, maxX = startX;
            int minY = startY, maxY = startY;

            while (q.Count > 0)
            {
                Point p = q.Dequeue();

                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;

                int[] dx = { 2, -2, 0, 0 };
                int[] dy = { 0, 0, 2, -2 };

                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i];
                    int ny = p.Y + dy[i];

                    if (nx >= 0 && nx < bmp.Width && ny >= 0 && ny < bmp.Height)
                    {
                        if (!visited[nx, ny])
                        {
                            Color c = bmp.GetPixel(nx, ny);
                            int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                            if (gray < thres)
                            {
                                visited[nx, ny] = true;
                                q.Enqueue(new Point(nx, ny));
                            }
                        }
                    }
                }
            }

            int width = maxX - minX;
            int height = maxY - minY;
            int diameter = Math.Max(width, height);
            Point center = new Point((minX + maxX) / 2, (minY + maxY) / 2);

            Rectangle cropRect = new Rectangle(minX, minY, width, height);
            Bitmap cropped = null;
            try
            {
                cropped = bmp.Clone(cropRect, bmp.PixelFormat);
            }
            catch { /* Hata olursa boş geç */ }

            return new CoinObject { Merkez = center, Cap = diameter, CroppedImage = cropped };
        }

        // Doku Analizi (Yazı mı Tura mı?)
        private void AnalyzeTexture(CoinObject coin)
        {
            if (coin.CroppedImage == null) return;

            Bitmap img = coin.CroppedImage;
            int edgeCount = 0;
            int totalPixels = 0;

            for (int x = 1; x < img.Width - 1; x++)
            {
                for (int y = 1; y < img.Height - 1; y++)
                {
                    Color c1 = img.GetPixel(x - 1, y);
                    Color c2 = img.GetPixel(x + 1, y);
                    Color c3 = img.GetPixel(x, y - 1);
                    Color c4 = img.GetPixel(x, y + 1);

                    int diff = Math.Abs(c1.R - c2.R) + Math.Abs(c3.R - c4.R);

                    if (diff > 30)
                    {
                        edgeCount++;
                    }
                    totalPixels++;
                }
            }

            if (totalPixels > 0)
                coin.DokuSkoru = ((double)edgeCount / totalPixels) * 1000;
            else
                coin.DokuSkoru = 0;

            if (coin.DokuSkoru > 80)
                coin.Yuz = "TURA";
            else
                coin.Yuz = "YAZI";
        }
    }
}