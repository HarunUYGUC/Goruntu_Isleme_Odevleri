using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje8_BitDerinligiForm : Form
    {
        private PictureBox pcbOriginal, pcbGray, pcbGrayResult, pcbColorResult;
        private Button btnYukle, btnUygula, btnGeri;
        private ComboBox cmbBitSayisi;
        private Label lblBitSayisi, lblOriginal, lblGray, lblGrayResult, lblColorResult;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap grayBitmap;

        public Proje8_BitDerinligiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 8: Bit Derinliği Azaltma";

            int pcbSize = 300;
            int margin = 25;

            lblOriginal = new Label() { Text = "1. Orijinal", Location = new Point(margin + 110, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblGray = new Label() { Text = "2. Gri Tonlamalı (8-bit)", Location = new Point(margin * 2 + pcbSize + 80, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbGray = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblGrayResult = new Label() { Text = "3. Sonuç (Gri i-bit)", Location = new Point(margin + 90, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbGrayResult = new PictureBox() { Location = new Point(margin, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblColorResult = new Label() { Text = "4. Sonuç (Renkli i-bit)", Location = new Point(margin * 2 + pcbSize + 70, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbColorResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin * 2 + pcbSize), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = this.ClientSize.Height - 60;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblBitSayisi = new Label() { Text = "Hedef Bit Sayısı:", Location = new Point(200, controlsY + 12), AutoSize = true };
            cmbBitSayisi = new ComboBox() { Location = new Point(310, controlsY + 10), DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
            for (int i = 7; i >= 1; i--)
            {
                cmbBitSayisi.Items.Add(i);
            }
            cmbBitSayisi.SelectedIndex = 0;

            btnUygula = new Button() { Text = "Uygula", Location = new Point(450, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblGray, pcbGray, lblGrayResult, pcbGrayResult, lblColorResult, pcbColorResult, btnYukle, lblBitSayisi, cmbBitSayisi, btnUygula, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje8_BitDerinligiForm";
            this.Size = new Size(800, 770);
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
                pcbGrayResult.Image = null;
                pcbColorResult.Image = null;
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

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) { MessageBox.Show("Lütfen önce bir resim yükleyin!"); return; }

            int bitSayisi = (int)cmbBitSayisi.SelectedItem;
            int renkSeviyesi = (int)Math.Pow(2, bitSayisi);

            Bitmap renkliSonuc = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            Bitmap griSonuc = new Bitmap(grayBitmap.Width, grayBitmap.Height);

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);
                    int r = (int)Math.Round((double)originalColor.R / 255 * (renkSeviyesi - 1)) * (255 / (renkSeviyesi - 1));
                    int g = (int)Math.Round((double)originalColor.G / 255 * (renkSeviyesi - 1)) * (255 / (renkSeviyesi - 1));
                    int b = (int)Math.Round((double)originalColor.B / 255 * (renkSeviyesi - 1)) * (255 / (renkSeviyesi - 1));
                    renkliSonuc.SetPixel(x, y, Color.FromArgb(r, g, b));

                    Color grayColor = grayBitmap.GetPixel(x, y);
                    int grayValue = (int)Math.Round((double)grayColor.R / 255 * (renkSeviyesi - 1)) * (255 / (renkSeviyesi - 1));
                    griSonuc.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            pcbGrayResult.Image = griSonuc;
            pcbColorResult.Image = renkliSonuc;
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

