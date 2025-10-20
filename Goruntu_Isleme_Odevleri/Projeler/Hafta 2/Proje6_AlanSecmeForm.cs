using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje6_AlanSecmeForm : Form
    {
        private PictureBox pcbResim;
        private Panel pnlControls;
        private Button btnYukle, btnUygula, btnGeri, btnBolgeyiTemizle;
        private TrackBar tbParlaklik, tbKontrast, tbRed, tbGreen, tbBlue;
        private Label lblParlaklik, lblKontrast, lblRed, lblGreen, lblBlue;
        private Form haftaFormu;

        private Bitmap originalBitmap, processedBitmap;
        private List<Point> polygonPoints = new List<Point>();
        private GraphicsPath polygonPath;

        public Proje6_AlanSecmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 6: Alan Seçerek Renk Değiştirme";

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

            pnlControls = new Panel()
            {
                Location = new Point(650, 25),
                Size = new Size(280, 500),
                BorderStyle = BorderStyle.Fixed3D
            };

            lblParlaklik = new Label() { Text = "Parlaklık: 0", Location = new Point(10, 10), AutoSize = true };
            tbParlaklik = new TrackBar() { Location = new Point(5, 30), Size = new Size(260, 45), Minimum = -100, Maximum = 100, Value = 0, TickFrequency = 10 };

            lblKontrast = new Label() { Text = "Kontrast: 0", Location = new Point(10, 80), AutoSize = true };
            tbKontrast = new TrackBar() { Location = new Point(5, 100), Size = new Size(260, 45), Minimum = -100, Maximum = 100, Value = 0, TickFrequency = 10 };

            lblRed = new Label() { Text = "Kırmızı: 0", Location = new Point(10, 150), AutoSize = true };
            tbRed = new TrackBar() { Location = new Point(5, 170), Size = new Size(260, 45), Minimum = -255, Maximum = 255, Value = 0, TickFrequency = 25 };

            lblGreen = new Label() { Text = "Yeşil: 0", Location = new Point(10, 220), AutoSize = true };
            tbGreen = new TrackBar() { Location = new Point(5, 240), Size = new Size(260, 45), Minimum = -255, Maximum = 255, Value = 0, TickFrequency = 25 };

            lblBlue = new Label() { Text = "Mavi: 0", Location = new Point(10, 290), AutoSize = true };
            tbBlue = new TrackBar() { Location = new Point(5, 310), Size = new Size(260, 45), Minimum = -255, Maximum = 255, Value = 0, TickFrequency = 25 };

            tbParlaklik.Scroll += (s, e) => { lblParlaklik.Text = $"Parlaklık: {tbParlaklik.Value}"; };
            tbKontrast.Scroll += (s, e) => { lblKontrast.Text = $"Kontrast: {tbKontrast.Value}"; };
            tbRed.Scroll += (s, e) => { lblRed.Text = $"Kırmızı: {tbRed.Value}"; };
            tbGreen.Scroll += (s, e) => { lblGreen.Text = $"Yeşil: {tbGreen.Value}"; };
            tbBlue.Scroll += (s, e) => { lblBlue.Text = $"Mavi: {tbBlue.Value}"; };

            btnUygula = new Button() { Text = "Değişiklikleri Uygula", Location = new Point(10, 380), Size = new Size(260, 40), BackColor = Color.LightGreen };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnBolgeyiTemizle = new Button() { Text = "Bölgeyi Temizle", Location = new Point(10, 430), Size = new Size(260, 40) };
            btnBolgeyiTemizle.Click += new EventHandler(btnBolgeyiTemizle_Click);

            pnlControls.Controls.AddRange(new Control[] { lblParlaklik, tbParlaklik, lblKontrast, tbKontrast, lblRed, tbRed, lblGreen, tbGreen, lblBlue, tbBlue, btnUygula, btnBolgeyiTemizle });

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 550), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 175, 550), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, pnlControls, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje6_AlanSecmeForm";
            this.Size = new Size(960, 650);
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
                processedBitmap = new Bitmap(originalBitmap);
                pcbResim.Image = processedBitmap;
                btnBolgeyiTemizle_Click(null, null);
            }
        }

        // Tıklanan noktanın koordinatını alıp poligonu
        // oluşturacak köşe noktaları listesine ekler.
        private void pcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (pcbResim.Image == null) return;
            polygonPoints.Add(e.Location);
            pcbResim.Invalidate(); // PictureBox'ı yeniden çizmeye zorlar.
            // Bu sayede "pcbResim_Paint()" metodu otomatik olarak tetiklenir.
        }

        // Kullanıcının seçtiği noktaları birleştiren kesikli çizgileri çizer.
        private void pcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (polygonPoints.Count > 1)
            {
                using (Pen pen = new Pen(Color.Yellow, 2))
                {
                    pen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawLines(pen, polygonPoints.ToArray());
                }
            }
        }

        private void btnBolgeyiTemizle_Click(object sender, EventArgs e)
        {
            polygonPoints.Clear();
            polygonPath = null;
            tbParlaklik.Value = 0;
            tbKontrast.Value = 0;
            tbRed.Value = 0;
            tbGreen.Value = 0;
            tbBlue.Value = 0;
            lblParlaklik.Text = "Parlaklık: 0";
            lblKontrast.Text = "Kontrast: 0";
            lblRed.Text = "Kırmızı: 0";
            lblGreen.Text = "Yeşil: 0";
            lblBlue.Text = "Mavi: 0";

            if (originalBitmap != null)
            {
                processedBitmap = new Bitmap(originalBitmap);
                pcbResim.Image = processedBitmap;
            }
            pcbResim.Invalidate();
        }

        // Ray Casting Algoritması'nı kullanarak seçili poligonu algılayıp
        // içerisinde istenen değişiklikleri yapabiliyoruz.
        private void btnUygula_Click(object sender, EventArgs e)
        {
            // Resim yoksa veya geçerli bir poligon (en az 3 nokta)
            // çizilmediyse uyarı ver ve işlemi durdur.
            if (originalBitmap == null) { MessageBox.Show("Lütfen önce bir resim yükleyin!"); return; }
            if (polygonPoints.Count < 3) { MessageBox.Show("Lütfen en az 3 nokta ile kapalı bir alan seçin!"); return; }

            processedBitmap = new Bitmap(originalBitmap);

            // Eğer poligonun geometrik yolu daha önce hesaplanmadıysa,
            // bir kereye mahsus hesapla ve hafızada tut.
            if (polygonPath == null)
            {
                polygonPath = new GraphicsPath();
                polygonPath.AddPolygon(polygonPoints.ToArray());
            }

            for (int y = 0; y < processedBitmap.Height; y++)
            {
                for (int x = 0; x < processedBitmap.Width; x++)
                {
                    PointF imagePoint = ConvertPointToImage(new Point(x, y));

                    // 'IsVisible', bir noktanın oluşturduğumuz poligonun içinde olup olmadığını
                    // kontrol eden güçlü bir geometrik fonksiyondur.
                    if (polygonPath.IsVisible(imagePoint))
                    {
                        Color originalColor = processedBitmap.GetPixel(x, y);

                        // Renk Değişimi
                        int r = originalColor.R + tbRed.Value;
                        int g = originalColor.G + tbGreen.Value;
                        int b = originalColor.B + tbBlue.Value;

                        // Parlaklık
                        r += tbParlaklik.Value;
                        g += tbParlaklik.Value;
                        b += tbParlaklik.Value;

                        // Kontrast
                        double contrast = (100.0 + tbKontrast.Value) / 100.0;
                        contrast *= contrast;
                        r = (int)(((((r / 255.0) - 0.5) * contrast) + 0.5) * 255.0);
                        g = (int)(((((g / 255.0) - 0.5) * contrast) + 0.5) * 255.0);
                        b = (int)(((((b / 255.0) - 0.5) * contrast) + 0.5) * 255.0);

                        // Değerleri 0-255 aralığında tut
                        r = Math.Max(0, Math.Min(255, r));
                        g = Math.Max(0, Math.Min(255, g));
                        b = Math.Max(0, Math.Min(255, b));

                        processedBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
            }
            pcbResim.Image = processedBitmap;
            pcbResim.Invalidate();
        }

        private PointF ConvertPointToImage(Point pcbPoint)
        {
            if (pcbResim.Image == null) return pcbPoint;

            float pcbAspect = (float)pcbResim.ClientSize.Width / pcbResim.ClientSize.Height;
            float imgAspect = (float)pcbResim.Image.Width / pcbResim.Image.Height;

            if (pcbAspect > imgAspect)
            {
                float scale = (float)pcbResim.ClientSize.Height / pcbResim.Image.Height;
                float offsetX = (pcbResim.ClientSize.Width - (pcbResim.Image.Width * scale)) / 2;
                return new PointF((pcbPoint.X - offsetX) / scale, pcbPoint.Y / scale);
            }
            else
            {
                float scale = (float)pcbResim.ClientSize.Width / pcbResim.Image.Width;
                float offsetY = (pcbResim.ClientSize.Height - (pcbResim.Image.Height * scale)) / 2;
                return new PointF(pcbPoint.X / scale, (pcbPoint.Y - offsetY) / scale);
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
