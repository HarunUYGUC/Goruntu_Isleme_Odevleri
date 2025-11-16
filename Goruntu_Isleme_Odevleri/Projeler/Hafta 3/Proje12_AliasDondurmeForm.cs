using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje12_AliasDondurmeForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnDondurMerkez, btnDondurKose, btnGeri;
        private Label lblAcı, lblDurum;
        private TextBox txtAcı;
        private CheckBox chkAliasDuzelt;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        public Proje12_AliasDondurmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 12: Alias Düzeltme ve Köşeden Döndürme";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.AutoSize
            };
            // Resim büyükse taşabilir, bu yüzden panel içine alalım
            Panel pnlContainer = new Panel()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                AutoScroll = true,
                BorderStyle = BorderStyle.Fixed3D
            };
            pnlContainer.Controls.Add(pcbResim);

            int controlsY = 555;

            lblDurum = new Label() { Text = "Lütfen bir resim yükleyin.", Location = new Point(25, controlsY - 20), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, controlsY), Size = new Size(100, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblAcı = new Label() { Text = "Açı:", Location = new Point(140, controlsY + 10), AutoSize = true };
            txtAcı = new TextBox() { Location = new Point(175, controlsY + 8), Width = 40, Text = "30" };

            chkAliasDuzelt = new CheckBox() { Text = "Alias (Boşluk) Düzelt", Location = new Point(230, controlsY + 10), AutoSize = true, Checked = true };

            btnDondurMerkez = new Button() { Text = "Merkezden Döndür (a)", Location = new Point(380, controlsY), Size = new Size(140, 40), BackColor = Color.LightBlue, Enabled = false };
            btnDondurMerkez.Click += new EventHandler(btnDondurMerkez_Click);

            btnDondurKose = new Button() { Text = "Köşeden Döndür (b)", Location = new Point(530, controlsY), Size = new Size(140, 40), BackColor = Color.LightGreen, Enabled = false };
            btnDondurKose.Click += new EventHandler(btnDondurKose_Click);

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(680, controlsY), Size = new Size(100, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pnlContainer, lblDurum, btnYukle, lblAcı, txtAcı, chkAliasDuzelt, btnDondurMerkez, btnDondurKose, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje12_AliasDondurmeForm";
            this.Size = new Size(820, 640);
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
                btnDondurMerkez.Enabled = true;
                btnDondurKose.Enabled = true;
                lblDurum.Text = "Resim yüklendi.";
            }
        }

        // MERKEZDEN DÖNDÜRME VE ALIAS DÜZELTME 
        private void btnDondurMerkez_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            if (!double.TryParse(txtAcı.Text, out double angleDeg)) { MessageBox.Show("Geçersiz açı!"); return; }

            double angleRad = angleDeg * Math.PI / 180.0;
            double cosA = Math.Cos(angleRad);
            double sinA = Math.Sin(angleRad);

            // Döndürülmüş resmin sığacağı alanı hesapla
            // Köşelerin yeni koordinatlarını bularak en geniş sınırları hesaplarız.
            double w = originalBitmap.Width;
            double h = originalBitmap.Height;

            // Köşelerin merkeze göre koordinatları
            double cx = w / 2.0;
            double cy = h / 2.0;

            // Köşelerin yeni konumlarını hesapla
            double[] xCoords = new double[4];
            double[] yCoords = new double[4];

            // (0,0), (w,0), (0,h), (w,h) noktalarını döndür
            PointF[] corners = { new PointF(0, 0), new PointF((float)w, 0), new PointF(0, (float)h), new PointF((float)w, (float)h) };

            for (int i = 0; i < 4; i++)
            {
                double x = corners[i].X - cx;
                double y = corners[i].Y - cy;
                xCoords[i] = x * cosA - y * sinA + cx;
                yCoords[i] = x * sinA + y * cosA + cy;
            }

            double minX = xCoords[0], maxX = xCoords[0], minY = yCoords[0], maxY = yCoords[0];
            for (int i = 1; i < 4; i++)
            {
                if (xCoords[i] < minX) minX = xCoords[i];
                if (xCoords[i] > maxX) maxX = xCoords[i];
                if (yCoords[i] < minY) minY = yCoords[i];
                if (yCoords[i] > maxY) maxY = yCoords[i];
            }

            int newWidth = (int)Math.Ceiling(maxX - minX);
            int newHeight = (int)Math.Ceiling(maxY - minY);

            Bitmap rotatedBmp = new Bitmap(newWidth, newHeight);

            // Döndürme İşlemi (İleri Eşleme - Forward Mapping)
            // Bu yöntem boşluk (alias) oluşturur.
            // Her kaynak pikseli alıp hedefte nereye gideceğini hesaplıyoruz.

            double offsetX = (newWidth - w) / 2.0;
            double offsetY = (newHeight - h) / 2.0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Merkezi orijine taşı
                    double xt = x - cx;
                    double yt = y - cy;

                    // Döndür
                    double xnew = xt * cosA - yt * sinA;
                    double ynew = xt * sinA + yt * cosA;

                    // Merkezi geri taşı ve yeni boyuta göre ofset ekle
                    xnew += cx + offsetX;
                    ynew += cy + offsetY;

                    int xDest = (int)Math.Round(xnew);
                    int yDest = (int)Math.Round(ynew);

                    if (xDest >= 0 && xDest < newWidth && yDest >= 0 && yDest < newHeight)
                    {
                        rotatedBmp.SetPixel(xDest, yDest, originalBitmap.GetPixel(x, y));
                    }
                }
            }

            // Alias (Boşluk) Düzeltme
            if (chkAliasDuzelt.Checked)
            {
                rotatedBmp = FillHoles(rotatedBmp);
            }

            pcbResim.Image = rotatedBmp;
        }

        // ALIAS DÜZELTME ALGORİTMASI 
        // BU ALGORİTMA, RESİMDEKİ SİYAH (BOŞ) PİKSELLERİ BULUR.
        // EĞER BİR PİKSEL SİYAHSA VE ETRAFINDAKİ (SAĞ, SOL, ÜST, ALT)
        // PİKSELLERDEN EN AZ BİRİ DOLUYSA, O DOLU PİKSELLERİN
        // RENK ORTALAMASINI ALARAK BOŞ PİKSELİ DOLDURUR.
        private Bitmap FillHoles(Bitmap bmp)
        {
            Bitmap fixedBmp = new Bitmap(bmp);
            // Siyah (0,0,0,0) boş piksel kabul edilir.

            for (int y = 1; y < bmp.Height - 1; y++)
            {
                for (int x = 1; x < bmp.Width - 1; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    // Eğer piksel tamamen siyahsa (veya alfa 0 ise) boşluk olabilir
                    if (c.A == 0 || (c.R == 0 && c.G == 0 && c.B == 0))
                    {
                        int r = 0, g = 0, b = 0, count = 0;

                        // 4-Komşuluk Kontrolü (Üst, Alt, Sol, Sağ)
                        Color[] neighbors = {
                            bmp.GetPixel(x, y - 1),
                            bmp.GetPixel(x, y + 1),
                            bmp.GetPixel(x - 1, y),
                            bmp.GetPixel(x + 1, y)
                        };

                        foreach (Color n in neighbors)
                        {
                            // Komşu doluysa hesaba kat
                            if (!(n.A == 0 || (n.R == 0 && n.G == 0 && n.B == 0)))
                            {
                                r += n.R; g += n.G; b += n.B;
                                count++;
                            }
                        }

                        if (count > 0)
                        {
                            fixedBmp.SetPixel(x, y, Color.FromArgb(r / count, g / count, b / count));
                        }
                    }
                }
            }
            return fixedBmp;
        }


        // KÖŞEDEN (KAYDIRARAK) DÖNDÜRME 
        private void btnDondurKose_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            if (!double.TryParse(txtAcı.Text, out double angleDeg)) { MessageBox.Show("Geçersiz açı!"); return; }

            double angleRad = angleDeg * Math.PI / 180.0;
            double cosA = Math.Cos(angleRad);
            double sinA = Math.Sin(angleRad);

            // Köşeden döndürme formülü:
            // x2 = x1 * cos(theta) - y1 * sin(theta)
            // y2 = x1 * sin(theta) + y1 * cos(theta)
            // Bu formül (0,0) yani sol üst köşe etrafında döndürür.

            int w = originalBitmap.Width;
            int h = originalBitmap.Height;

            // Yeni sınırları hesapla (sadece pozitif bölgede kalmayabilir, ofset gerekebilir)
            // Köşeler: (0,0), (w,0), (0,h), (w,h)
            PointF[] corners = {
                RotatePoint(0, 0, sinA, cosA),
                RotatePoint(w, 0, sinA, cosA),
                RotatePoint(0, h, sinA, cosA),
                RotatePoint(w, h, sinA, cosA)
            };

            float minX = corners[0].X, maxX = corners[0].X, minY = corners[0].Y, maxY = corners[0].Y;
            foreach (var p in corners)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            int newWidth = (int)Math.Ceiling(maxX - minX);
            int newHeight = (int)Math.Ceiling(maxY - minY);

            Bitmap rotatedBmp = new Bitmap(newWidth, newHeight);

            // Ters Eşleme (Inverse Mapping) Kullanıyoruz
            // "köşeden eğerek" (rotation matrix around corner)
            // Ters eşleme ile boşluk sorunu (alias) otomatik olarak çözülür, (a) şıkkındaki hatayı almayız.

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // Hedef resimdeki (x,y) noktasını, ofset ile düzelt
                    double targetX = x + minX;
                    double targetY = y + minY;

                    // Ters döndürme formülü (açı yerine -açı kullanılır)
                    // x1 = x2 * cos(theta) + y2 * sin(theta)
                    // y1 = -x2 * sin(theta) + y2 * cos(theta)

                    double sourceX = targetX * cosA + targetY * sinA;
                    double sourceY = -targetX * sinA + targetY * cosA;

                    // En yakın komşu (Nearest Neighbor)
                    int srcX = (int)Math.Round(sourceX);
                    int srcY = (int)Math.Round(sourceY);

                    if (srcX >= 0 && srcX < w && srcY >= 0 && srcY < h)
                    {
                        rotatedBmp.SetPixel(x, y, originalBitmap.GetPixel(srcX, srcY));
                    }
                }
            }

            pcbResim.Image = rotatedBmp;
        }

        private PointF RotatePoint(float x, float y, double sin, double cos)
        {
            return new PointF((float)(x * cos - y * sin), (float)(x * sin + y * cos));
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