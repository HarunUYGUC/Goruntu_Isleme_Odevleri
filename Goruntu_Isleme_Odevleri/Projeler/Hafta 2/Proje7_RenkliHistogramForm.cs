using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje7_RenkliHistogramForm : Form
    {
        private PictureBox pcbOriginal, pcbRed, pcbGreen, pcbBlue;
        private Button btnYukle, btnGeri;
        private Label lblOriginal, lblRed, lblGreen, lblBlue;
        private Form haftaFormu;

        public Proje7_RenkliHistogramForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 7: Renkli Resim Histogramları";

            int pcbSize = 300;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblRed = new Label() { Text = "Kırmızı Kanal Histogramı", Location = new Point(margin * 2 + pcbSize + 60, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbRed = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(256, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };

            lblGreen = new Label() { Text = "Yeşil Kanal Histogramı", Location = new Point(margin + 80, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbGreen = new PictureBox() { Location = new Point(margin, margin * 2 + pcbSize), Size = new Size(256, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };

            lblBlue = new Label() { Text = "Mavi Kanal Histogramı", Location = new Point(margin * 2 + pcbSize + 70, margin * 2 + pcbSize - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbBlue = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin * 2 + pcbSize), Size = new Size(256, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };

            int controlsY = this.ClientSize.Height - 60;
            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(256, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 256, controlsY), Size = new Size(256, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblRed, pcbRed, lblGreen, pcbGreen, lblBlue, pcbBlue, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje7_RenkliHistogramForm";
            this.Size = new Size(675, 770);
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
                Bitmap originalBitmap = new Bitmap(dialog.FileName);
                pcbOriginal.Image = originalBitmap;
                CalculateAndDrawHistograms(originalBitmap);
            }
        }

        private void CalculateAndDrawHistograms(Bitmap bmp)
        {
            int[] redHistogram = new int[256];
            int[] greenHistogram = new int[256];
            int[] blueHistogram = new int[256];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    redHistogram[pixel.R]++;
                    greenHistogram[pixel.G]++;
                    blueHistogram[pixel.B]++;
                }
            }

            DrawHistogram(pcbRed, redHistogram, Color.Red);
            DrawHistogram(pcbGreen, greenHistogram, Color.Green);
            DrawHistogram(pcbBlue, blueHistogram, Color.Blue);
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

                for (int i = 0; i < histogram.Length; i++)
                {
                    float height = (max > 0) ? (float)histogram[i] / max * pcb.Height : 0;
                    g.DrawLine(new Pen(color), i, pcb.Height, i, pcb.Height - height);
                }
            }
            pcb.Image = histoBitmap;
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
