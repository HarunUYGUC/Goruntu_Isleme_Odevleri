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
        private Label lblSonuc, lblThresholdVal;
        private Button btnYukle, btnAnaliz, btnGeri;
        private TrackBar tbBrightness;

        private Bitmap originalBitmap;
        private Form haftaFormu;

        // Para sınıfı 
        class CoinInfo
        {
            public Point Center { get; set; }
            public int Diameter { get; set; }
            public string Value { get; set; }
            public Rectangle Bounds { get; set; }
        }

        public Proje4_ParaSaymaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Sade Para Sayma (Boyut Analizi)";
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


            GroupBox grp = new GroupBox() { Text = "Ayarlar", Location = new Point(ctrlX, 70), Size = new Size(130, 100) };
            Label l1 = new Label() { Text = "Zemin Eşiği:", Location = new Point(5, 20), AutoSize = true };
            tbBrightness = new TrackBar() { Location = new Point(5, 40), Size = new Size(120, 45), Maximum = 255, Value = 180, TickFrequency = 20 };
            lblThresholdVal = new Label() { Text = "180", Location = new Point(80, 20), AutoSize = true };

            tbBrightness.Scroll += (s, e) => lblThresholdVal.Text = tbBrightness.Value.ToString();
            grp.Controls.AddRange(new Control[] { l1, tbBrightness, lblThresholdVal });

            btnAnaliz = new Button() { Text = "HESAPLA", Location = new Point(ctrlX, 190), Size = new Size(130, 50), BackColor = Color.LightGreen, Font = new Font("Arial", 11, FontStyle.Bold) };
            btnAnaliz.Click += BtnAnaliz_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX, 260), Size = new Size(130, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            lblSonuc = new Label() { Text = "Sonuçlar...", Location = new Point(margin, 480), Size = new Size(920, 160), Font = new Font("Consolas", 14), ForeColor = Color.DarkBlue, BorderStyle = BorderStyle.FixedSingle };

            this.Controls.AddRange(new Control[] { pcbOriginal, pcbProcessed, btnYukle, grp, btnAnaliz, btnGeri, lblSonuc });
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
                lblSonuc.Text = "Resim yüklendi. 'HESAPLA' butonuna basın.";
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
                lblSonuc.Text = "Para bulunamadı! Zemin eşiğini değiştirip tekrar deneyin.";
                this.Cursor = Cursors.Default;
                return;
            }

            // 2. Boyut Analizi (Kümeleme)
            int minD = coins.Min(c => c.Diameter);
            int maxD = coins.Max(c => c.Diameter);
            int range = maxD - minD;

            foreach (var c in coins)
            {
                // Boyut farkı yoksa hepsi 1 TL varsay
                if (range < maxD * 0.15)
                {
                    c.Value = "1 TL";
                }
                else
                {
                    // Oransal boyutlandırma
                    double relativeSize = (double)(c.Diameter - minD) / range;
                    if (relativeSize < 0.33) c.Value = "25 Kr";
                    else if (relativeSize < 0.70) c.Value = "50 Kr";
                    else c.Value = "1 TL";
                }
            }

            // 3. Çizim ve Hesaplama
            Bitmap resBmp = new Bitmap(originalBitmap);
            Graphics g = Graphics.FromImage(resBmp);
            double total = 0;
            int c1 = 0, c50 = 0, c25 = 0;

            foreach (var c in coins)
            {
                if (c.Value == "1 TL") { total += 1.0; c1++; }
                else if (c.Value == "50 Kr") { total += 0.50; c50++; }
                else if (c.Value == "25 Kr") { total += 0.25; c25++; }

                // Basit Kırmızı Daire
                g.DrawEllipse(new Pen(Color.Red, 4), c.Bounds);

                // Sadece Değeri Yaz
                g.DrawString(c.Value, new Font("Arial", 12, FontStyle.Bold), Brushes.Yellow, c.Center.X - 20, c.Center.Y - 10);
            }

            pcbProcessed.Image = resBmp;
            lblSonuc.Text = $"TOPLAM TUTAR : {total:0.00} TL\n" +
                            $"----------------------------\n" +
                            $"1 TL Adedi   : {c1}\n" +
                            $"50 Kr Adedi  : {c50}\n" +
                            $"25 Kr Adedi  : {c25}";

            this.Cursor = Cursors.Default;
        }

        // --- GÖRÜNTÜ İŞLEME (LockBits) ---
        private List<CoinInfo> FindBlobs(Bitmap src, int threshold)
        {
            List<CoinInfo> blobs = new List<CoinInfo>();
            int w = src.Width;
            int h = src.Height;
            bool[,] visited = new bool[w, h];

            BitmapData data = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            int bytes = stride * h;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(data.Scan0, buffer, 0, bytes);
            src.UnlockBits(data);

            // 4'er atlayarak hızlı tara
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

                // 2 piksel atlayarak yayıl
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
            // Çok küçükleri ele (Gürültü)
            if (dia < 20 || count < 100) return null;

            return new CoinInfo
            {
                Bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY),
                Center = new Point((minX + maxX) / 2, (minY + maxY) / 2),
                Diameter = dia
            };
        }
    }
}