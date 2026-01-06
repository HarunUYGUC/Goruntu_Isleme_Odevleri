using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_BuzluCamForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnGeri;
        private Label lblBilgi;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private List<Point> selectionPoints = new List<Point>();
        private bool isSelecting = false;

        private const int BLUR_SIZE = 15;

        public Proje3_BuzluCamForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Bölgesel Bulanıklaştırma (Buzlu Cam)";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };

            pcbResim.MouseDown += PcbResim_MouseDown;
            pcbResim.Paint += PcbResim_Paint;

            lblBilgi = new Label()
            {
                Text = "Sol Tık: Nokta Ekle | Sağ Tık: Alanı Kapat ve Bulanıklaştır",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(575, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, lblBilgi, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_BuzluCamForm";
            this.Size = new Size(770, 670);
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
                pcbResim.Image = originalBitmap;
                selectionPoints.Clear();
            }
        }

        private void PcbResim_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;

            if (e.Button == MouseButtons.Left)
            {
                selectionPoints.Add(e.Location);
                pcbResim.Invalidate();
            }
            else if (e.Button == MouseButtons.Right && selectionPoints.Count > 2)
            {
                // Sağ tıklandığında ve en az 3 nokta varsa alanı kapat ve uygula
                ApplyBlurToPolygon();
                selectionPoints.Clear();
                pcbResim.Invalidate();
            }
        }

        private void PcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (selectionPoints.Count > 1)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    pen.DashStyle = DashStyle.Dash;
                    // Mevcut noktalar arasına çizgi çek
                    e.Graphics.DrawLines(pen, selectionPoints.ToArray());

                    // Eğer 2'den fazla nokta varsa son noktayı ilk noktaya hayali bağla
                    if (selectionPoints.Count > 2)
                    {
                        e.Graphics.DrawLine(pen, selectionPoints.Last(), selectionPoints.First());
                    }
                }
            }

            // Noktaları belirginleştir
            foreach (var pt in selectionPoints)
            {
                e.Graphics.FillEllipse(Brushes.Yellow, pt.X - 3, pt.Y - 3, 6, 6);
            }
        }

        private void ApplyBlurToPolygon()
        {
            if (originalBitmap == null || selectionPoints.Count < 3) return;

            // Ekran koordinatlarını resim koordinatlarına çevir
            List<Point> realPoints = selectionPoints.Select(p => GetImagePoint(p)).ToList();

            // Seçili alanı kapsayan bir GraphicsPath ve Region oluştur (İçeride mi kontrolü için)
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(realPoints.ToArray());
            Region region = new Region(path);

            // İşlem yapılacak sınırları belirle (Performans için)
            Rectangle bounds = Rectangle.Round(path.GetBounds());

            Bitmap tempBmp = new Bitmap(originalBitmap);
            int radius = BLUR_SIZE / 2;

            for (int y = bounds.Top; y < bounds.Bottom; y++)
            {
                for (int x = bounds.Left; x < bounds.Right; x++)
                {
                    // Sınır ve "Nokta Çokgenin içinde mi?" kontrolü
                    if (x >= 0 && x < originalBitmap.Width && y >= 0 && y < originalBitmap.Height)
                    {
                        if (region.IsVisible(x, y))
                        {
                            int rSum = 0, gSum = 0, bSum = 0, count = 0;

                            for (int ky = -radius; ky <= radius; ky++)
                            {
                                for (int kx = -radius; kx <= radius; kx++)
                                {
                                    int pX = x + kx;
                                    int pY = y + ky;

                                    if (pX >= 0 && pX < tempBmp.Width && pY >= 0 && pY < tempBmp.Height)
                                    {
                                        Color p = tempBmp.GetPixel(pX, pY);
                                        rSum += p.R;
                                        gSum += p.G;
                                        bSum += p.B;
                                        count++;
                                    }
                                }
                            }
                            originalBitmap.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                        }
                    }
                }
            }

            pcbResim.Image = originalBitmap;
        }

        // Tek bir noktayı resim koordinatına çeviren yardımcı fonksiyon
        private Point GetImagePoint(Point pcbPoint)
        {
            Size pcbSize = pcbResim.ClientSize;
            Size imgSize = pcbResim.Image.Size;
            float scale;
            float offsetX = 0, offsetY = 0;

            if ((float)pcbSize.Width / pcbSize.Height > (float)imgSize.Width / imgSize.Height)
            {
                scale = (float)pcbSize.Height / imgSize.Height;
                offsetX = (pcbSize.Width - imgSize.Width * scale) / 2;
            }
            else
            {
                scale = (float)pcbSize.Width / imgSize.Width;
                offsetY = (pcbSize.Height - imgSize.Height * scale) / 2;
            }

            int realX = (int)((pcbPoint.X - offsetX) / scale);
            int realY = (int)((pcbPoint.Y - offsetY) / scale);

            return new Point(realX, realY);
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