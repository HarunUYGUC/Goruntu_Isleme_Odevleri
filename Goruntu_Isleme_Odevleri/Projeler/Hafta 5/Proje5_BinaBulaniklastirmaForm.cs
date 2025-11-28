using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_BinaBulaniklastirmaForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnUygula, btnTemizle, btnGeri;
        private Label lblBilgi, lblBlurSiddeti;
        private TrackBar tbBlurSize;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap processedBitmap;

        // Seçim için değişkenler
        private List<Point> polygonPoints = new List<Point>();
        private GraphicsPath polygonPath;

        public Proje5_BinaBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: Bina Netleştirme (Arka Plan Bulanıklaştırma)";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };
            pcbResim.MouseClick += PcbResim_MouseClick;
            pcbResim.Paint += PcbResim_Paint;

            int controlsY = 540;

            lblBilgi = new Label()
            {
                Text = "1. Resim yükleyin. 2. Binanın çevresini noktalarla seçin. 3. 'Uygula' diyerek arka planı bulanıklaştırın.",
                Location = new Point(25, controlsY),
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY + 30), Size = new Size(120, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnTemizle = new Button() { Text = "Seçimi Sıfırla", Location = new Point(155, controlsY + 30), Size = new Size(120, 40) };
            btnTemizle.Click += new EventHandler(btnTemizle_Click);

            lblBlurSiddeti = new Label() { Text = "Bulanıklık Şiddeti:", Location = new Point(300, controlsY + 15), AutoSize = true };
            tbBlurSize = new TrackBar() { Location = new Point(300, controlsY + 35), Size = new Size(150, 45), Minimum = 3, Maximum = 25, Value = 9, TickFrequency = 2, SmallChange = 2 };

            btnUygula = new Button() { Text = "Uygula (Mean Filtresi)", Location = new Point(470, controlsY + 30), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(630, controlsY + 30), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, lblBilgi, btnYukle, btnTemizle, lblBlurSiddeti, tbBlurSize, btnUygula, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje5_BinaBulaniklastirmaForm";
            this.Size = new Size(800, 650);
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
                ResetSelection();
            }
        }

        private void PcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;

            // Sadece sol tık ile nokta ekle
            if (e.Button == MouseButtons.Left)
            {
                polygonPoints.Add(e.Location);

                // En az 3 nokta varsa uygulanabilir
                if (polygonPoints.Count >= 3)
                    btnUygula.Enabled = true;

                pcbResim.Invalidate(); // Çizgileri güncelle
            }
        }

        private void PcbResim_Paint(object sender, PaintEventArgs e)
        {
            if (polygonPoints.Count > 0)
            {
                // Seçilen noktaları ve çizgileri çiz
                using (Pen pen = new Pen(Color.Yellow, 2) { DashStyle = DashStyle.Dash })
                {
                    if (polygonPoints.Count > 1)
                    {
                        e.Graphics.DrawLines(pen, polygonPoints.ToArray());
                        // Poligonu kapatmak için son noktadan ilk noktaya hayali çizgi
                        e.Graphics.DrawLine(Pens.Gray, polygonPoints[polygonPoints.Count - 1], polygonPoints[0]);
                    }

                    foreach (Point p in polygonPoints)
                    {
                        e.Graphics.FillEllipse(Brushes.Red, p.X - 3, p.Y - 3, 6, 6);
                    }
                }
            }
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            ResetSelection();
        }

        private void ResetSelection()
        {
            if (originalBitmap != null)
            {
                processedBitmap = new Bitmap(originalBitmap);
                pcbResim.Image = processedBitmap;
            }
            polygonPoints.Clear();
            polygonPath = null;
            btnUygula.Enabled = false;
            pcbResim.Invalidate();
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null || polygonPoints.Count < 3) return;

            this.Cursor = Cursors.WaitCursor;

            // Poligon Yolunu Oluştur (Ekran Koordinatlarını Resim Koordinatlarına Çevirerek)
            polygonPath = new GraphicsPath();
            List<PointF> imagePoints = new List<PointF>();
            foreach (Point p in polygonPoints)
            {
                imagePoints.Add(ConvertPointToImage(p));
            }
            polygonPath.AddPolygon(imagePoints.ToArray());

            // İşlem Yapılacak Kopya Resmi Oluştur
            processedBitmap = new Bitmap(originalBitmap);

            // Bulanıklık Parametreleri
            int kernelSize = tbBlurSize.Value;
            if (kernelSize % 2 == 0) kernelSize++; // Tek sayı olmalı
            int radius = kernelSize / 2;

            // Piksel Piksel Tarama
            for (int y = 0; y < processedBitmap.Height; y++)
            {
                for (int x = 0; x < processedBitmap.Width; x++)
                {
                    // En kritik nokta: Piksel poligonun DIŞINDA MI?
                    // IsVisible metodu, noktanın poligon içinde olup olmadığını söyler.
                    // Biz "değilse" (!) yani dışındaysa bulanıklaştıracağız.
                    if (!polygonPath.IsVisible(x, y))
                    {
                        // Mean (Ortalama) Filtresi 
                        int rSum = 0, gSum = 0, bSum = 0, count = 0;

                        for (int ky = -radius; ky <= radius; ky++)
                        {
                            for (int kx = -radius; kx <= radius; kx++)
                            {
                                int pX = x + kx;
                                int pY = y + ky;

                                // Sınır kontrolü
                                if (pX >= 0 && pX < processedBitmap.Width && pY >= 0 && pY < processedBitmap.Height)
                                {
                                    Color p = originalBitmap.GetPixel(pX, pY); // Orijinalden oku
                                    rSum += p.R;
                                    gSum += p.G;
                                    bSum += p.B;
                                    count++;
                                }
                            }
                        }

                        processedBitmap.SetPixel(x, y, Color.FromArgb(rSum / count, gSum / count, bSum / count));
                    }
                }
            }

            pcbResim.Image = processedBitmap;
            this.Cursor = Cursors.Default;
        }

        // PictureBox Zoom modunda olduğu için ekran koordinatını resim koordinatına çevirir
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