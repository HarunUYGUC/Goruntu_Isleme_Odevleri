using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje9_GeceGorusForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private TrackBar tbParlaklik, tbKontrastAlt, tbKontrastUst;
        private Button btnYukle, btnGeri;
        private Label lblParlaklik, lblKontrastAlt, lblKontrastUst, lblOriginal, lblResult;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap processedBitmap;

        public Proje9_GeceGorusForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 9: Gece Görüş Simülasyonu";

            int pcbY = 25;
            int pcbSize = 400;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal Düşük Işık", Location = new Point(margin + 120, pcbY - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Gece Görüş Sonucu", Location = new Point(margin * 2 + pcbSize + 110, pcbY - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, pcbY), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = pcbY + pcbSize + 20;
            lblParlaklik = new Label() { Text = "Ek Parlaklık: 0", Location = new Point(margin, controlsY), AutoSize = true };
            tbParlaklik = new TrackBar() { Location = new Point(margin - 5, controlsY + 20), Size = new Size(pcbSize + 10, 45), Minimum = -100, Maximum = 100, Value = 0, TickFrequency = 10 };

            lblKontrastAlt = new Label() { Text = "Kontrast Alt Sınır: 0", Location = new Point(margin * 2 + pcbSize, controlsY), AutoSize = true };
            tbKontrastAlt = new TrackBar() { Location = new Point(margin * 2 + pcbSize - 5, controlsY + 20), Size = new Size(pcbSize + 10, 45), Minimum = 0, Maximum = 255, Value = 0, TickFrequency = 10 };

            lblKontrastUst = new Label() { Text = "Kontrast Üst Sınır: 255", Location = new Point(margin * 2 + pcbSize, controlsY + 70), AutoSize = true };
            tbKontrastUst = new TrackBar() { Location = new Point(margin * 2 + pcbSize - 5, controlsY + 90), Size = new Size(pcbSize + 10, 45), Minimum = 0, Maximum = 255, Value = 255, TickFrequency = 10 };

            tbParlaklik.Scroll += new EventHandler(tb_Scroll);
            tbKontrastAlt.Scroll += new EventHandler(tb_Scroll);
            tbKontrastUst.Scroll += new EventHandler(tb_Scroll);

            int buttonsY = controlsY + 150;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, buttonsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, buttonsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            SetSliderStatus(false);
            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, lblParlaklik, tbParlaklik, lblKontrastAlt, tbKontrastAlt, lblKontrastUst, tbKontrastUst, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje9_GeceGorusForm";
            this.Size = new Size(880, 680);
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
                SetSliderStatus(true);
                ApplyNightVision();
            }
        }

        private void tb_Scroll(object sender, EventArgs e)
        {
            if (tbKontrastAlt.Value > tbKontrastUst.Value)
            {
                if (sender == tbKontrastAlt) tbKontrastUst.Value = tbKontrastAlt.Value;
                else tbKontrastAlt.Value = tbKontrastUst.Value;
            }
            lblParlaklik.Text = $"Ek Parlaklık: {tbParlaklik.Value}";
            lblKontrastAlt.Text = $"Kontrast Alt Sınır: {tbKontrastAlt.Value}";
            lblKontrastUst.Text = $"Kontrast Üst Sınır: {tbKontrastUst.Value}";
            ApplyNightVision();
        }

        private void ApplyNightVision()
        {
            if (originalBitmap == null) return;

            processedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            int parlaklik = tbParlaklik.Value;
            int altKontrast = tbKontrastAlt.Value;
            int ustKontrast = tbKontrastUst.Value;

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);
                    int gray = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11);

                    int contrastedGray = Stretch(gray, altKontrast, ustKontrast);

                    int finalValue = contrastedGray + parlaklik;

                    finalValue = Math.Max(0, Math.Min(255, finalValue));

                    // Gece Görüşü, Yeşil Tonunu Uygula
                    Color nightVisionColor = Color.FromArgb(finalValue / 5, finalValue, finalValue / 5);

                    processedBitmap.SetPixel(x, y, nightVisionColor);
                }
            }
            pcbResult.Image = processedBitmap;
        }

        private int Stretch(int value, int lower, int upper)
        {
            if (upper == lower) return 0; // Veya 128 gibi bir orta değer
            if (value <= lower) return 0;
            if (value >= upper) return 255;
            return (int)(((double)(value - lower) / (upper - lower)) * 255.0);
        }

        private void SetSliderStatus(bool status)
        {
            tbParlaklik.Enabled = status;
            tbKontrastAlt.Enabled = status;
            tbKontrastUst.Enabled = status;
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
