using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_OrneklemeNicemlemeForm : Form
    {
        private PictureBox pcbOriginal, pcbSampled, pcbQuantized;
        private Button btnYukle, btnOrnekle, btnNicemle, btnGeri;
        private ComboBox cmbOrnekleme, cmbNicemleme;
        private Label lblOrnekleme, lblNicemleme, lblOriginal, lblSampled, lblQuantized;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje3_OrneklemeNicemlemeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Örnekleme ve Renk Nicemlemesi";

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(160, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblSampled = new Label() { Text = "Örneklenmiş Resim", Location = new Point(585, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblQuantized = new Label() { Text = "Nicemlenmiş Resim", Location = new Point(1010, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            int pcbY = 40;
            int pcbSize = 400;
            pcbOriginal = new PictureBox() { Location = new Point(25, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbSampled = new PictureBox() { Location = new Point(450, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbQuantized = new PictureBox() { Location = new Point(875, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = 460;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblOrnekleme = new Label() { Text = "Örnekleme Aralığı:", Location = new Point(200, controlsY), AutoSize = true };
            cmbOrnekleme = new ComboBox() { Location = new Point(200, controlsY + 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOrnekleme.Items.AddRange(new object[] { "2", "5", "10", "25", "50" });
            cmbOrnekleme.SelectedIndex = 0;
            btnOrnekle = new Button() { Text = "Örnekle", Location = new Point(330, controlsY + 15), Size = new Size(95, 30) };
            btnOrnekle.Click += new EventHandler(btnOrnekle_Click);

            lblNicemleme = new Label() { Text = "Renk Nicemleme Katı:", Location = new Point(450, controlsY), AutoSize = true };
            cmbNicemleme = new ComboBox() { Location = new Point(450, controlsY + 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbNicemleme.Items.AddRange(new object[] { "10", "50", "100" });
            cmbNicemleme.SelectedIndex = 0;
            btnNicemle = new Button() { Text = "Nicemle", Location = new Point(580, controlsY + 15), Size = new Size(95, 30) };
            btnNicemle.Click += new EventHandler(btnNicemle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(1125, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, lblSampled, lblQuantized, pcbOriginal, pcbSampled, pcbQuantized, btnYukle, lblOrnekleme, cmbOrnekleme, btnOrnekle, lblNicemleme, cmbNicemleme, btnNicemle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_OrneklemeNicemlemeForm";
            this.Size = new Size(1300, 570);
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
                // Yeni resim yüklendiğinde diğer kutuları temizle
                pcbSampled.Image = null;
                pcbQuantized.Image = null;
            }
        }

        private void btnOrnekle_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null)
            {
                MessageBox.Show("Lütfen önce bir resim yükleyin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int aralik = int.Parse(cmbOrnekleme.SelectedItem.ToString());
            Bitmap sampledBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            for (int y = 0; y < originalBitmap.Height; y += aralik)
            {
                for (int x = 0; x < originalBitmap.Width; x += aralik)
                {
                    Color pixelColor = originalBitmap.GetPixel(x, y);
                    for (int blockY = y; blockY < y + aralik && blockY < originalBitmap.Height; blockY++)
                    {
                        for (int blockX = x; blockX < x + aralik && blockX < originalBitmap.Width; blockX++)
                        {
                            sampledBitmap.SetPixel(blockX, blockY, pixelColor);
                        }
                    }
                }
            }

            pcbSampled.Image = sampledBitmap;
        }

        private void btnNicemle_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null)
            {
                MessageBox.Show("Lütfen önce bir resim yükleyin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int kat = int.Parse(cmbNicemleme.SelectedItem.ToString());
            Bitmap quantizedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);

                    int newR = (int)(Math.Round((double)originalColor.R / kat) * kat);
                    int newG = (int)(Math.Round((double)originalColor.G / kat) * kat);
                    int newB = (int)(Math.Round((double)originalColor.B / kat) * kat);

                    newR = Math.Min(255, newR);
                    newG = Math.Min(255, newG);
                    newB = Math.Min(255, newB);

                    quantizedBitmap.SetPixel(x, y, Color.FromArgb(newR, newG, newB));
                }
            }

            pcbQuantized.Image = quantizedBitmap;
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

