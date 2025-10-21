using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje8_YapayRenklendirmeForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnRenklendir, btnTemizle, btnGeri;
        private Label lblDurum;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private List<Tuple<Point, Color>> renkNoktalari = new List<Tuple<Point, Color>>();
        private const int MAX_NOKTA = 10;

        public Proje8_YapayRenklendirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 8: Yapay Renklendirme";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pcbResim.Paint += new PaintEventHandler(pcbResim_Paint);
            pcbResim.MouseClick += new MouseEventHandler(pcbResim_MouseClick);

            lblDurum = new Label()
            {
                Text = "Lütfen bir siyah-beyaz resim yükleyin.",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(140, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnRenklendir = new Button() { Text = "Renklendir", Location = new Point(175, 570), Size = new Size(140, 40), Enabled = false, BackColor = Color.LightGreen };
            btnRenklendir.Click += new EventHandler(btnRenklendir_Click);

            btnTemizle = new Button() { Text = "Noktaları Temizle", Location = new Point(325, 570), Size = new Size(140, 40) };
            btnTemizle.Click += new EventHandler(btnTemizle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 175, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, lblDurum, btnYukle, btnRenklendir, btnTemizle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje8_YapayRenklendirmeForm";
            this.Size = new Size(670, 670);
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
                pcbResim.Image = originalBitmap;
                btnTemizle_Click(null, null);
                lblDurum.Text = $"Renk seçmek için resme tıklayın. ({MAX_NOKTA - renkNoktalari.Count} nokta kaldı)";
            }
        }

        private void pcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || renkNoktalari.Count >= MAX_NOKTA) return;

            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    Point imagePoint = ConvertPointToImage(e.Location);
                    renkNoktalari.Add(new Tuple<Point, Color>(imagePoint, colorDialog.Color));

                    int kalan = MAX_NOKTA - renkNoktalari.Count;
                    lblDurum.Text = (kalan > 0)
                        ? $"Renk seçmek için resme tıklayın. ({kalan} nokta kaldı)"
                        : "10 nokta seçildi. 'Renklendir' butonuna basabilirsiniz.";

                    btnRenklendir.Enabled = (renkNoktalari.Count == MAX_NOKTA);
                    pcbResim.Invalidate();
                }
            }
        }

        // Renkelendirme İşlemi
        private void btnRenklendir_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null || renkNoktalari.Count < MAX_NOKTA) return;

            this.Cursor = Cursors.WaitCursor;
            lblDurum.Text = "Renklendirme yapılıyor, lütfen bekleyin...";
            Application.DoEvents();

            Bitmap renkliResim = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    double toplamAgirlik = 0;
                    double toplamR = 0;
                    double toplamG = 0;
                    double toplamB = 0;

                    int hedefGri = originalBitmap.GetPixel(x, y).R;

                    // Ağırlık Hesaplama
                    foreach (var nokta in renkNoktalari)
                    {
                        // Uzaklık Farkı: Mevcut piksel ile renkli nokta arasındaki geometrik uzaklığı hesaplarız (Pisagor teoremi).
                        double mesafe = Math.Sqrt(Math.Pow(x - nokta.Item1.X, 2) + Math.Pow(y - nokta.Item1.Y, 2));

                        // Parlaklık Farkı: Mevcut pikselin gri değeri ile renkli noktanın gri değeri arasındaki farkı hesaplarız.
                        int kaynakGri = originalBitmap.GetPixel(nokta.Item1.X, nokta.Item1.Y).R;
                        double parlaklikFarki = Math.Abs(hedefGri - kaynakGri);

                        // Ağırlık (Etki Faktörü): Bir renkli noktanın mevcut piksel üzerindeki etkisi (ağırlığı) nedir?
                        // Yani bir nokta ne kadar UZAKSA veya parlaklığı ne kadar FARKLIYSA, etkisi o kadar AZALIR.
                        // (+ 0.001, sıfıra bölme hatasını önlemek için küçük bir eklemedir).
                        // Ağırlık = 1 / (uzaklık² + parlaklık_farkı²).
                        double agirlik = 1.0 / (mesafe * mesafe + parlaklikFarki * parlaklikFarki + 0.001);

                        // Ağırlıklı Toplam: Her renk kanalını (R, G, B) kendi ağırlığıyla çarparak toplamlara ekleriz.
                        toplamR += nokta.Item2.R * agirlik;
                        toplamG += nokta.Item2.G * agirlik;
                        toplamB += nokta.Item2.B * agirlik;
                        toplamAgirlik += agirlik;
                    }

                    // Ağırlıklı Ortalama: Toplam ağırlıklı renk değerlerini, toplam ağırlığa bölerek o pikselin nihai rengini buluruz.
                    int r = (int)(toplamR / toplamAgirlik);
                    int g = (int)(toplamG / toplamAgirlik);
                    int b = (int)(toplamB / toplamAgirlik);

                    renkliResim.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            pcbResim.Image = renkliResim;
            lblDurum.Text = "Renklendirme tamamlandı!";
            this.Cursor = Cursors.Default;
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            renkNoktalari.Clear();
            if (originalBitmap != null)
            {
                pcbResim.Image = originalBitmap;
                lblDurum.Text = $"Renk seçmek için resme tıklayın. ({MAX_NOKTA} nokta kaldı)";
            }
            else
            {
                lblDurum.Text = "Lütfen bir siyah-beyaz resim yükleyin.";
            }
            btnRenklendir.Enabled = false;
            pcbResim.Invalidate();
        }

        private void pcbResim_Paint(object sender, PaintEventArgs e)
        {
            foreach (var nokta in renkNoktalari)
            {
                Point pcbPoint = ConvertPointFromImage(nokta.Item1);
                using (SolidBrush brush = new SolidBrush(nokta.Item2))
                {
                    e.Graphics.FillEllipse(brush, pcbPoint.X - 5, pcbPoint.Y - 5, 10, 10);
                }
                using (Pen pen = new Pen(Color.White, 2))
                {
                    e.Graphics.DrawEllipse(pen, pcbPoint.X - 5, pcbPoint.Y - 5, 10, 10);
                }
            }
        }

        private Point ConvertPointToImage(Point pcbPoint)
        {
            if (pcbResim.Image == null) return pcbPoint;
            Size pcbSize = pcbResim.ClientSize;
            Size imgSize = pcbResim.Image.Size;
            float pcbAspect = pcbSize.Width / (float)pcbSize.Height;
            float imgAspect = imgSize.Width / (float)imgSize.Height;
            if (pcbAspect > imgAspect)
            {
                float scale = pcbSize.Height / (float)imgSize.Height;
                float offsetX = (pcbSize.Width - imgSize.Width * scale) / 2f;
                return new Point((int)((pcbPoint.X - offsetX) / scale), (int)(pcbPoint.Y / scale));
            }
            else
            {
                float scale = pcbSize.Width / (float)imgSize.Width;
                float offsetY = (pcbSize.Height - imgSize.Height * scale) / 2f;
                return new Point((int)(pcbPoint.X / scale), (int)((pcbPoint.Y - offsetY) / scale));
            }
        }

        private Point ConvertPointFromImage(Point imgPoint)
        {
            if (pcbResim.Image == null) return imgPoint;
            Size pcbSize = pcbResim.ClientSize;
            Size imgSize = pcbResim.Image.Size;
            float pcbAspect = pcbSize.Width / (float)pcbSize.Height;
            float imgAspect = imgSize.Width / (float)imgSize.Height;
            if (pcbAspect > imgAspect)
            {
                float scale = pcbSize.Height / (float)imgSize.Height;
                float offsetX = (pcbSize.Width - imgSize.Width * scale) / 2f;
                return new Point((int)(imgPoint.X * scale + offsetX), (int)(imgPoint.Y * scale));
            }
            else
            {
                float scale = pcbSize.Width / (float)imgSize.Width;
                float offsetY = (pcbSize.Height - imgSize.Height * scale) / 2f;
                return new Point((int)(imgPoint.X * scale), (int)(imgPoint.Y * scale + offsetY));
            }
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
