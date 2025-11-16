using System;
using System.Drawing;
using System.Drawing.Drawing2D; // İnterpolasyon için gerekli
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje9_InterpolasyonForm : Form
    {
        private PictureBox pcbNearest, pcbBilinear;
        private Panel pnlNearest, pnlBilinear;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblOran, lblNearest, lblBilinear;
        private NumericUpDown nudOran;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        // Eş zamanlı kaydırma için
        private bool isScrolling = false;

        public Proje9_InterpolasyonForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 9: Yeniden Örnekleme Karşılaştırması";

            // Nearest Neighbor Paneli ve Kutusu
            lblNearest = new Label() { Text = "Piksel Değiştirme (Nearest Neighbor)", Location = new Point(25, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pnlNearest = new Panel()
            {
                Location = new Point(25, 45),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            pnlNearest.Scroll += Panel_Scroll; // Kaydırma olayı

            pcbNearest = new PictureBox()
            {
                Location = new Point(0, 0),
                Size = new Size(400, 400),
                SizeMode = PictureBoxSizeMode.AutoSize
            };
            pnlNearest.Controls.Add(pcbNearest);

            // Bilinear Interpolation Paneli ve Kutusu
            lblBilinear = new Label() { Text = "Piksel İnterpolasyon (Bilinear)", Location = new Point(450, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pnlBilinear = new Panel()
            {
                Location = new Point(450, 45),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            pnlBilinear.Scroll += Panel_Scroll; // Kaydırma olayı

            pcbBilinear = new PictureBox()
            {
                Location = new Point(0, 0),
                Size = new Size(400, 400),
                SizeMode = PictureBoxSizeMode.AutoSize
            };
            pnlBilinear.Controls.Add(pcbBilinear);

            int controlsY = 470;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblOran = new Label() { Text = "Büyütme Katsayısı:", Location = new Point(200, controlsY + 12), AutoSize = true };
            nudOran = new NumericUpDown() { Location = new Point(310, controlsY + 10), Width = 60, Minimum = 2, Maximum = 10, Value = 3 }; // Farkı görmek için en az 3x önerilir

            btnUygula = new Button() { Text = "Karşılaştır", Location = new Point(400, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(700, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblNearest, pnlNearest, lblBilinear, pnlBilinear, btnYukle, lblOran, nudOran, btnUygula, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje9_InterpolasyonForm";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        // Eş Zamanlı Kaydırma Mantığı
        private void Panel_Scroll(object sender, ScrollEventArgs e)
        {
            if (isScrolling) return; // Sonsuz döngüyü engelle
            isScrolling = true;

            Panel source = (Panel)sender;
            Panel target = (source == pnlNearest) ? pnlBilinear : pnlNearest;

            // Diğer panelin kaydırma pozisyonunu eşitle
            target.HorizontalScroll.Value = Math.Min(source.HorizontalScroll.Value, target.HorizontalScroll.Maximum);
            target.VerticalScroll.Value = Math.Min(source.VerticalScroll.Value, target.VerticalScroll.Maximum);

            // Kaydırma pozisyonunu güncellemek için paneli yeniden çizmeye zorla
            target.PerformLayout();

            isScrolling = false;
        }

        private void btnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(dialog.FileName);
                // Resmi her iki kutuya da yükle (başlangıçta orijinali göster)
                pcbNearest.Image = originalBitmap;
                pcbBilinear.Image = originalBitmap;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            int scaleFactor = (int)nudOran.Value;
            int newWidth = originalBitmap.Width * scaleFactor;
            int newHeight = originalBitmap.Height * scaleFactor;

            // Nearest Neighbor (Piksel Değiştirme)
            Bitmap bmpNearest = new Bitmap(newWidth, newHeight);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = x / scaleFactor;
                    int srcY = y / scaleFactor;
                    // Sınır kontrolü
                    if (srcX >= originalBitmap.Width) srcX = originalBitmap.Width - 1;
                    if (srcY >= originalBitmap.Height) srcY = originalBitmap.Height - 1;

                    bmpNearest.SetPixel(x, y, originalBitmap.GetPixel(srcX, srcY));
                }
            }
            pcbNearest.Image = bmpNearest;

            // Bilinear Interpolation (Piksel İnterpolasyon)
            Bitmap bmpBilinear = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(bmpBilinear))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;

                // Kenar yumuşatma ve piksel kaydırma ayarları
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                g.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);
            }
            pcbBilinear.Image = bmpBilinear;
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