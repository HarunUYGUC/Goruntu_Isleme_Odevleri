using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_CevizFindikForm : Form
    {
        private PictureBox pcbOriginal, pcbProcessed;
        private Label lblOriginal, lblProcessed, lblInfo;
        private Button btnYukle, btnAnaliz, btnGeri;
        private TrackBar tbThreshold, tbSizeLimit;
        private Label lblThreshold, lblSizeLimit;

        private Bitmap originalBitmap;

        private Form haftaFormu;

        class BlobInfo
        {
            public int Alan { get; set; }
            public Point Merkez { get; set; }
        }

        public Proje1_CevizFindikForm(Form parentForm)
        {
            InitializeComponent();

            haftaFormu = parentForm;

            this.Text = "Proje 1: Ceviz/Fındık Sayma ve Sınıflandırma";
            SetupUI();

            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }

        private void SetupUI()
        {
            int margin = 20;
            int pcbSize = 350;

            lblOriginal = new Label() { Text = "Orijinal Görüntü", Location = new Point(margin, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            lblProcessed = new Label() { Text = "Sınıflandırılmış Görüntü", Location = new Point(margin + pcbSize + 20, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbProcessed = new PictureBox() { Location = new Point(margin + pcbSize + 20, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            int ctrlY = margin + pcbSize + 40;

            btnYukle = new Button() { Text = "Fotoğraf Yükle", Location = new Point(margin, ctrlY), Size = new Size(150, 40) };
            btnYukle.Click += BtnYukle_Click;

            // Eşik Ayarı
            lblThreshold = new Label() { Text = "Siyah/Beyaz Eşiği: 128", Location = new Point(margin + 170, ctrlY - 15), AutoSize = true };
            tbThreshold = new TrackBar() { Location = new Point(margin + 170, ctrlY + 5), Size = new Size(150, 45), Maximum = 255, Value = 128 };
            tbThreshold.Scroll += (s, e) => lblThreshold.Text = $"Siyah/Beyaz Eşiği: {tbThreshold.Value}";

            // Boyut Ayarı
            lblSizeLimit = new Label() { Text = "Ceviz Boyut Sınırı: 500", Location = new Point(margin + 340, ctrlY - 15), AutoSize = true };
            tbSizeLimit = new TrackBar() { Location = new Point(margin + 340, ctrlY + 5), Size = new Size(150, 45), Maximum = 2000, Minimum = 100, Value = 500, TickFrequency = 100 };
            tbSizeLimit.Scroll += (s, e) => lblSizeLimit.Text = $"Ceviz Boyut Sınırı: {tbSizeLimit.Value}";

            btnAnaliz = new Button() { Text = "ANALİZ ET", Location = new Point(margin + 510, ctrlY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnAnaliz.Click += BtnAnaliz_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(margin + 650, ctrlY), Size = new Size(100, 40), BackColor = Color.LightCoral };
            btnGeri.Click += BtnGeri_Click;

            lblInfo = new Label() { Text = "Sonuçlar burada görünecek...", Location = new Point(margin, ctrlY + 60), Size = new Size(700, 30), Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.Blue };

            this.Size = new Size(800, 600);
            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblProcessed, pcbProcessed, btnYukle, lblThreshold, tbThreshold, lblSizeLimit, tbSizeLimit, btnAnaliz, btnGeri, lblInfo });
        }

        private void InitializeComponent() { this.Name = "Proje1_CevizFindikForm"; this.StartPosition = FormStartPosition.CenterScreen; }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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

            Bitmap src = new Bitmap(originalBitmap, new Size(originalBitmap.Width / 2, originalBitmap.Height / 2));
            int w = src.Width;
            int h = src.Height;

            bool[,] visited = new bool[w, h];
            List<BlobInfo> objects = new List<BlobInfo>();
            int threshold = tbThreshold.Value;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!visited[x, y])
                    {
                        Color c = src.GetPixel(x, y);
                        int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                        if (gray < threshold)
                        {
                            BlobInfo blob = FloodFill(src, visited, x, y, threshold);
                            if (blob.Alan > 50)
                            {
                                objects.Add(blob);
                            }
                        }
                    }
                }
            }

            Bitmap dest = new Bitmap(src);
            Graphics g = Graphics.FromImage(dest);
            int cevizCount = 0;
            int findikCount = 0;
            int sizeLimit = tbSizeLimit.Value;

            foreach (var obj in objects)
            {
                Pen kalem;
                string label;

                if (obj.Alan > sizeLimit)
                {
                    cevizCount++;
                    label = "Ceviz";
                    kalem = new Pen(Color.Red, 2);
                }
                else
                {
                    findikCount++;
                    label = "Fındık";
                    kalem = new Pen(Color.Blue, 2);
                }

                int r = (int)Math.Sqrt(obj.Alan / Math.PI);
                g.DrawEllipse(kalem, obj.Merkez.X - r, obj.Merkez.Y - r, r * 2, r * 2);
                g.DrawString(label, new Font("Arial", 8, FontStyle.Bold), Brushes.Yellow, obj.Merkez.X, obj.Merkez.Y);
            }

            pcbProcessed.Image = dest;
            lblInfo.Text = $"Tespit Edilen: {cevizCount} Ceviz, {findikCount} Fındık.";

            this.Cursor = Cursors.Default;
        }

        private BlobInfo FloodFill(Bitmap bmp, bool[,] visited, int startX, int startY, int thres)
        {
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(startX, startY));
            visited[startX, startY] = true;

            int pixelCount = 0;
            long sumX = 0, sumY = 0;
            int w = bmp.Width;
            int h = bmp.Height;

            while (q.Count > 0)
            {
                Point p = q.Dequeue();
                pixelCount++;
                sumX += p.X;
                sumY += p.Y;

                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i];
                    int ny = p.Y + dy[i];

                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
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

            return new BlobInfo
            {
                Alan = pixelCount,
                Merkez = new Point((int)(sumX / Math.Max(1, pixelCount)), (int)(sumY / Math.Max(1, pixelCount)))
            };
        }
    }
}