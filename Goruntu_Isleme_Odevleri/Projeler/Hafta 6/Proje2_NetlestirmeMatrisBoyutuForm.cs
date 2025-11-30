using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_NetlestirmeMatrisBoyutuForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Button btnYukle, btnUygula, btnGeri;
        private Label lblOriginal, lblResult, lblMatrisBoyutu, lblInfo;
        private NumericUpDown nudMatrisBoyutu;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje2_NetlestirmeMatrisBoyutuForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Netleştirmede Matris Boyutunun Etkisi";

            int pcbSize = 350;
            int margin = 25;

            lblOriginal = new Label() { Text = "Orijinal (Bulanık) Resim", Location = new Point(margin + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            lblResult = new Label() { Text = "Netleştirilmiş Sonuç", Location = new Point(margin * 2 + pcbSize + 100, margin - 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin * 2 + pcbSize, margin), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            int controlsY = margin + pcbSize + 30;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, controlsY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblMatrisBoyutu = new Label() { Text = "Matris Boyutu (Kernel Size):", Location = new Point(200, controlsY + 12), AutoSize = true };

            // Matris boyutu seçimi (3, 5, 7, ... 21 gibi tek sayılar)
            nudMatrisBoyutu = new NumericUpDown()
            {
                Location = new Point(370, controlsY + 10),
                Width = 60,
                Minimum = 3,
                Maximum = 51,
                Value = 3,
                Increment = 2
            };

            btnUygula = new Button() { Text = "Uygula", Location = new Point(450, controlsY), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            lblInfo = new Label()
            {
                Text = "Not: Matris boyutu büyüdükçe daha kalın kenarlar ve geniş alanlar netleşir.",
                Location = new Point(200, controlsY + 60),
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Italic),
                ForeColor = Color.DarkGray
            };

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - margin - 150, controlsY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblResult, pcbResult, btnYukle, lblMatrisBoyutu, nudMatrisBoyutu, btnUygula, lblInfo, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_NetlestirmeMatrisBoyutuForm";
            this.Size = new Size(850, 550);
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
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;

            this.Cursor = Cursors.WaitCursor;
            int kernelSize = (int)nudMatrisBoyutu.Value;

            // Çift sayı girildiyse tek sayıya çevir (Merkezi piksel olması için)
            if (kernelSize % 2 == 0) kernelSize++;

            // Unsharp Masking Tekniği ile Netleştirme
            // Resmi belirtilen matris boyutuyla BULANIKLAŞTIR
            Bitmap blurredBitmap = ApplyMeanFilter(originalBitmap, kernelSize);

            // Orijinal resimden bulanık resmi çıkararak detayları bul ve ekle
            // Formül: Net = Orijinal + (Orijinal - Bulanık) * Kuvvet
            Bitmap sharpenedBitmap = ApplyUnsharpMask(originalBitmap, blurredBitmap, 1.5); // 1.5 kuvvet çarpanı

            pcbResult.Image = sharpenedBitmap;
            this.Cursor = Cursors.Default;
        }

        // Basit Mean (Ortalama) Filtresi - Matris boyutuna göre bulanıklaştırır
        private Bitmap ApplyMeanFilter(Bitmap srcImage, int kernelSize)
        {
            Bitmap dstImage = new Bitmap(srcImage.Width, srcImage.Height);
            int radius = kernelSize / 2;

            // LockBits kullanılmadığı için büyük resimlerde ve büyük matrislerde yavaş olabilir.
            // Ama mantığı anlamak için en temiz yöntem budur.
            for (int y = 0; y < srcImage.Height; y++)
            {
                for (int x = 0; x < srcImage.Width; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, count = 0;

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int pX = x + kx;
                            int pY = y + ky;

                            if (pX >= 0 && pX < srcImage.Width && pY >= 0 && pY < srcImage.Height)
                            {
                                Color p = srcImage.GetPixel(pX, pY);
                                rSum += p.R;
                                gSum += p.G;
                                bSum += p.B;
                                count++;
                            }
                        }
                    }
                    dstImage.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                }
            }
            return dstImage;
        }

        // Unsharp Mask
        private Bitmap ApplyUnsharpMask(Bitmap original, Bitmap blurred, double amount)
        {
            Bitmap result = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color cOrg = original.GetPixel(x, y);
                    Color cBlur = blurred.GetPixel(x, y);

                    // Farkı bul ve ekle
                    int r = (int)(cOrg.R + (cOrg.R - cBlur.R) * amount);
                    int g = (int)(cOrg.G + (cOrg.G - cBlur.G) * amount);
                    int b = (int)(cOrg.B + (cOrg.B - cBlur.B) * amount);

                    // Değerleri sınırla (0-255)
                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return result;
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