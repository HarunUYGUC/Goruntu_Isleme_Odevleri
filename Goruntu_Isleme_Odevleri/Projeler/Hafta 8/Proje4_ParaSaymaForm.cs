using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_ParaSaymaForm : Form
    {
        private PictureBox pcbOriginal, pcbProcessed;
        private Label lblSonuc, lblAyarlar;
        private Button btnYukle, btnAnaliz, btnGeri;

        private TrackBar tbEsik; // Binary Threshold
        private TrackBar tbBoyut25, tbBoyut50; // Para çapı sınırları
        private Label lblEsik, lblBoyut25, lblBoyut50;

        private Bitmap originalBitmap;
        private Form haftaFormu;

        // Para Nesnesi Sınıfı
        class CoinObject
        {
            public Point Merkez { get; set; }
            public int Cap { get; set; } // Çap (Piksel)
            public Bitmap CroppedImage { get; set; } // Paranın kesilmiş görüntüsü
            public string Deger { get; set; } // 1 TL, 50 Kr...
            public string Yuz { get; set; } // Yazı / Tura
            public double DokuSkoru { get; set; } // Kenar/Histogram skoru
        }

        public Proje4_ParaSaymaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Para Sayma ve Yazı/Tura Tespiti";

            SetupUI();

            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void InitializeComponent() { this.Name = "Proje4_ParaSaymaForm"; }

        private void SetupUI()
        {
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            int margin = 20;

            Label lbl1 = new Label() { Text = "a) Orijinal Görüntü (Beyaz Zemin)", Location = new Point(margin, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, 40), Size = new Size(400, 400), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            Label lbl2 = new Label() { Text = "b) Tespit ve Yazı/Tura Analizi", Location = new Point(margin + 420, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbProcessed = new PictureBox() { Location = new Point(margin + 420, 40), Size = new Size(400, 400), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            int ctrlX = 840;
            int ctrlY = 40;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(ctrlX, ctrlY), Size = new Size(200, 40) };
            btnYukle.Click += BtnYukle_Click;

            ctrlY += 60;
            lblEsik = new Label() { Text = "Zemin Ayrımı (Eşik): 180", Location = new Point(ctrlX, ctrlY), AutoSize = true };
            tbEsik = new TrackBar() { Location = new Point(ctrlX, ctrlY + 20), Size = new Size(200, 45), Maximum = 255, Value = 180, TickFrequency = 10 };
            tbEsik.Scroll += (s, e) => lblEsik.Text = $"Zemin Ayrımı (Eşik): {tbEsik.Value}";

            ctrlY += 70;
            lblAyarlar = new Label() { Text = "BOYUT KALİBRASYONU (Çap)", Location = new Point(ctrlX, ctrlY), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold), ForeColor = Color.Red };

            ctrlY += 25;
            lblBoyut25 = new Label() { Text = "25kr Limiti (Max): 60px", Location = new Point(ctrlX, ctrlY), AutoSize = true };
            tbBoyut25 = new TrackBar() { Location = new Point(ctrlX, ctrlY + 20), Size = new Size(200, 45), Maximum = 300, Value = 60, TickFrequency = 10 };
            tbBoyut25.Scroll += (s, e) => lblBoyut25.Text = $"25kr Limiti (Max): {tbBoyut25.Value}px";

            ctrlY += 70;
            lblBoyut50 = new Label() { Text = "50kr Limiti (Max): 80px", Location = new Point(ctrlX, ctrlY), AutoSize = true };
            tbBoyut50 = new TrackBar() { Location = new Point(ctrlX, ctrlY + 20), Size = new Size(200, 45), Maximum = 300, Value = 80, TickFrequency = 10 };
            tbBoyut50.Scroll += (s, e) => lblBoyut50.Text = $"50kr Limiti (Max): {tbBoyut50.Value}px";

            ctrlY += 90;
            btnAnaliz = new Button() { Text = "ANALİZ ET", Location = new Point(ctrlX, ctrlY), Size = new Size(200, 50), BackColor = Color.LightGreen, Font = new Font("Arial", 11, FontStyle.Bold) };
            btnAnaliz.Click += BtnAnaliz_Click;

            btnGeri = new Button() { Text = "Menüye Dön", Location = new Point(ctrlX, ctrlY + 60), Size = new Size(200, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            lblSonuc = new Label() { Text = "Sonuçlar burada görünecek...", Location = new Point(margin, 460), Size = new Size(800, 150), Font = new Font("Consolas", 11, FontStyle.Regular), ForeColor = Color.Blue };

            this.Controls.AddRange(new Control[] { pcbOriginal, pcbProcessed, lbl1, lbl2, btnYukle, lblEsik, tbEsik, lblAyarlar, lblBoyut25, tbBoyut25, lblBoyut50, tbBoyut50, btnAnaliz, btnGeri, lblSonuc });
        }

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(ofd.FileName);
                pcbOriginal.Image = originalBitmap;
                pcbProcessed.Image = null;
                lblSonuc.Text = "Resim yüklendi. Ayarları yapıp 'Analiz Et' butonuna basın.";
            }
        }

        private void BtnAnaliz_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // İşlem yapılacak kopyayı oluştur
            Bitmap processedBmp = new Bitmap(originalBitmap);

            // Paraları Bul (Blob Detection)
            List<CoinObject> coins = FindCoins(originalBitmap, tbEsik.Value);

            double totalValue = 0;
            int count1TL = 0, count50Kr = 0, count25Kr = 0;

            Graphics g = Graphics.FromImage(processedBmp);

            foreach (var coin in coins)
            {
                // Çapa göre değer belirle
                double val = 0;
                if (coin.Cap <= tbBoyut25.Value) { coin.Deger = "25 Kr"; val = 0.25; count25Kr++; }
                else if (coin.Cap <= tbBoyut50.Value) { coin.Deger = "50 Kr"; val = 0.50; count50Kr++; }
                else { coin.Deger = "1 TL"; val = 1.00; count1TL++; }

                totalValue += val;

                // Doku Analizi ile Yazı/Tura Belirle
                AnalyzeTextureHistogram(coin);

                // Çizim (Görselleştirme)
                Pen pen = new Pen(Color.Red, 3);
                if (coin.Yuz == "TURA") pen.Color = Color.Blue; // Turalar Mavi, Yazılar Kırmızı

                g.DrawEllipse(pen, coin.Merkez.X - coin.Cap / 2, coin.Merkez.Y - coin.Cap / 2, coin.Cap, coin.Cap);

                // Etiket Yaz
                string label = $"{coin.Deger}\n{coin.Yuz}";
                g.DrawString(label, new Font("Arial", 10, FontStyle.Bold), Brushes.Yellow, coin.Merkez.X - 20, coin.Merkez.Y - 10);
                g.FillRectangle(Brushes.Black, coin.Merkez.X - 20, coin.Merkez.Y - 10, 40, 30); // Okunabilirlik için arka plan
                g.DrawString(label, new Font("Arial", 10, FontStyle.Bold), Brushes.Yellow, coin.Merkez.X - 20, coin.Merkez.Y - 10);
            }

            pcbProcessed.Image = processedBmp;
            lblSonuc.Text = $"ANALİZ SONUCU:\n" +
                            $"----------------------\n" +
                            $"Toplam Para Sayısı : {coins.Count}\n" +
                            $"Toplam Tutar       : {totalValue:0.00} TL\n" +
                            $"Detay              : {count1TL} x 1TL, {count50Kr} x 50Kr, {count25Kr} x 25Kr";

            this.Cursor = Cursors.Default;
        }

        // İteratif Flood-Fill ile Blob Tespiti
        private List<CoinObject> FindCoins(Bitmap src, int threshold)
        {
            List<CoinObject> coins = new List<CoinObject>();
            int w = src.Width;
            int h = src.Height;
            bool[,] visited = new bool[w, h];

            for (int x = 0; x < w; x += 4)
            {
                for (int y = 0; y < h; y += 4)
                {
                    if (visited[x, y]) continue;

                    Color c = src.GetPixel(x, y);
                    int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    // Eşik değerinden küçükse (yani koyu renkse/paraysa)
                    if (gray < threshold)
                    {
                        CoinObject coin = FloodFillIterative(src, visited, x, y, threshold);

                        // Çok küçük gürültüleri filtrele (Örn: Çap > 20px)
                        if (coin.Cap > 20)
                        {
                            coins.Add(coin);
                        }
                    }
                }
            }
            return coins;
        }

        private CoinObject FloodFillIterative(Bitmap bmp, bool[,] visited, int startX, int startY, int thres)
        {
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(startX, startY));
            visited[startX, startY] = true;

            int minX = startX, maxX = startX;
            int minY = startY, maxY = startY;

            while (q.Count > 0)
            {
                Point p = q.Dequeue();

                // Bounding Box (Kapsayıcı Kutu) sınırlarını genişlet
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;

                // 4 Komşu piksele bak
                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i];
                    int ny = p.Y + dy[i];

                    // Resim sınırları içinde mi?
                    if (nx >= 0 && nx < bmp.Width && ny >= 0 && ny < bmp.Height)
                    {
                        if (!visited[nx, ny])
                        {
                            Color c = bmp.GetPixel(nx, ny);
                            int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                            // Hala para üzerindeysek (koyu renk)
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

            // Paranın resmini kesip al (Analiz için)
            Bitmap cropped = null;
            try
            {
                Rectangle cropRect = new Rectangle(minX, minY, width, height);
                cropped = bmp.Clone(cropRect, bmp.PixelFormat);
            }
            catch { }

            return new CoinObject { Merkez = center, Cap = diameter, CroppedImage = cropped };
        }

        // Doku ve Kenar Histogram Analizi (Yazı/Tura)
        private void AnalyzeTextureHistogram(CoinObject coin)
        {
            if (coin.CroppedImage == null) return;

            Bitmap img = coin.CroppedImage;
            long edgeScore = 0;
            int pixelCount = 0;

            for (int x = 1; x < img.Width - 1; x++)
            {
                for (int y = 1; y < img.Height - 1; y++)
                {
                    Color c = img.GetPixel(x, y);
                    Color cRight = img.GetPixel(x + 1, y);
                    Color cBottom = img.GetPixel(x, y + 1);

                    int gray = (c.R + c.G + c.B) / 3;
                    int grayR = (cRight.R + cRight.G + cRight.B) / 3;
                    int grayB = (cBottom.R + cBottom.G + cBottom.B) / 3;

                    // Kenar Şiddeti (Gradient)
                    int diff = Math.Abs(gray - grayR) + Math.Abs(gray - grayB);

                    // Eğer belirgin bir kenarsa skora ekle
                    if (diff > 20)
                    {
                        edgeScore += diff;
                    }
                    pixelCount++;
                }
            }

            double normalizedScore = (double)edgeScore / pixelCount;
            coin.DokuSkoru = normalizedScore;

            if (normalizedScore > 5.0)
                coin.Yuz = "TURA";
            else
                coin.Yuz = "YAZI";
        }
    }
}