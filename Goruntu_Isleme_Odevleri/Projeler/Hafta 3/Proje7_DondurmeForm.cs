using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje7_DondurmeForm : Form
    {
        private PictureBox pcbResim;
        private Panel pnlContainer; // Resmi tutacak kaydırılabilir panel
        private Button btnYukle, btnDondur, btnGeri;
        private Label lblAcı, lblDurum;
        private TextBox txtAcı;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Point centerPoint = Point.Empty;
        private bool isCenterSelected = false;

        public Proje7_DondurmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 7: Resmi Döndürme (Rotation)";

            // 1. Panel Oluşturma (Scroll özelliği için)
            pnlContainer = new Panel()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                AutoScroll = true // Püf noktası: Otomatik kaydırma çubukları
            };

            // 2. PictureBox Oluşturma
            pcbResim = new PictureBox()
            {
                // Boyut vermiyoruz, AutoSize ile resim kadar büyüyecek
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(0, 0)
            };

            pcbResim.MouseClick += new MouseEventHandler(pcbResim_MouseClick);
            pcbResim.Paint += new PaintEventHandler(pcbResim_Paint);

            // PictureBox'ı Panelin içine ekle
            pnlContainer.Controls.Add(pcbResim);

            int controlsY = 550;

            lblDurum = new Label() { Text = "Lütfen bir resim yükleyin.", Location = new Point(25, controlsY - 20), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblAcı = new Label() { Text = "Dönme Açısı:", Location = new Point(180, controlsY + 10), AutoSize = true };
            txtAcı = new TextBox() { Location = new Point(260, controlsY + 8), Width = 60, Text = "45" };

            btnDondur = new Button() { Text = "Döndür", Location = new Point(340, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnDondur.Click += new EventHandler(btnDondur_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(475, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pnlContainer, lblDurum, btnYukle, lblAcı, txtAcı, btnDondur, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje7_DondurmeForm";
            this.Size = new Size(700, 650);
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
            if (pcbResim.Image == null) return;

            // AutoSize modunda olduğumuz için e.Location DOĞRUDAN resim koordinatıdır.
            // Ekstra matematik işlemine gerek yoktur.
            centerPoint = e.Location;

            isCenterSelected = true;
            btnDondur.Enabled = true;
            lblDurum.Text = $"Merkez: ({centerPoint.X}, {centerPoint.Y}). 'Döndür'e basın.";
            pcbResim.Invalidate();
        }

        private void pcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (isCenterSelected && pcbResim.Image != null)
            {
                int size = 10;
                using (Pen pen = new Pen(Color.Red, 3))
                {
                    // Merkez noktasına '+' işareti çiz
                    e.Graphics.DrawLine(pen, centerPoint.X - size, centerPoint.Y, centerPoint.X + size, centerPoint.Y);
                    e.Graphics.DrawLine(pen, centerPoint.X, centerPoint.Y - size, centerPoint.X, centerPoint.Y + size);
                }
            }
        }

        private void btnDondur_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null || !isCenterSelected) return;

            float angle;
            if (!float.TryParse(txtAcı.Text, out angle))
            {
                MessageBox.Show("Geçerli bir açı girin.");
                return;
            }

            // Resmi kesilmeden (tuvali büyüterek) döndür
            Bitmap rotatedBitmap = RotateImageAndExpand(originalBitmap, centerPoint, angle);

            // Yeni resmi göster
            pcbResim.Image = rotatedBitmap;
            originalBitmap = rotatedBitmap; // Zincirleme işlem için kaydet

            // Merkez noktası resimle birlikte kaydığı için seçimi sıfırla
            isCenterSelected = false;
            btnDondur.Enabled = false;
            lblDurum.Text = "Döndürüldü. Resim büyüdüyse kaydırma çubuklarını kullanın.";
        }

        // --- EN ÖNEMLİ KISIM: KESİLMEYİ ÖNLEYEN ALGORİTMA ---
        private Bitmap RotateImageAndExpand(Bitmap bmp, Point center, float angle)
        {
            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, center); // Seçilen merkez etrafında döndür

            // Resmin 4 köşesini al ve döndürülmüş halini hesapla
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(new Point[] {
                new Point(0, 0),
                new Point(bmp.Width, 0),
                new Point(0, bmp.Height),
                new Point(bmp.Width, bmp.Height)
            });
            gp.Transform(matrix);
            RectangleF bounds = gp.GetBounds(); // Yeni oluşan en geniş sınırları bul

            // Yeni Bitmap'i GENİŞLEMİŞ boyutlarda oluştur (Böylece kesilmez)
            Bitmap rotatedBmp = new Bitmap((int)bounds.Width, (int)bounds.Height);
            rotatedBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                g.Clear(Color.Black); // Arka plan
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;

                // Koordinat sistemini negatif tarafa kayan resmi içeri alacak şekilde ötele
                Matrix drawMatrix = matrix.Clone();
                drawMatrix.Translate(-bounds.X, -bounds.Y, MatrixOrder.Append);

                g.Transform = drawMatrix;
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