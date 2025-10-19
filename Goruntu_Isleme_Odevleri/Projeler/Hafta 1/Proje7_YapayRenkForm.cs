using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje7_YapayRenkForm : Form
    {
        private PictureBox pcbOriginal, pcbGRB, pcbRBG, pcbGBR;
        private Button btnYukle, btnDonustur, btnGeri;
        private Label lblOriginal, lblGRB, lblRBG, lblGBR;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje7_YapayRenkForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 7: Yapay Renkli Görüntüler";

            int pcbSize = 300;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal (R-G-B)", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblGRB = new Label() { Text = "Yapay (G-R-B)", Location = new Point(margin * 2 + pcbSize + 90, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblRBG = new Label() { Text = "Yapay (R-B-G)", Location = new Point(margin + 100, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblGBR = new Label() { Text = "Yapay (G-B-R)", Location = new Point(margin * 2 + pcbSize + 90, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbGRB = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            pcbRBG = new PictureBox() { Location = new Point(margin, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            pcbGBR = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = this.ClientSize.Height - 60;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnDonustur = new Button() { Text = "Dönüştür", Location = new Point(margin + 170, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen };
            btnDonustur.Click += new EventHandler(btnDonustur_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, lblGRB, lblRBG, lblGBR, pcbOriginal, pcbGRB, pcbRBG, pcbGBR, btnYukle, btnDonustur, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje7_YapayRenkForm";
            this.Size = new Size(700, 770);
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
                pcbGRB.Image = null;
                pcbRBG.Image = null;
                pcbGBR.Image = null;
            }
        }

        private void btnDonustur_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) { MessageBox.Show("Lütfen önce bir resim yükleyin!"); return; }

            int genislik = originalBitmap.Width;
            int yukseklik = originalBitmap.Height;

            Bitmap grbBitmap = new Bitmap(genislik, yukseklik);
            Bitmap rbgBitmap = new Bitmap(genislik, yukseklik);
            Bitmap gbrBitmap = new Bitmap(genislik, yukseklik);

            for (int y = 0; y < yukseklik; y++)
            {
                for (int x = 0; x < genislik; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);
                    int r = originalColor.R;
                    int g = originalColor.G;
                    int b = originalColor.B;

                    // G-R-B -> / Kırmızıya G, Yeşile R, Maviye B değerini ata
                    Color grbColor = Color.FromArgb(g, r, b);

                    // R-B-G -> Kırmızıya R, Yeşile B, Maviye G değerini ata
                    Color rbgColor = Color.FromArgb(r, b, g);

                    // G-B-R -> Kırmızıya G, Yeşile B, Maviye R değerini ata
                    Color gbrColor = Color.FromArgb(g, b, r);

                    grbBitmap.SetPixel(x, y, grbColor);
                    rbgBitmap.SetPixel(x, y, rbgColor);
                    gbrBitmap.SetPixel(x, y, gbrColor);
                }
            }

            pcbGRB.Image = grbBitmap;
            pcbRBG.Image = rbgBitmap;
            pcbGBR.Image = gbrBitmap;
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
