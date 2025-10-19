using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_MozaikForm : Form
    {
        private PictureBox pcbOriginal, pcbGray, pcbMosaic;
        private Button btnYukle, btnMozaikOlustur, btnGeri;
        private ComboBox cmbMozaikBoyutu;
        private Label lblMozaikBoyutu, lblOriginal, lblGray, lblMosaic;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap grayBitmap;

        public Proje4_MozaikForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: Mozaik Oluşturma";

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(160, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblGray = new Label() { Text = "Gri Tonlamalı Resim", Location = new Point(580, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblMosaic = new Label() { Text = "Mozaik Resim", Location = new Point(1010, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            int pcbY = 40;
            int pcbSize = 400;
            pcbOriginal = new PictureBox() { Location = new Point(25, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbGray = new PictureBox() { Location = new Point(450, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbMosaic = new PictureBox() { Location = new Point(875, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = 460;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblMozaikBoyutu = new Label() { Text = "Mozaik Boyutu (Piksel):", Location = new Point(200, controlsY + 12), AutoSize = true };
            cmbMozaikBoyutu = new ComboBox() { Location = new Point(340, controlsY + 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMozaikBoyutu.Items.AddRange(new object[] { "2", "5", "10", "25" });
            cmbMozaikBoyutu.SelectedIndex = 0;

            btnMozaikOlustur = new Button() { Text = "Mozaik Oluştur", Location = new Point(480, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen };
            btnMozaikOlustur.Click += new EventHandler(btnMozaikOlustur_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(1125, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, lblGray, lblMosaic, pcbOriginal, pcbGray, pcbMosaic, btnYukle, lblMozaikBoyutu, cmbMozaikBoyutu, btnMozaikOlustur, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje4_MozaikForm";
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

                ConvertToGray();
                pcbMosaic.Image = null;
            }
        }

        private void ConvertToGray()
        {
            if (originalBitmap == null) return;
            grayBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);
                    int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                    Color grayColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    grayBitmap.SetPixel(x, y, grayColor);
                }
            }
            pcbGray.Image = grayBitmap;
        }

        private void btnMozaikOlustur_Click(object sender, EventArgs e)
        {
            if (grayBitmap == null) { MessageBox.Show("Lütfen önce bir resim yükleyin!"); return; }

            int mozaikBoyutu = int.Parse(cmbMozaikBoyutu.SelectedItem.ToString());

            int yeniGenislik = grayBitmap.Width / mozaikBoyutu;
            int yeniYukseklik = grayBitmap.Height / mozaikBoyutu;

            Bitmap mosaicBitmap = new Bitmap(yeniGenislik, yeniYukseklik);

            for (int y = 0; y < yeniYukseklik; y++)
            {
                for (int x = 0; x < yeniGenislik; x++)
                {
                    long toplamGriDeger = 0;
                    int pikselSayisi = 0;

                    // Orijinal (gri) resimdeki mozaik bloğunu tara
                    for (int blockY = 0; blockY < mozaikBoyutu; blockY++)
                    {
                        for (int blockX = 0; blockX < mozaikBoyutu; blockX++)
                        {
                            int pikselX = (x * mozaikBoyutu) + blockX;
                            int pikselY = (y * mozaikBoyutu) + blockY;

                            if (pikselX < grayBitmap.Width && pikselY < grayBitmap.Height)
                            {
                                // Gri resimde R, G, B aynı olduğu için birini almak yeterli.
                                toplamGriDeger += grayBitmap.GetPixel(pikselX, pikselY).R;
                                pikselSayisi++;
                            }
                        }
                    }

                    int ortalamaGri = (int)(toplamGriDeger / pikselSayisi);
                    Color ortalamaRenk = Color.FromArgb(ortalamaGri, ortalamaGri, ortalamaGri);

                    mosaicBitmap.SetPixel(x, y, ortalamaRenk);
                }
            }
            pcbMosaic.Image = mosaicBitmap;
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

