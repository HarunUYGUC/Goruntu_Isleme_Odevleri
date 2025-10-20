using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_KontrastGermeForm : Form
    {
        private PictureBox pcbResult, pcbHistogram;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblX1, lblX2;
        private TextBox txtX1, txtX2;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap grayBitmap;
        private int x1 = -1, x2 = -1; // Başlangıçta sınır değerleri tanımsız

        public Proje4_KontrastGermeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 4: İnteraktif Kontrast Germe";

            pcbResult = new PictureBox() { Location = new Point(25, 25), Size = new Size(512, 512), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            pcbHistogram = new PictureBox() { Location = new Point(562, 25), Size = new Size(256, 200), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black };
            pcbHistogram.MouseClick += new MouseEventHandler(pcbHistogram_MouseClick);

            lblX1 = new Label() { Text = "X1 (Sol Tık):", Location = new Point(562, 240), AutoSize = true };
            txtX1 = new TextBox() { Location = new Point(650, 238), Width = 168, ReadOnly = true };

            lblX2 = new Label() { Text = "X2 (Sağ Tık):", Location = new Point(562, 270), AutoSize = true };
            txtX2 = new TextBox() { Location = new Point(650, 268), Width = 168, ReadOnly = true };

            btnUygula = new Button() { Text = "Kontrast Uygula", Location = new Point(562, 310), Size = new Size(256, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 550), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 175, 550), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResult, pcbHistogram, lblX1, txtX1, lblX2, txtX2, btnUygula, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje4_KontrastGermeForm";
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
                pcbResult.Image = grayBitmap;
                DrawHistogram();
                btnUygula.Enabled = true;
                x1 = -1; x2 = -1;
                txtX1.Text = ""; txtX2.Text = "";
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

        private void DrawHistogram()
        {
            if (grayBitmap == null) return;

            int[] histogram = new int[256];
            for (int y = 0; y < grayBitmap.Height; y++)
            {
                for (int x = 0; x < grayBitmap.Width; x++)
                {
                    histogram[grayBitmap.GetPixel(x, y).R]++;
                }
            }

            if (pcbHistogram.Image != null) pcbHistogram.Image.Dispose();
            Bitmap histoBitmap = new Bitmap(pcbHistogram.Width, pcbHistogram.Height);
            using (Graphics g = Graphics.FromImage(histoBitmap))
            {
                g.Clear(Color.Black);
                int max = 0;
                foreach (int value in histogram) if (value > max) max = value;

                for (int i = 0; i < 256; i++)
                {
                    float height = (max > 0) ? (float)histogram[i] / max * pcbHistogram.Height : 0;
                    g.DrawLine(Pens.White, i, pcbHistogram.Height, i, pcbHistogram.Height - height);
                }
            }
            pcbHistogram.Image = histoBitmap;
        }

        private void pcbHistogram_MouseClick(object sender, MouseEventArgs e)
        {
            int grayLevel = e.X;
            if (grayLevel < 0 || grayLevel > 255) return;

            if (e.Button == MouseButtons.Left)
            {
                x1 = grayLevel;
                txtX1.Text = x1.ToString();
            }
            else if (e.Button == MouseButtons.Right)
            {
                x2 = grayLevel;
                txtX2.Text = x2.ToString();
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (grayBitmap == null || x1 == -1 || x2 == -1)
            {
                MessageBox.Show("Lütfen bir resim yükleyin ve histogram üzerinden X1 (sol tık) ve X2 (sağ tık) sınırlarını seçin.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // X1 her zaman X2'den küçük olmalı
            int lowerBound = Math.Min(x1, x2);
            int upperBound = Math.Max(x1, x2);

            Bitmap resultBitmap = new Bitmap(grayBitmap.Width, grayBitmap.Height);

            for (int y = 0; y < grayBitmap.Height; y++)
            {
                for (int x = 0; x < grayBitmap.Width; x++)
                {
                    int originalGray = grayBitmap.GetPixel(x, y).R;
                    int newGray;

                    if (originalGray <= lowerBound) newGray = 0;
                    else if (originalGray >= upperBound) newGray = 255;
                    else
                    {
                        if (upperBound == lowerBound)
                            newGray = 0;
                        else
                            newGray = (int)(((double)(originalGray - lowerBound) / (upperBound - lowerBound)) * 255.0);
                    }

                    newGray = Math.Max(0, Math.Min(255, newGray));
                    resultBitmap.SetPixel(x, y, Color.FromArgb(newGray, newGray, newGray));
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
