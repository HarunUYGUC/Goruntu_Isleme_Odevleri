using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje11_AcisizDondurmeForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnSaatYonu, btnTersYon, btnGeri;
        private Label lblBilgi;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap processedBitmap;

        // Alan Paeth'in 3-Shear Döndürme Algoritması için parametreler
        private double alpha = -0.1; // X-Shear katsayısı
        private double beta = 0.1;  // Y-Shear katsayısı

        // Shear miktarı = tan(açı / 2). 

        public Proje11_AcisizDondurmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 11: Kaydırma (Shear) ile Döndürme";

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
                Text = "Resmi yükleyin ve butonlarla kaydırarak döndürün.",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnSaatYonu = new Button() { Text = "Saat Yönü (Sağa Kaydır)", Location = new Point(160, 570), Size = new Size(180, 40), BackColor = Color.LightGreen, Enabled = false };
            btnSaatYonu.Click += (s, e) => ApplyShearRotation(true);

            btnTersYon = new Button() { Text = "Ters Yön (Sola Kaydır)", Location = new Point(350, 570), Size = new Size(180, 40), BackColor = Color.LightBlue, Enabled = false };
            btnTersYon.Click += (s, e) => ApplyShearRotation(false);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(550, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, lblBilgi, btnYukle, btnSaatYonu, btnTersYon, btnGeri });
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
                // Resmi biraz küçültelim ki işlem hızlı olsun ve dönünce taşmasın
                int w = Math.Min(originalBitmap.Width, 400);
                int h = (int)(w * ((float)originalBitmap.Height / originalBitmap.Width));
                processedBitmap = new Bitmap(originalBitmap, new Size(w, h));

                pcbResim.Image = processedBitmap;
                btnSaatYonu.Enabled = true;
                btnTersYon.Enabled = true;
            }
        }

        private void ApplyShearRotation(bool clockwise)
        {
            if (processedBitmap == null) return;

            // Sabit bir kaydırma miktarı (shear factor) belirleyelim.
            // Pozitif shear saat yönünde, negatif shear ters yönde etki eder.
            double shearX = clockwise ? -0.2 : 0.2; // Yatay kaydırma
            double shearY = clockwise ? 0.2 : -0.2; // Dikey kaydırma

            // 3 Adımlı Kaydırma (Paeth Algoritması benzeri)
            // X-Shear (Yatay Kaydırma)
            Bitmap step1 = ShearX(processedBitmap, shearX);

            // Y-Shear (Dikey Kaydırma)
            Bitmap step2 = ShearY(step1, shearY);

            // Tekrar X-Shear (Tam bir dönüş hareketi için)
            Bitmap step3 = ShearX(step2, shearX);

            processedBitmap = step3;
            pcbResim.Image = processedBitmap;
        }

        // Yatay Kaydırma (X-Shear) 
        private Bitmap ShearX(Bitmap bmp, double shear)
        {
            int width = bmp.Width + (int)(Math.Abs(shear) * bmp.Height);
            int height = bmp.Height;
            Bitmap result = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                // Her satır için kaydırma miktarı (skew) hesapla
                // shear pozitifse: üst satırlar az, alt satırlar çok kayar.
                // shear negatifse: tam tersi.
                double skew = shear * (y - height / 2);

                for (int x = 0; x < bmp.Width; x++)
                {
                    int newX = x + (int)skew + (width - bmp.Width) / 2; // Merkeze hizala

                    if (newX >= 0 && newX < width)
                    {
                        result.SetPixel(newX, y, bmp.GetPixel(x, y));
                    }
                }
            }
            return result;
        }

        // Dikey Kaydırma (Y-Shear)
        private Bitmap ShearY(Bitmap bmp, double shear)
        {
            int width = bmp.Width;
            int height = bmp.Height + (int)(Math.Abs(shear) * bmp.Width);
            Bitmap result = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                // Her sütun için kaydırma miktarı
                double skew = shear * (x - width / 2);

                for (int y = 0; y < bmp.Height; y++)
                {
                    int newY = y + (int)skew + (height - bmp.Height) / 2; // Merkeze hizala

                    if (newY >= 0 && newY < height)
                    {
                        result.SetPixel(x, newY, bmp.GetPixel(x, y));
                    }
                }
            }
            return result;
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