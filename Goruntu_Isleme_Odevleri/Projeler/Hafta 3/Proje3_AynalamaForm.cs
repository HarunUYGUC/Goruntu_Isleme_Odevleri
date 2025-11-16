using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_AynalamaForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnAynala, btnTemizle, btnGeri;
        private Label lblDurum;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap displayBitmap; // Çizgileri göstermek için geçici bitmap

        private Point p1 = Point.Empty;
        private Point p2 = Point.Empty;
        private bool isSelecting = true;

        public Proje3_AynalamaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Eksen Etrafında Aynalama";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pcbResim.MouseClick += new MouseEventHandler(pcbResim_MouseClick);

            lblDurum = new Label() { Text = "Lütfen bir resim yükleyin.", Location = new Point(25, 540), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnAynala = new Button() { Text = "Aynala", Location = new Point(155, 570), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnAynala.Click += new EventHandler(btnAynala_Click);

            btnTemizle = new Button() { Text = "Noktaları Sıfırla", Location = new Point(285, 570), Size = new Size(120, 40) };
            btnTemizle.Click += new EventHandler(btnTemizle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(575, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, lblDurum, btnYukle, btnAynala, btnTemizle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_AynalamaForm";
            this.Size = new Size(765, 670);
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
                ResetSelection();
            }
        }

        private void pcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || !isSelecting) return;

            Point imagePoint = ConvertPointToImage(e.Location);

            if (p1.IsEmpty)
            {
                p1 = imagePoint;
                lblDurum.Text = "Birinci nokta seçildi. İkinci noktayı seçin.";
                DrawPointsAndLine();
            }
            else if (p2.IsEmpty)
            {
                p2 = imagePoint;
                lblDurum.Text = "İki nokta seçildi. 'Aynala' butonuna basın.";
                DrawPointsAndLine();
                btnAynala.Enabled = true;
                isSelecting = false;
            }
        }

        private void DrawPointsAndLine()
        {
            displayBitmap = new Bitmap(originalBitmap);
            using (Graphics g = Graphics.FromImage(displayBitmap))
            {
                using (Pen pen = new Pen(Color.Red, 3))
                using (Brush brush = new SolidBrush(Color.Yellow))
                {
                    if (!p1.IsEmpty)
                    {
                        g.FillEllipse(brush, p1.X - 5, p1.Y - 5, 10, 10);
                    }
                    if (!p2.IsEmpty)
                    {
                        g.FillEllipse(brush, p2.X - 5, p2.Y - 5, 10, 10);
                        g.DrawLine(pen, p1, p2);
                    }
                }
            }
            pcbResim.Image = displayBitmap;
        }

        private void btnAynala_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null || p1.IsEmpty || p2.IsEmpty) return;

            this.Cursor = Cursors.WaitCursor;

            // 1. Eksenin Açısını ve Merkezini Bulma
            // P1 noktasını (x0, y0) olarak kabul edelim.
            double x0 = p1.X;
            double y0 = p1.Y;

            // Açıyı hesapla (Radyan cinsinden)
            double theta = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            double sinT = Math.Sin(theta);
            double cosT = Math.Cos(theta);

            Bitmap mirrorBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            // INVERSE MAPPING (Ters Eşleme) Yöntemi
            for (int y = 0; y < mirrorBitmap.Height; y++)
            {
                for (int x = 0; x < mirrorBitmap.Width; x++)
                {
                    double delta = (x - x0) * sinT - (y - y0) * cosT;

                    int srcX = (int)(x - 2 * delta * sinT);
                    int srcY = (int)(y + 2 * delta * cosT);

                    // Eğer hesaplanan kaynak koordinat resim sınırları içindeyse rengi al
                    if (srcX >= 0 && srcX < originalBitmap.Width && srcY >= 0 && srcY < originalBitmap.Height)
                    {
                        Color c = originalBitmap.GetPixel(srcX, srcY);
                        mirrorBitmap.SetPixel(x, y, c);
                    }
                    else
                    {
                        mirrorBitmap.SetPixel(x, y, Color.Black);
                    }
                }
            }

            pcbResim.Image = mirrorBitmap;

            this.Cursor = Cursors.Default;
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            ResetSelection();
        }

        private void ResetSelection()
        {
            p1 = Point.Empty;
            p2 = Point.Empty;
            isSelecting = true;
            btnAynala.Enabled = false;

            if (originalBitmap != null)
            {
                pcbResim.Image = originalBitmap;
                lblDurum.Text = "Birinci noktayı seçmek için resme tıklayın.";
            }
            else
            {
                lblDurum.Text = "Lütfen bir resim yükleyin.";
            }
        }

        private Point ConvertPointToImage(Point pcbPoint)
        {
            if (pcbResim.Image == null) return pcbPoint;

            int imgWidth = originalBitmap.Width;
            int imgHeight = originalBitmap.Height;

            float pcbAspect = (float)pcbResim.ClientSize.Width / pcbResim.ClientSize.Height;
            float imgAspect = (float)imgWidth / imgHeight;

            if (pcbAspect > imgAspect)
            {
                float scale = (float)pcbResim.ClientSize.Height / imgHeight;
                float offsetX = (pcbResim.ClientSize.Width - imgWidth * scale) / 2f;
                return new Point((int)((pcbPoint.X - offsetX) / scale), (int)(pcbPoint.Y / scale));
            }
            else
            {
                float scale = (float)pcbResim.ClientSize.Width / imgWidth;
                float offsetY = (pcbResim.ClientSize.Height - imgHeight * scale) / 2f;
                return new Point((int)(pcbPoint.X / scale), (int)((pcbPoint.Y - offsetY) / scale));
            }
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