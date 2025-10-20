using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_HistogramSikistirmaForm : Form
    {
        private PictureBox pcbResult, pcbHistogram;
        private TrackBar tbLower, tbUpper;
        private Button btnYukle, btnGeri;
        private Label lblLower, lblUpper, lblCoords, lblRGB;
        private TextBox txtCoords, txtRGB;
        private CheckBox chkCiftSurgu;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap grayBitmap;

        public Proje3_HistogramSikistirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Histogram ile Sıkıştırma";

            pcbResult = new PictureBox() { Location = new Point(25, 25), Size = new Size(512, 512), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbResult.MouseMove += new MouseEventHandler(pcbResult_MouseMove);

            pcbHistogram = new PictureBox() { Location = new Point(562, 25), Size = new Size(256, 200), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };

            chkCiftSurgu = new CheckBox() { Text = "Çift Sürgü Modu (Sıkıştırma)", Location = new Point(562, 240), AutoSize = true };
            chkCiftSurgu.CheckedChanged += new EventHandler(chkCiftSurgu_CheckedChanged);

            lblLower = new Label() { Text = "Alt Sınır: 0", Location = new Point(562, 275), AutoSize = true, Font = new Font("Arial", 10), Visible = false };
            tbLower = new TrackBar() { Location = new Point(557, 295), Size = new Size(266, 45), Maximum = 255, Value = 0, TickFrequency = 10, Visible = false };

            lblUpper = new Label() { Text = "Üst Sınır: 255", Location = new Point(562, 345), AutoSize = true, Font = new Font("Arial", 10) };
            tbUpper = new TrackBar() { Location = new Point(557, 365), Size = new Size(266, 45), Maximum = 255, Value = 255, TickFrequency = 10 };

            tbLower.Scroll += new EventHandler(tb_Scroll);
            tbUpper.Scroll += new EventHandler(tb_Scroll);

            lblCoords = new Label() { Text = "Koordinat (X, Y):", Location = new Point(562, 430), AutoSize = true };
            txtCoords = new TextBox() { Location = new Point(680, 428), Width = 138, ReadOnly = true };
            lblRGB = new Label() { Text = "Renk (R, G, B):", Location = new Point(562, 460), AutoSize = true };
            txtRGB = new TextBox() { Location = new Point(680, 458), Width = 138, ReadOnly = true };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 550), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 175, 550), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResult, pcbHistogram, chkCiftSurgu, lblLower, tbLower, lblUpper, tbUpper, lblCoords, txtCoords, lblRGB, txtRGB, btnYukle, btnGeri });
            SetSliderStatus(false);
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_HistogramSikistirmaForm";
            this.Size = new Size(860, 650);
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
                ConvertToGray();
                SetSliderStatus(true);
                ApplyStretchingAndDraw();
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

        private void chkCiftSurgu_CheckedChanged(object sender, EventArgs e)
        {
            lblLower.Visible = chkCiftSurgu.Checked;
            tbLower.Visible = chkCiftSurgu.Checked;
            ApplyStretchingAndDraw();
        }

        private void tb_Scroll(object sender, EventArgs e)
        {
            if (tbLower.Value > tbUpper.Value)
            {
                if (sender == tbLower)
                    tbUpper.Value = tbLower.Value;
                else
                    tbLower.Value = tbUpper.Value;
            }
            lblLower.Text = $"Alt Sınır: {tbLower.Value}";
            lblUpper.Text = $"Üst Sınır: {tbUpper.Value}";
            ApplyStretchingAndDraw();
        }

        private void ApplyStretchingAndDraw()
        {
            if (grayBitmap == null) return;

            Bitmap resultBitmap = new Bitmap(grayBitmap.Width, grayBitmap.Height);
            int[] histogram = new int[256];
            int lowerBound = chkCiftSurgu.Checked ? tbLower.Value : 0;
            int upperBound = tbUpper.Value;

            for (int y = 0; y < grayBitmap.Height; y++)
            {
                for (int x = 0; x < grayBitmap.Width; x++)
                {
                    int originalGray = grayBitmap.GetPixel(x, y).R;
                    int newGray;

                    if (chkCiftSurgu.Checked)
                    {
                        if (originalGray <= lowerBound) newGray = 0;
                        else if (originalGray >= upperBound) newGray = 255;
                        else
                        {
                            // upperBound ve lowerBound aynıysa 0'a bölünme hatasını önle
                            if (upperBound == lowerBound)
                            {
                                newGray = 0;
                            }
                            else
                            {
                                newGray = (int)(((double)(originalGray - lowerBound) / (upperBound - lowerBound)) * 255.0);
                            }
                        }
                    }
                    else
                    {
                        newGray = (int)(((double)originalGray / 255.0) * upperBound);
                    }

                    newGray = Math.Max(0, Math.Min(255, newGray));
                    resultBitmap.SetPixel(x, y, Color.FromArgb(newGray, newGray, newGray));
                    histogram[newGray]++;
                }
            }
            pcbResult.Image = resultBitmap;
            DrawHistogram(histogram);
        }

        private void DrawHistogram(int[] histogram)
        {
            if (pcbHistogram.Image != null) pcbHistogram.Image.Dispose();
            Bitmap histoBitmap = new Bitmap(pcbHistogram.Width, pcbHistogram.Height);
            Graphics g = Graphics.FromImage(histoBitmap);
            g.Clear(Color.Black);

            int max = 0;
            foreach (int value in histogram) if (value > max) max = value;

            for (int i = 0; i < 256; i++)
            {
                float height = (max > 0) ? (float)histogram[i] / max * pcbHistogram.Height : 0;
                g.DrawLine(Pens.White, i, pcbHistogram.Height, i, pcbHistogram.Height - height);
            }
            g.Dispose();
            pcbHistogram.Image = histoBitmap;
        }

        private void pcbResult_MouseMove(object sender, MouseEventArgs e)
        {
            if (pcbResult.Image == null) return;

            // PictureBox'ın Zoom özelliği nedeniyle koordinatları yeniden hesaplamamız gerekiyor.
            Bitmap bmp = (Bitmap)pcbResult.Image;
            int x = e.X * bmp.Width / pcbResult.ClientSize.Width;
            int y = e.Y * bmp.Height / pcbResult.ClientSize.Height;

            if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
            {
                Color pixelColor = bmp.GetPixel(x, y);
                txtCoords.Text = $"{x}, {y}";
                txtRGB.Text = $"{pixelColor.R}, {pixelColor.G}, {pixelColor.B}";
            }
        }

        private void SetSliderStatus(bool status)
        {
            tbLower.Enabled = status;
            tbUpper.Enabled = status;
            chkCiftSurgu.Enabled = status;
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

