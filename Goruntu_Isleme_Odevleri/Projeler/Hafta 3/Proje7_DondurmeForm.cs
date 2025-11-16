using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje7_DondurmeForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnDondur, btnGeri;
        private Label lblAcı, lblDurum;
        private TextBox txtAcı;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Point centerPoint = Point.Empty; // Döndürme merkezi
        private bool isCenterSelected = false;

        public Proje7_DondurmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 7: Resmi Döndürme (Rotation)";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pcbResim.MouseClick += new MouseEventHandler(pcbResim_MouseClick);
            pcbResim.Paint += new PaintEventHandler(pcbResim_Paint);

            int controlsY = 550;

            lblDurum = new Label() { Text = "Lütfen bir resim yükleyin.", Location = new Point(25, controlsY - 20), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblAcı = new Label() { Text = "Dönme Açısı (Derece):", Location = new Point(180, controlsY + 10), AutoSize = true };
            txtAcı = new TextBox() { Location = new Point(310, controlsY + 8), Width = 60, Text = "45" };

            btnDondur = new Button() { Text = "Döndür", Location = new Point(390, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnDondur.Click += new EventHandler(btnDondur_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(550, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, lblDurum, btnYukle, lblAcı, txtAcı, btnDondur, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje7_DondurmeForm";
            this.Size = new Size(750, 650);
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
                ResetSelection();
                lblDurum.Text = "Döndürme merkezini belirlemek için resme tıklayın.";
            }
        }

        private void pcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;

            // Tıklanan noktayı resmin gerçek koordinatına çevir
            centerPoint = ConvertPointToImage(e.Location);
            isCenterSelected = true;
            btnDondur.Enabled = true;
            lblDurum.Text = $"Merkez seçildi: ({centerPoint.X}, {centerPoint.Y}). Açıyı girip 'Döndür'e basın.";
            pcbResim.Invalidate(); // Artı işaretini çizdir
        }

        private void pcbResim_Paint(object sender, PaintEventArgs e)
        {
            // Seçilen merkeze "+" işareti çiz
            if (isCenterSelected && originalBitmap != null)
            {
                Point pcbPoint = ConvertPointFromImage(centerPoint);
                int size = 10;
                using (Pen pen = new Pen(Color.Red, 3))
                {
                    e.Graphics.DrawLine(pen, pcbPoint.X - size, pcbPoint.Y, pcbPoint.X + size, pcbPoint.Y);
                    e.Graphics.DrawLine(pen, pcbPoint.X, pcbPoint.Y - size, pcbPoint.X, pcbPoint.Y + size);
                }
            }
        }

        private void btnDondur_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null || !isCenterSelected) return;

            float angle;
            if (!float.TryParse(txtAcı.Text, out angle))
            {
                MessageBox.Show("Lütfen geçerli bir açı değeri girin.");
                return;
            }

            Bitmap rotatedBitmap = RotateImage(originalBitmap, centerPoint, angle);
            pcbResim.Image = rotatedBitmap;

            // İşlemden sonra işareti kaldır ve orijinal resmi güncelle
            isCenterSelected = false;
            originalBitmap = rotatedBitmap; // Zincirleme Döndürme
            lblDurum.Text = "Döndürme tamamlandı. Yeni bir merkez seçebilirsiniz.";
            pcbResim.Invalidate();
        }

        private Bitmap RotateImage(Bitmap bmp, Point center, float angle)
        {
            Bitmap rotatedBmp = new Bitmap(bmp.Width, bmp.Height);
            rotatedBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                // Arka planı temizle 
                g.Clear(Color.Black);

                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                // Dönüşüm Matrisini Ayarla
                // Merkezi orijine (0,0) taşı
                g.TranslateTransform(-center.X, -center.Y, MatrixOrder.Append);
                // Döndür
                g.RotateTransform(angle, MatrixOrder.Append);
                // Merkezi geri taşı
                g.TranslateTransform(center.X, center.Y, MatrixOrder.Append);

                g.DrawImage(bmp, 0, 0);
            }
            return rotatedBmp;
        }

        private void ResetSelection()
        {
            isCenterSelected = false;
            centerPoint = Point.Empty;
            btnDondur.Enabled = false;
            pcbResim.Invalidate();
        }

        // PictureBox koordinat dönüşümleri (Zoom modu için)
        private Point ConvertPointToImage(Point pcbPoint)
        {
            if (pcbResim.Image == null) return pcbPoint;
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
            return new Point((int)((pcbPoint.X - offsetX) / scale), (int)((pcbPoint.Y - offsetY) / scale));
        }

        private Point ConvertPointFromImage(Point imgPoint)
        {
            if (pcbResim.Image == null) return imgPoint;
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
            return new Point((int)(imgPoint.X * scale + offsetX), (int)(imgPoint.Y * scale + offsetY));
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