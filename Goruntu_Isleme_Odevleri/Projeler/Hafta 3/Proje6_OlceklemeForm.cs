using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje6_OlceklemeForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnOlcekle, btnGeri;
        private Label lblOriginal, lblResult, lblOran;
        private NumericUpDown nudOran;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje6_OlceklemeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 6: Resim Ölçekleme (Scaling)";

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(25, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox()
            {
                Location = new Point(25, 45),
                Size = new Size(250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            lblResult = new Label() { Text = "Ölçeklenmiş Resim", Location = new Point(300, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            Panel pnlResultContainer = new Panel()
            {
                Location = new Point(300, 45),
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

            lblOran = new Label() { Text = "Ölçek Oranı (%):", Location = new Point(160, controlsY + 10), AutoSize = true };

            // Ölçek oranı için NumericUpDown (Yüzde olarak, örneğin 50 = %50, 200 = %200)
            nudOran = new NumericUpDown()
            {
                Location = new Point(260, controlsY + 8),
                Width = 70,
                Minimum = 1,
                Maximum = 500,
                Value = 100, // Varsayılan
                Increment = 10
            };

            btnOlcekle = new Button() { Text = "Uygula", Location = new Point(350, controlsY), Size = new Size(120, 40), BackColor = Color.LightGreen, Enabled = false };
            btnOlcekle.Click += new EventHandler(btnOlcekle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(650, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pnlResultContainer, btnYukle, lblOran, nudOran, btnOlcekle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje6_OlceklemeForm";
            this.Size = new Size(850, 660);
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
                pcbResult.Image = originalBitmap;
                btnOlcekle.Enabled = true;
            }
        }

        private void btnOlcekle_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            // Yüzdelik oranı ondalıklı sayıya çevir
            double scaleFactor = (double)nudOran.Value / 100.0;

            int newWidth = (int)(originalBitmap.Width * scaleFactor);
            int newHeight = (int)(originalBitmap.Height * scaleFactor);

            // Boyutların 0 olmasını engelle
            newWidth = Math.Max(1, newWidth);
            newHeight = Math.Max(1, newHeight);

            Bitmap scaledBitmap = new Bitmap(newWidth, newHeight);

            // Nearest Neighbor (En Yakın Komşu) Algoritması
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // Yeni resimdeki (x, y) pikselinin orijinal resimdeki karşılığını bul.
                    // Ters eşleme (Inverse Mapping): Hedef koordinat / Ölçek faktörü
                    int srcX = (int)(x / scaleFactor);
                    int srcY = (int)(y / scaleFactor);

                    // Sınır kontrolü (Taşmayı önle)
                    if (srcX >= originalBitmap.Width) srcX = originalBitmap.Width - 1;
                    if (srcY >= originalBitmap.Height) srcY = originalBitmap.Height - 1;

                    Color pixelColor = originalBitmap.GetPixel(srcX, srcY);
                    scaledBitmap.SetPixel(x, y, pixelColor);
                }
            }

            pcbResult.Image = scaledBitmap;
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