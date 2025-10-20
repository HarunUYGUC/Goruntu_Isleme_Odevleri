using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_RenkliKontrastForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private PictureBox pcbRedHistogram, pcbGreenHistogram, pcbBlueHistogram;
        private Button btnYukle, btnUygula, btnGeri;
        private TextBox txtRedLower, txtRedUpper, txtGreenLower, txtGreenUpper, txtBlueLower, txtBlueUpper;
        private Label lblRed, lblGreen, lblBlue;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private int[] redHistogram = new int[256];
        private int[] greenHistogram = new int[256];
        private int[] blueHistogram = new int[256];

        public Proje5_RenkliKontrastForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: Renkli Kontrast Artırma";

            pcbOriginal = new PictureBox() { Location = new Point(25, 25), Size = new Size(400, 350), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };
            pcbResult = new PictureBox() { Location = new Point(450, 25), Size = new Size(400, 350), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int histoY = 400;
            int histoWidth = 256;
            int histoHeight = 150;
            int gap = 30;

            lblRed = new Label() { Text = "Kırmızı Kanal (Alt - Üst)", Location = new Point(25, histoY), AutoSize = true };
            pcbRedHistogram = new PictureBox() { Location = new Point(25, histoY + 20), Size = new Size(histoWidth, histoHeight), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };
            txtRedLower = new TextBox() { Location = new Point(25, histoY + 20 + histoHeight + 5), Width = 125 };
            txtRedUpper = new TextBox() { Location = new Point(156, histoY + 20 + histoHeight + 5), Width = 125 };
            pcbRedHistogram.MouseClick += (sender, e) => HandleHistogramMouseClick(sender, e, txtRedLower, txtRedUpper);

            int greenX = 25 + histoWidth + gap;
            lblGreen = new Label() { Text = "Yeşil Kanal (Alt - Üst)", Location = new Point(greenX, histoY), AutoSize = true };
            pcbGreenHistogram = new PictureBox() { Location = new Point(greenX, histoY + 20), Size = new Size(histoWidth, histoHeight), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };
            txtGreenLower = new TextBox() { Location = new Point(greenX, histoY + 20 + histoHeight + 5), Width = 125 };
            txtGreenUpper = new TextBox() { Location = new Point(greenX + 131, histoY + 20 + histoHeight + 5), Width = 125 };
            pcbGreenHistogram.MouseClick += (sender, e) => HandleHistogramMouseClick(sender, e, txtGreenLower, txtGreenUpper);

            int blueX = greenX + histoWidth + gap;
            lblBlue = new Label() { Text = "Mavi Kanal (Alt - Üst)", Location = new Point(blueX, histoY), AutoSize = true };
            pcbBlueHistogram = new PictureBox() { Location = new Point(blueX, histoY + 20), Size = new Size(histoWidth, histoHeight), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };
            txtBlueLower = new TextBox() { Location = new Point(blueX, histoY + 20 + histoHeight + 5), Width = 125 };
            txtBlueUpper = new TextBox() { Location = new Point(blueX + 131, histoY + 20 + histoHeight + 5), Width = 125 };
            pcbBlueHistogram.MouseClick += (sender, e) => HandleHistogramMouseClick(sender, e, txtBlueLower, txtBlueUpper);

            int buttonsY = histoY + 20 + histoHeight + 40;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, buttonsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnUygula = new Button() { Text = "Kontrast Uygula", Location = new Point(200, buttonsY), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 25 - 150, buttonsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] {
                pcbOriginal, pcbResult, lblRed, pcbRedHistogram, txtRedLower, txtRedUpper,
                lblGreen, pcbGreenHistogram, txtGreenLower, txtGreenUpper,
                lblBlue, pcbBlueHistogram, txtBlueLower, txtBlueUpper,
                btnYukle, btnUygula, btnGeri
            });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje5_RenkliKontrastForm";
            this.Size = new Size(880, 700);
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
                CalculateAndDrawHistograms();
                btnUygula.Enabled = true;
            }
        }

        private void CalculateAndDrawHistograms()
        {
            if (originalBitmap == null) return;
            Array.Clear(redHistogram, 0, 256);
            Array.Clear(greenHistogram, 0, 256);
            Array.Clear(blueHistogram, 0, 256);

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color pixel = originalBitmap.GetPixel(x, y);
                    redHistogram[pixel.R]++;
                    greenHistogram[pixel.G]++;
                    blueHistogram[pixel.B]++;
                }
            }
            DrawHistogram(pcbRedHistogram, redHistogram, Color.Red);
            DrawHistogram(pcbGreenHistogram, greenHistogram, Color.Green);
            DrawHistogram(pcbBlueHistogram, blueHistogram, Color.Blue);
        }

        private void DrawHistogram(PictureBox pcb, int[] histogram, Color color)
        {
            if (pcb.Image != null) pcb.Image.Dispose();
            Bitmap histoBitmap = new Bitmap(pcb.Width, pcb.Height);
            using (Graphics g = Graphics.FromImage(histoBitmap))
            {
                g.Clear(Color.Black);
                int max = 0;
                foreach (int value in histogram) if (value > max) max = value;

                for (int i = 0; i < 256; i++)
                {
                    // Çubuğun yüksekliği.
                    // (mevcut tonun piksel sayısı / en yüksek tonun piksel sayısı) oranıyla bulunur.
                    float height = (max > 0) ? (float)histogram[i] / max * pcb.Height : 0;
                    g.DrawLine(new Pen(color), i, pcb.Height, i, pcb.Height - height);
                }
            }
            pcb.Image = histoBitmap;
        }

        private void HandleHistogramMouseClick(object sender, MouseEventArgs e, TextBox lowerTxt, TextBox upperTxt)
        {
            int grayLevel = e.X;
            if (grayLevel < 0 || grayLevel > 255) return;

            if (e.Button == MouseButtons.Left)
                lowerTxt.Text = grayLevel.ToString();
            else if (e.Button == MouseButtons.Right)
                upperTxt.Text = grayLevel.ToString();
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            try
            {
                int r_lower = int.Parse(txtRedLower.Text);
                int r_upper = int.Parse(txtRedUpper.Text);
                int g_lower = int.Parse(txtGreenLower.Text);
                int g_upper = int.Parse(txtGreenUpper.Text);
                int b_lower = int.Parse(txtBlueLower.Text);
                int b_upper = int.Parse(txtBlueUpper.Text);

                Bitmap resultBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    for (int x = 0; x < originalBitmap.Width; x++)
                    {
                        Color p = originalBitmap.GetPixel(x, y);

                        int r = Stretch(p.R, r_lower, r_upper);
                        int g = Stretch(p.G, g_lower, g_upper);
                        int b = Stretch(p.B, b_lower, b_upper);

                        resultBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pcbResult.Image = resultBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lütfen tüm kanallar için geçerli alt ve üst sınırlar seçin.\n\nHata: " + ex.Message, "Geçersiz Değer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int Stretch(int value, int lower, int upper)
        {
            if (upper == lower) return 0;
            if (value <= lower) return 0;
            if (value >= upper) return 255;

            return (int)(((double)(value - lower) / (upper - lower)) * 255.0);
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
