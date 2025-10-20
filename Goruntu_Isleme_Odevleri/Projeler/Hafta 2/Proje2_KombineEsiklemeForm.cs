using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_KombineEsiklemeForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private TrackBar tbGrayLower, tbGrayUpper, tbRedLower, tbRedUpper, tbGreenLower, tbGreenUpper, tbBlueLower, tbBlueUpper;
        private Button btnYukle, btnGeri;
        private Label lblGrayLower, lblGrayUpper, lblRed, lblGreen, lblBlue, lblRedLower, lblRedUpper, lblGreenLower, lblGreenUpper, lblBlueLower, lblBlueUpper;
        private Panel panelGrayControls, panelColorControls;
        private CheckBox chkColorMode;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap grayBitmap;

        public Proje2_KombineEsiklemeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Kombine Eşikleme";

            pcbOriginal = new PictureBox() { Location = new Point(25, 25), Size = new Size(400, 350), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbResult = new PictureBox() { Location = new Point(450, 25), Size = new Size(400, 350), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            panelGrayControls = new Panel() { Location = new Point(0, 380), Size = new Size(900, 100) };
            lblGrayLower = new Label() { Text = "Alt Eşik Değeri: 50", Location = new Point(25, 20), AutoSize = true, Font = new Font("Arial", 10) };
            tbGrayLower = new TrackBar() { Location = new Point(20, 40), Size = new Size(360, 45), Maximum = 255, Value = 50, TickFrequency = 10 };
            lblGrayUpper = new Label() { Text = "Üst Eşik Değeri: 200", Location = new Point(450, 20), AutoSize = true, Font = new Font("Arial", 10) };
            tbGrayUpper = new TrackBar() { Location = new Point(445, 40), Size = new Size(360, 45), Maximum = 255, Value = 200, TickFrequency = 10 };
            panelGrayControls.Controls.AddRange(new Control[] { lblGrayLower, tbGrayLower, lblGrayUpper, tbGrayUpper });

            panelColorControls = new Panel() { Location = new Point(0, 380), Size = new Size(900, 200), Visible = false };
            int trackBarWidth = 256;
            int gap = 30;

            lblRed = new Label() { Text = "Kırmızı Kanal", Location = new Point(25 + 80, 0), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblRedLower = new Label() { Text = "Alt: 0", Location = new Point(25, 30), AutoSize = true };
            tbRedLower = new TrackBar() { Location = new Point(20, 50), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 0 };
            lblRedUpper = new Label() { Text = "Üst: 255", Location = new Point(25, 95), AutoSize = true };
            tbRedUpper = new TrackBar() { Location = new Point(20, 115), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 255 };

            int greenX = 25 + trackBarWidth + gap;
            lblGreen = new Label() { Text = "Yeşil Kanal", Location = new Point(greenX + 90, 0), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblGreenLower = new Label() { Text = "Alt: 0", Location = new Point(greenX, 30), AutoSize = true };
            tbGreenLower = new TrackBar() { Location = new Point(greenX - 5, 50), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 0 };
            lblGreenUpper = new Label() { Text = "Üst: 255", Location = new Point(greenX, 95), AutoSize = true };
            tbGreenUpper = new TrackBar() { Location = new Point(greenX - 5, 115), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 255 };

            int blueX = greenX + trackBarWidth + gap;
            lblBlue = new Label() { Text = "Mavi Kanal", Location = new Point(blueX + 90, 0), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            lblBlueLower = new Label() { Text = "Alt: 0", Location = new Point(blueX, 30), AutoSize = true };
            tbBlueLower = new TrackBar() { Location = new Point(blueX - 5, 50), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 0 };
            lblBlueUpper = new Label() { Text = "Üst: 255", Location = new Point(blueX, 95), AutoSize = true };
            tbBlueUpper = new TrackBar() { Location = new Point(blueX - 5, 115), Size = new Size(trackBarWidth + 10, 45), Maximum = 255, Value = 255 };

            panelColorControls.Controls.AddRange(new Control[] { lblRed, lblRedLower, tbRedLower, lblRedUpper, tbRedUpper, lblGreen, lblGreenLower, tbGreenLower, lblGreenUpper, tbGreenUpper, lblBlue, lblBlueLower, tbBlueLower, lblBlueUpper, tbBlueUpper });

            TrackBar[] allTrackBars = { tbGrayLower, tbGrayUpper, tbRedLower, tbRedUpper, tbGreenLower, tbGreenUpper, tbBlueLower, tbBlueUpper };
            foreach (var tb in allTrackBars)
            {
                tb.Scroll += new EventHandler(tb_Scroll);
                tb.Enabled = false;
            }

            int buttonsY = 580;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, buttonsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            chkColorMode = new CheckBox() { Text = "Renkli Eşikleme Modu", Location = new Point(200, buttonsY + 12), AutoSize = true };
            chkColorMode.CheckedChanged += new EventHandler(chkColorMode_CheckedChanged);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 25 - 150, buttonsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbOriginal, pcbResult, panelGrayControls, panelColorControls, btnYukle, chkColorMode, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_KombineEsiklemeForm";
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
                ConvertToGray();

                TrackBar[] allTrackBars = { tbGrayLower, tbGrayUpper, tbRedLower, tbRedUpper, tbGreenLower, tbGreenUpper, tbBlueLower, tbBlueUpper };
                foreach (var tb in allTrackBars) tb.Enabled = true;

                ApplyThresholding();
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
                    Color c = originalBitmap.GetPixel(x, y);
                    int gray = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
                    grayBitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
        }

        private void chkColorMode_CheckedChanged(object sender, EventArgs e)
        {
            panelGrayControls.Visible = !chkColorMode.Checked;
            panelColorControls.Visible = chkColorMode.Checked;
            ApplyThresholding();
        }

        private void tb_Scroll(object sender, EventArgs e)
        {
            if (tbGrayLower.Value > tbGrayUpper.Value) tbGrayUpper.Value = tbGrayLower.Value;
            if (tbGrayUpper.Value < tbGrayLower.Value) tbGrayLower.Value = tbGrayUpper.Value;

            if (tbRedLower.Value > tbRedUpper.Value) tbRedUpper.Value = tbRedLower.Value;
            if (tbRedUpper.Value < tbRedLower.Value) tbRedLower.Value = tbRedUpper.Value;

            if (tbGreenLower.Value > tbGreenUpper.Value) tbGreenUpper.Value = tbGreenLower.Value;
            if (tbGreenUpper.Value < tbGreenLower.Value) tbGreenLower.Value = tbGreenUpper.Value;

            if (tbBlueLower.Value > tbBlueUpper.Value) tbBlueUpper.Value = tbBlueLower.Value;
            if (tbBlueUpper.Value < tbBlueLower.Value) tbBlueLower.Value = tbBlueUpper.Value;

            lblGrayLower.Text = $"Alt Eşik Değeri: {tbGrayLower.Value}";
            lblGrayUpper.Text = $"Üst Eşik Değeri: {tbGrayUpper.Value}";
            lblRedLower.Text = $"Alt: {tbRedLower.Value}";
            lblRedUpper.Text = $"Üst: {tbRedUpper.Value}";
            lblGreenLower.Text = $"Alt: {tbGreenLower.Value}";
            lblGreenUpper.Text = $"Üst: {tbGreenUpper.Value}";
            lblBlueLower.Text = $"Alt: {tbBlueLower.Value}";
            lblBlueUpper.Text = $"Üst: {tbBlueUpper.Value}";

            ApplyThresholding();
        }

        private void ApplyThresholding()
        {
            if (originalBitmap == null) return;

            Bitmap resultBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            // Orijinal resim üzerinden renkli ayrım.
            if (chkColorMode.Checked)
            {
                pcbOriginal.Image = originalBitmap;
                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    for (int x = 0; x < originalBitmap.Width; x++)
                    {
                        Color p = originalBitmap.GetPixel(x, y);
                        int r = (p.R >= tbRedLower.Value && p.R <= tbRedUpper.Value) ? p.R : 0;
                        int g = (p.G >= tbGreenLower.Value && p.G <= tbGreenUpper.Value) ? p.G : 0;
                        int b = (p.B >= tbBlueLower.Value && p.B <= tbBlueUpper.Value) ? p.B : 0;
                        resultBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
            }
            // Gri resim üzerinden siyah beyaz ayrımı.
            else
            {
                pcbOriginal.Image = grayBitmap;
                for (int y = 0; y < grayBitmap.Height; y++)
                {
                    for (int x = 0; x < grayBitmap.Width; x++)
                    {
                        int gray = grayBitmap.GetPixel(x, y).R;
                        if (gray >= tbGrayLower.Value && gray <= tbGrayUpper.Value)
                            resultBitmap.SetPixel(x, y, Color.White);
                        else
                            resultBitmap.SetPixel(x, y, Color.Black);
                    }
                }
            }
            pcbResult.Image = resultBitmap;
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

