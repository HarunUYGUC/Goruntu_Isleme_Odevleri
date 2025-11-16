using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje5_KirpmaForm : Form
    {
        private PictureBox pcbOriginal, pcbCropped;
        private Button btnYukle, btnGeri;
        private Label lblOriginal, lblCropped, lblInfo;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Point startPoint;
        private Rectangle selectionRect;
        private bool isSelecting = false;

        public Proje5_KirpmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 5: Resim Kırpma Aracı";

            lblOriginal = new Label() { Text = "Orijinal Resim (Seçim Yapın)", Location = new Point(25, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox()
            {
                Location = new Point(25, 45),
                Size = new Size(500, 400),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };

            pcbOriginal.MouseDown += new MouseEventHandler(pcbOriginal_MouseDown);
            pcbOriginal.MouseMove += new MouseEventHandler(pcbOriginal_MouseMove);
            pcbOriginal.MouseUp += new MouseEventHandler(pcbOriginal_MouseUp);
            pcbOriginal.Paint += new PaintEventHandler(pcbOriginal_Paint);

            lblCropped = new Label() { Text = "Kırpılan Bölge", Location = new Point(550, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbCropped = new PictureBox()
            {
                Location = new Point(550, 45),
                Size = new Size(300, 300),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            lblInfo = new Label()
            {
                Text = "Seçim Boyutu: 0x0",
                Location = new Point(550, 360),
                AutoSize = true,
                Font = new Font("Arial", 9)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 470), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(700, 470), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { lblOriginal, pcbOriginal, lblCropped, pcbCropped, lblInfo, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje5_KirpmaForm";
            this.Size = new Size(900, 600);
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
                pcbCropped.Image = null;
                selectionRect = Rectangle.Empty;
                lblInfo.Text = "Seçim Boyutu: 0x0";
            }
        }

        // --- Fare Olayları (Seçim İşlemi) ---

        private void pcbOriginal_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;
            isSelecting = true;
            startPoint = e.Location; 
            selectionRect = new Rectangle(e.Location, new Size(0, 0));
            pcbOriginal.Invalidate(); // Çizimi yenile
        }

        private void pcbOriginal_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting || originalBitmap == null) return;

            // Dikdörtgeni oluştur (Ters çekmeye karşı önlem: Math.Min/Abs kullanımı)
            // Math.Min: Başlangıç ve bitiş noktalarından hangisi daha solda/yukarıdaysa onu alır.
            // Math.Abs: Genişlik ve yükseklik her zaman pozitif olmalıdır.
            int x = Math.Min(startPoint.X, e.X);
            int y = Math.Min(startPoint.Y, e.Y);
            int width = Math.Abs(startPoint.X - e.X);
            int height = Math.Abs(startPoint.Y - e.Y);

            selectionRect = new Rectangle(x, y, width, height);
            lblInfo.Text = $"Seçim: {width}x{height}";
            pcbOriginal.Invalidate(); // Kırmızı kutuyu sürekli güncelle (Paint olayını tetikler)
        }

        private void pcbOriginal_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isSelecting) return;
            isSelecting = false;

            if (selectionRect.Width > 1 && selectionRect.Height > 1)
            {
                CropImage();
            }
        }

        private void pcbOriginal_Paint(object sender, PaintEventArgs e)
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
                using (Brush brush = new SolidBrush(Color.FromArgb(50, 255, 0, 0)))
                {
                    e.Graphics.FillRectangle(brush, selectionRect);
                }
            }
        }

        // --- Kırpma Mantığı ---
        private void CropImage()
        {
            if (originalBitmap == null) return;

            // PictureBox koordinatlarını Gerçek Resim koordinatlarına çevir
            // (Çünkü resim Zoom modunda olduğu için ekrandaki 100. piksel, resmin 100. pikseli olmayabilir)
            Rectangle realRect = GetImageRectangle(selectionRect);

            if (realRect.Width <= 0 || realRect.Height <= 0) return;

            try
            {
                Bitmap croppedBitmap = new Bitmap(realRect.Width, realRect.Height);

                // Orijinal resimden o parçayı kopyala
                using (Graphics g = Graphics.FromImage(croppedBitmap))
                {
                    // DrawImage(kaynakResim, hedefAlan, kaynakAlan, birim)
                    g.DrawImage(originalBitmap, new Rectangle(0, 0, realRect.Width, realRect.Height), realRect, GraphicsUnit.Pixel);
                }

                pcbCropped.Image = croppedBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kırpma sırasında hata oluştu (Seçim resim sınırları dışında olabilir): " + ex.Message);
            }
        }

        // Bu metot, PictureBox üzerindeki (Zoom edilmiş) dikdörtgeni, 
        // orijinal resim üzerindeki gerçek piksel koordinatlarına çevirir.
        private Rectangle GetImageRectangle(Rectangle pcbRect)
        {
            if (pcbOriginal.Image == null) return Rectangle.Empty;

            Size pcbSize = pcbOriginal.ClientSize;
            Size imgSize = pcbOriginal.Image.Size;
            float scale;
            float offsetX = 0;
            float offsetY = 0;

            // Zoom oranını ve resmin kutu içindeki boşluklarını (offset) hesapla
            if ((float)pcbSize.Width / pcbSize.Height > (float)imgSize.Width / imgSize.Height)
            {
                scale = (float)pcbSize.Height / imgSize.Height;
                offsetX = (pcbSize.Width - imgSize.Width * scale) / 2;
            }
            else
            {
                scale = (float)pcbSize.Width / imgSize.Width;
                offsetY = (pcbSize.Height - imgSize.Height * scale) / 2;
            }

            // Koordinatları dönüştür: (Ekran Koordinatı - Boşluk) / Ölçek = Gerçek Koordinat
            int realX = (int)((pcbRect.X - offsetX) / scale);
            int realY = (int)((pcbRect.Y - offsetY) / scale);
            int realW = (int)(pcbRect.Width / scale);
            int realH = (int)(pcbRect.Height / scale);

            // Resim dışına taşan kısımları kırp
            realX = Math.Max(0, realX);
            realY = Math.Max(0, realY);
            if (realX + realW > imgSize.Width) realW = imgSize.Width - realX;
            if (realY + realH > imgSize.Height) realH = imgSize.Height - realY;

            return new Rectangle(realX, realY, realW, realH);
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