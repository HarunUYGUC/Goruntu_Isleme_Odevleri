using System;
using System.Drawing;
using System.Drawing.Drawing2D; 
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje11_AcisizDondurmeForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnSaatYonu, btnTersYon, btnGeri;
        private Label lblBilgi, lblAci;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private float currentAngle = 0;

        public Proje11_AcisizDondurmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 11: Yüksek Kaliteli Döndürme";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            lblBilgi = new Label()
            {
                Text = "Resim her açıda orijinal kalitesinde görünür.",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblAci = new Label()
            {
                Text = "Açı: 0°",
                Location = new Point(500, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Blue
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(120, 40) };
            btnYukle.Click += btnYukle_Click;

            btnSaatYonu = new Button() { Text = "Saat Yönü (+10°)", Location = new Point(160, 570), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnSaatYonu.Click += (s, e) => AciyiDegistir(10);

            btnTersYon = new Button() { Text = "Ters Yön (-10°)", Location = new Point(320, 570), Size = new Size(150, 40), BackColor = Color.LightBlue, Enabled = false };
            btnTersYon.Click += (s, e) => AciyiDegistir(-10);

            btnGeri = new Button() { Text = "Menüye Dön", Location = new Point(550, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += btnGeri_Click;

            this.Controls.AddRange(new Control[] { pcbResim, lblBilgi, lblAci, btnYukle, btnSaatYonu, btnTersYon, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje11_AcisizDondurmeForm";
            this.Size = new Size(750, 670);
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

                currentAngle = 0;
                lblAci.Text = "Açı: 0°";

                pcbResim.Image = originalBitmap;

                btnSaatYonu.Enabled = true;
                btnTersYon.Enabled = true;
            }
        }

        private void AciyiDegistir(float delta)
        {
            if (originalBitmap == null) return;

            currentAngle += delta;

            if (currentAngle >= 360) currentAngle -= 360;
            if (currentAngle < 0) currentAngle += 360;

            lblAci.Text = $"Açı: {currentAngle}°";

            pcbResim.Image = RotateImageHighQuality(originalBitmap, currentAngle);
        }

        private Bitmap RotateImageHighQuality(Bitmap bmp, float angle)
        {
            float radyan = angle * (float)Math.PI / 180.0f;
            float cos = (float)Math.Abs(Math.Cos(radyan));
            float sin = (float)Math.Abs(Math.Sin(radyan));

            int newWidth = (int)(bmp.Width * cos + bmp.Height * sin);
            int newHeight = (int)(bmp.Width * sin + bmp.Height * cos);

            Bitmap rotatedBmp = new Bitmap(newWidth, newHeight);

            rotatedBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; 
                g.SmoothingMode = SmoothingMode.HighQuality; 
                g.PixelOffsetMode = PixelOffsetMode.HighQuality; 
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.Clear(Color.Transparent);

                g.TranslateTransform(newWidth / 2.0f, newHeight / 2.0f);

                g.RotateTransform(angle);

                g.TranslateTransform(-bmp.Width / 2.0f, -bmp.Height / 2.0f);

                g.DrawImage(bmp, new Point(0, 0));
            }

            return rotatedBmp;
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