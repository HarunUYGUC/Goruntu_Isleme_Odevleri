using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_ResimBuyutmeForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnBuyut, btnGeri;
        private Label lblOran, lblOriginal, lblResult;
        private NumericUpDown nudOran;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje4_ResimBuyutmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Resim Büyütme (Zooming)";

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(25, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox()
            {
                Location = new Point(25, 45),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom 
            };

            lblResult = new Label() { Text = "Büyütülmüş Resim", Location = new Point(250, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            Panel pnlResultContainer = new Panel()
            {
                Location = new Point(250, 45),
                Size = new Size(500, 500),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true 
            };
            pcbResult = new PictureBox()
            {
                Location = new Point(0, 0),
                Size = new Size(500, 500), 
                SizeMode = PictureBoxSizeMode.AutoSize
            };
            pnlResultContainer.Controls.Add(pcbResult);

            int controlsY = 560;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblOran = new Label() { Text = "Büyütme Katsayısı:", Location = new Point(160, controlsY + 10), AutoSize = true };
            nudOran = new NumericUpDown() { Location = new Point(270, controlsY + 8), Width = 60, Minimum = 1, Maximum = 10, Value = 2 };

            btnBuyut = new Button() { Text = "Büyüt", Location = new Point(350, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnBuyut.Click += new EventHandler(btnBuyut_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(600, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pnlResultContainer, btnYukle, lblOran, nudOran, btnBuyut, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje4_ResimBuyutmeForm";
            this.Size = new Size(800, 660);
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
                pcbOriginal.Image = originalBitmap;
                pcbResult.Image = null;
                btnBuyut.Enabled = true;
            }
        }

        private void btnBuyut_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            int scaleFactor = (int)nudOran.Value; // Büyütme katsayısı (örn: 2)

            int newWidth = originalBitmap.Width * scaleFactor;
            int newHeight = originalBitmap.Height * scaleFactor;

            Bitmap zoomedBitmap = new Bitmap(newWidth, newHeight);

            // Nearest Neighbor (En Yakın Komşu) İnterpolasyonu
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = x / scaleFactor;
                    int srcY = y / scaleFactor;

                    if (srcX >= originalBitmap.Width) srcX = originalBitmap.Width - 1;
                    if (srcY >= originalBitmap.Height) srcY = originalBitmap.Height - 1;

                    Color pixelColor = originalBitmap.GetPixel(srcX, srcY);
                    zoomedBitmap.SetPixel(x, y, pixelColor);
                }
            }

            pcbResult.Image = zoomedBitmap;
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