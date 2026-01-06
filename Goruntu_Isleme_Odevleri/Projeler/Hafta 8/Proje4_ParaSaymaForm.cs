using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_ParaSaymaForm : Form
    {
        private PictureBox pcbOriginal, pcbProcessed;
        private Label lblSonuc, lblThresholds;
        private Button btnYukle, btnAnaliz, btnGeri;

        // Ayarlar
        private TrackBar tbBrightness; // Zemin ayrımı
        private TrackBar tbTexture;    // Doku hassasiyeti (Standart Sapma Eşiği)

        private Bitmap originalBitmap;
        private Form haftaFormu;

        class CoinInfo
        {
            public Point Center { get; set; }
            public int Diameter { get; set; }
            public string Value { get; set; }
            public string Face { get; set; }
            public double TextureScore { get; set; }
            public Rectangle Bounds { get; set; }
        }

        public Proje4_ParaSaymaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Gelişmiş Doku Analizi ile Para Sayma";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            SetupUI();
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void InitializeComponent() { this.Name = "Proje4_ParaSaymaForm"; }

        private void SetupUI()
        {
            int margin = 20;
            pcbOriginal = new PictureBox() { Location = new Point(margin, 20), Size = new Size(450, 450), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom };
            pcbProcessed = new PictureBox() { Location = new Point(margin + 470, 20), Size = new Size(450, 450), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom };

            int ctrlX = 940;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(ctrlX, 20), Size = new Size(130, 40), BackColor = Color.LightBlue };
            btnYukle.Click += BtnYukle_Click;

            GroupBox grp = new GroupBox() { Text = "Hassasiyet Ayarları", Location = new Point(ctrlX, 70), Size = new Size(130, 220) };

            Label l1 = new Label() { Text = "Zemin Eşiği:", Location = new Point(5, 20), AutoSize = true };
            tbBrightness = new TrackBar() { Location = new Point(5, 40), Size = new Size(120, 45), Maximum = 255, Value = 180, TickFrequency = 20 };

            Label l2 = new Label() { Text = "Doku Eşiği (StdDev):", Location = new Point(5, 90), AutoSize = true };
            tbTexture = new TrackBar() { Location = new Point(5, 110), Size = new Size(120, 45), Minimum = 5, Maximum = 80, Value = 25, TickFrequency = 5 };

            lblThresholds = new Label() { Text = "", Location = new Point(5, 160), AutoSize = true };

            tbBrightness.Scroll += (s, e) => UpdateLabels();
            tbTexture.Scroll += (s, e) => UpdateLabels();
            UpdateLabels();

            grp.Controls.AddRange(new Control[] { l1, tbBrightness, l2, tbTexture, lblThresholds });

            btnAnaliz = new Button() { Text = "ANALİZ ET", Location = new Point(ctrlX, 300), Size = new Size(130, 50), BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnAnaliz.Click += BtnAnaliz_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX, 370), Size = new Size(130, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            lblSonuc = new Label() { Text = "Sonuçlar...", Location = new Point(margin, 480), Size = new Size(920, 160), Font = new Font("Consolas", 12), ForeColor = Color.DarkBlue, BorderStyle = BorderStyle.FixedSingle };

            this.Controls.AddRange(new Control[] { pcbOriginal, pcbProcessed, btnYukle, grp, btnAnaliz, btnGeri, lblSonuc });
        }

        private void UpdateLabels()
        {
            lblThresholds.Text = $"Zemin: {tbBrightness.Value}\nDoku: {tbTexture.Value}";
        }

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap temp = new Bitmap(ofd.FileName);
                float scale = Math.Min(800f / temp.Width, 800f / temp.Height);
                if (scale > 1) scale = 1;
                originalBitmap = new Bitmap(temp, new Size((int)(temp.Width * scale), (int)(temp.Height * scale)));

                if (originalBitmap.PixelFormat != PixelFormat.Format24bppRgb)
                {
                    Bitmap clone = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(clone)) g.DrawImage(originalBitmap, 0, 0);
                    originalBitmap = clone;
                }

                pcbOriginal.Image = originalBitmap;
                pcbProcessed.Image = null;
                lblSonuc.Text = "Resim yüklendi. Ayarları yapıp 'Analiz Et' butonuna basınız.";
            }
        }

        private void BtnAnaliz_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // 1. Paraları Bul
            List<CoinInfo> coins = FindBlobs(originalBitmap, tbBrightness.Value);

            if (coins.Count == 0)
            {
                lblSonuc.Text = "Para bulunamadı! Zemin eşiğini düşürerek tekrar deneyin.";
                this.Cursor = Cursors.Default;
                return;
            }

            // 2. Değer Belirleme (Boyut)
            int minD = coins.Min(c => c.Diameter);
            int maxD = coins.Max(c => c.Diameter);
            int range = maxD - minD;

            foreach (var c in coins)
            {
                // Doku Analizi 
                CalculateStdDevTexture(originalBitmap, c);

                // YAZI / TURA KARARI
                // Eşik değerinin üstü -> Karmaşık -> TURA
                // Eşik değerinin altı -> Düz -> YAZI
                if (c.TextureScore > tbTexture.Value)
                    c.Face = "TURA";
                else
                    c.Face = "YAZI";

                // DEĞER KARARI
                if (range < maxD * 0.15)
                {
                    c.Value = "1 TL";
                }
                else
                {
                    double relativeSize = (double)(c.Diameter - minD) / range;
                    if (relativeSize < 0.33) c.Value = "25 Kr";
                    else if (relativeSize < 0.70) c.Value = "50 Kr";
                    else c.Value = "1 TL";
                }
            }

            // 3. Sonuç Çizimi
            Bitmap resBmp = new Bitmap(originalBitmap);
            Graphics g = Graphics.FromImage(resBmp);
            double total = 0;
            int cHead = 0, cTail = 0;

            foreach (var c in coins)
            {
                if (c.Value == "1 TL") total += 1.0;
                else if (c.Value == "50 Kr") total += 0.50;
                else if (c.Value == "25 Kr") total += 0.25;

                if (c.Face == "TURA") cTail++; else cHead++;

                Pen p = (c.Face == "TURA") ? new Pen(Color.Blue, 4) : new Pen(Color.Red, 4);
                g.DrawEllipse(p, c.Bounds);

                string info = $"{c.Value}\n{c.Face}\n(S:{c.TextureScore:0.0})";
                g.DrawString(info, new Font("Arial", 10, FontStyle.Bold), Brushes.Yellow, c.Center.X - 25, c.Center.Y - 20);
            }

            pcbProcessed.Image = resBmp;
            lblSonuc.Text = $"SONUÇ:\n" +
                            $"Toplam Tutar : {total:0.00} TL\n" +
                            $"Para Sayısı  : {coins.Count}\n" +
                            $"Yazı (Kırmızı): {cHead} | Tura (Mavi): {cTail}\n" +
                            $"* Yazı/Tura yanlışsa 'Doku Eşiği' ile oynayın (Puanlara göre).";

            this.Cursor = Cursors.Default;
        }

        private List<CoinInfo> FindBlobs(Bitmap src, int threshold)
        {
            List<CoinInfo> blobs = new List<CoinInfo>();
            int w = src.Width;
            int h = src.Height; // Düzeltildi
            bool[,] visited = new bool[w, h];

            BitmapData data = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            int bytes = stride * h;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(data.Scan0, buffer, 0, bytes);
            src.UnlockBits(data);

            for (int y = 0; y < h; y += 4)
            {
                for (int x = 0; x < w; x += 4)
                {
                    if (visited[x, y]) continue;
                    int idx = y * stride + x * 3;
                    if (idx >= bytes - 3) continue;

                    int gray = (buffer[idx] + buffer[idx + 1] + buffer[idx + 2]) / 3;

                    if (gray < threshold)
                    {
                        CoinInfo coin = ExtractBlob(buffer, w, h, stride, visited, x, y, threshold);
                        if (coin != null) blobs.Add(coin);
                    }
                }
            }
            return blobs;
        }

        private CoinInfo ExtractBlob(byte[] pix, int w, int h, int stride, bool[,] visit, int sx, int sy, int thres)
        {
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(sx, sy));
            visit[sx, sy] = true;

            int minX = sx, maxX = sx, minY = sy, maxY = sy;
            int count = 0;

            while (q.Count > 0)
            {
                Point p = q.Dequeue();
                count++;
                if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;

                int[] dx = { 2, -2, 0, 0 };
                int[] dy = { 0, 0, 2, -2 };

                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i];
                    int ny = p.Y + dy[i];

                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && !visit[nx, ny])
                    {
                        int idx = ny * stride + nx * 3;
                        if (idx >= 0 && idx < pix.Length - 3)
                        {
                            if ((pix[idx] + pix[idx + 1] + pix[idx + 2]) / 3 < thres)
                            {
                                visit[nx, ny] = true;
                                q.Enqueue(new Point(nx, ny));
                            }
                        }
                    }
                }
            }

            int dia = Math.Max(maxX - minX, maxY - minY);
            if (dia < 20 || count < 100) return null;

            return new CoinInfo
            {
                Bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY),
                Center = new Point((minX + maxX) / 2, (minY + maxY) / 2),
                Diameter = dia
            };
        }

        // --- YENİ DOKU ANALİZİ (STANDART SAPMA) ---
        private void CalculateStdDevTexture(Bitmap src, CoinInfo coin)
        {
            // Paranın sadece orta kısmını al (%50'si)
            // Kenardaki yazılar ve çerçeve yanıltmasın diye
            int innerW = coin.Bounds.Width / 2;
            int innerH = coin.Bounds.Height / 2;
            int startX = coin.Center.X - innerW / 2;
            int startY = coin.Center.Y - innerH / 2;

            // Güvenlik
            if (startX < 0) startX = 0; if (startY < 0) startY = 0;
            if (startX + innerW > src.Width) innerW = src.Width - startX;
            if (startY + innerH > src.Height) innerH = src.Height - startY;

            BitmapData data = src.LockBits(new Rectangle(startX, startY, innerW, innerH), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            int bytes = stride * innerH;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(data.Scan0, buffer, 0, bytes);
            src.UnlockBits(data);

            // 1. Ortalama Parlaklığı Bul
            double sum = 0;
            int count = 0;
            for (int i = 0; i < buffer.Length; i += 3)
            {
                sum += (buffer[i] + buffer[i + 1] + buffer[i + 2]) / 3.0; // Gri değer
                count++;
            }
            double mean = sum / count;

            // 2. Varyansı (Değişkenliği) Bul
            double sumSqDiff = 0;
            for (int i = 0; i < buffer.Length; i += 3)
            {
                double gray = (buffer[i] + buffer[i + 1] + buffer[i + 2]) / 3.0;
                sumSqDiff += Math.Pow(gray - mean, 2);
            }

            // 3. Standart Sapma
            double stdDev = Math.Sqrt(sumSqDiff / count);

            coin.TextureScore = stdDev;
        }
    }
}