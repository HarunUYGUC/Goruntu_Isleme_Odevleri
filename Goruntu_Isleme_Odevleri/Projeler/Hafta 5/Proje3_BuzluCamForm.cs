using System;
using System.Drawing;
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
        private Point startPoint;
        private Rectangle selectionRect;
        private bool isSelecting = false;

        // Bulanıklık şiddeti (Kernel boyutu)
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
            pcbResim.MouseMove += PcbResim_MouseMove;
            pcbResim.MouseUp += PcbResim_MouseUp;
            pcbResim.Paint += PcbResim_Paint;

            lblBilgi = new Label()
            {
                Text = "Resim yükleyin ve gizlemek istediğiniz alanı fareyle seçin.",
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
                selectionRect = Rectangle.Empty;
            }
        }

        // Fare Olayları (Seçim) 

        private void PcbResim_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;
            isSelecting = true;
            startPoint = e.Location;
            selectionRect = new Rectangle(e.Location, new Size(0, 0));
            pcbResim.Invalidate();
        }

        private void PcbResim_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting || originalBitmap == null) return;

            int x = Math.Min(startPoint.X, e.X);
            int y = Math.Min(startPoint.Y, e.Y);
            int w = Math.Abs(startPoint.X - e.X);
            int h = Math.Abs(startPoint.Y - e.Y);

            selectionRect = new Rectangle(x, y, w, h);
            pcbResim.Invalidate();
        }

        private void PcbResim_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isSelecting) return;
            isSelecting = false;

            if (selectionRect.Width > 5 && selectionRect.Height > 5)
            {
                ApplyBlurToSelection();
            }
            // Seçim karesini temizle (işlem bittiğinde çizgi kalmasın)
            selectionRect = Rectangle.Empty;
            pcbResim.Invalidate();
        }

        private void PcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
            }
        }

        // Bölgesel Bulanıklaştırma 
        private void ApplyBlurToSelection()
        {
            if (originalBitmap == null) return;

            Rectangle realRect = GetImageRectangle(selectionRect);
            if (realRect.Width <= 0 || realRect.Height <= 0) return;

            // Orijinal resim üzerinde işlem yapacağız
            // Basit ve hızlı bir Mean (Ortalama) filtresi uygulayalım.
            // Performans için tüm resmi değil, sadece seçili alanı işleyeceğiz.

            Bitmap tempBmp = new Bitmap(originalBitmap); // Geçici kopya (kaynak)
            int radius = BLUR_SIZE / 2;

            // Sadece seçili dikdörtgenin içindeki pikselleri gez
            for (int y = realRect.Top; y < realRect.Bottom; y++)
            {
                for (int x = realRect.Left; x < realRect.Right; x++)
                {
                    // Güvenlik kontrolü (resim sınırları)
                    if (x < 0 || x >= originalBitmap.Width || y < 0 || y >= originalBitmap.Height) continue;

                    int rSum = 0, gSum = 0, bSum = 0, count = 0;

                    // Çekirdek (Kernel) Döngüsü
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

                    // Ortalamayı al ve orijinal resme yaz
                    originalBitmap.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                }
            }

            pcbResim.Image = originalBitmap;
        }

        private Rectangle GetImageRectangle(Rectangle pcbRect)
        {
            if (pcbResim.Image == null) return Rectangle.Empty;
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

            int realX = (int)((pcbRect.X - offsetX) / scale);
            int realY = (int)((pcbRect.Y - offsetY) / scale);
            int realW = (int)(pcbRect.Width / scale);
            int realH = (int)(pcbRect.Height / scale);

            realX = Math.Max(0, realX);
            realY = Math.Max(0, realY);
            if (realX + realW > imgSize.Width) realW = imgSize.Width - realX;
            if (realY + realH > imgSize.Height) realH = imgSize.Height - realY;

            return new Rectangle(realX, realY, realW, realH);
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