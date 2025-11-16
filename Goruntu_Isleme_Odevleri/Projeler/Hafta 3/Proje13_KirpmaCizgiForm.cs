using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje13_KirpmaCizgiForm : Form
    {
        private PictureBox pcbOriginal;
        private Button btnYukle, btnGeri;
        private Label lblBilgi;
        private Form haftaFormu;

        private Bitmap originalBitmap;

        private Point startPoint;
        private Rectangle selectionRect;
        private bool isSelecting = false;

        public Proje13_KirpmaCizgiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 13: Çizgili Kırpma Aracı";

            pcbOriginal = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Cross
            };
            pcbOriginal.MouseDown += PcbOriginal_MouseDown;
            pcbOriginal.MouseMove += PcbOriginal_MouseMove;
            pcbOriginal.MouseUp += PcbOriginal_MouseUp;
            pcbOriginal.Paint += PcbOriginal_Paint;

            lblBilgi = new Label()
            {
                Text = "Resim yükleyin ve kırpmak istediğiniz alanı çizin.",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(475, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbOriginal, lblBilgi, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje13_KirpmaCizgiForm";
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
                pcbOriginal.Image = originalBitmap;
                selectionRect = Rectangle.Empty;
            }
        }

        // Fare Olayları (Çizim ve Seçim)

        private void PcbOriginal_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;
            isSelecting = true;
            startPoint = e.Location;
            selectionRect = new Rectangle(e.Location, new Size(0, 0));
            pcbOriginal.Invalidate();
        }

        private void PcbOriginal_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting || originalBitmap == null) return;

            // Dikdörtgeni oluştur (Ters yönde çekmeyi destekler)
            int x = Math.Min(startPoint.X, e.X);
            int y = Math.Min(startPoint.Y, e.Y);
            int w = Math.Abs(startPoint.X - e.X);
            int h = Math.Abs(startPoint.Y - e.Y);

            selectionRect = new Rectangle(x, y, w, h);
            lblBilgi.Text = $"Seçim Boyutu: {w}x{h}";
            pcbOriginal.Invalidate(); // Çizimi tetikler
        }

        private void PcbOriginal_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isSelecting) return;
            isSelecting = false;

            if (selectionRect.Width > 5 && selectionRect.Height > 5)
            {
                CropAndShowImage();
            }
            else
            {
                // Çok küçük seçimleri iptal et
                selectionRect = Rectangle.Empty;
                pcbOriginal.Invalidate();
            }
        }

        // Çizim Metodu (Paint Event)
        private void PcbOriginal_Paint(object sender, PaintEventArgs e)
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                // Dış Çerçeve (Siyah-Beyaz karınca yürüyüşü efekti için 2 kalem)
                using (Pen penWhite = new Pen(Color.White, 2) { DashStyle = DashStyle.DashDot })
                using (Pen penBlack = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dot })
                {
                    e.Graphics.DrawRectangle(penBlack, selectionRect);
                    e.Graphics.DrawRectangle(penWhite, selectionRect);
                }

                // İç Dolgu (Hafif şeffaf beyaz) - Seçili alanı vurgulamak için
                using (Brush brush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                {
                    e.Graphics.FillRectangle(brush, selectionRect);
                }

                // Köşe Tutamakları
                int handleSize = 6;
                e.Graphics.FillRectangle(Brushes.White, selectionRect.X - 3, selectionRect.Y - 3, handleSize, handleSize);
                e.Graphics.FillRectangle(Brushes.White, selectionRect.Right - 3, selectionRect.Bottom - 3, handleSize, handleSize);
                e.Graphics.FillRectangle(Brushes.White, selectionRect.Right - 3, selectionRect.Y - 3, handleSize, handleSize);
                e.Graphics.FillRectangle(Brushes.White, selectionRect.X - 3, selectionRect.Bottom - 3, handleSize, handleSize);
            }
        }

        // Kırpma ve Yeni Pencerede Gösterme
        private void CropAndShowImage()
        {
            if (originalBitmap == null) return;

            // PictureBox koordinatlarını gerçek resim koordinatlarına çevir
            Rectangle realRect = GetImageRectangle(selectionRect);

            if (realRect.Width <= 0 || realRect.Height <= 0) return;

            try
            {
                Bitmap croppedBitmap = new Bitmap(realRect.Width, realRect.Height);
                using (Graphics g = Graphics.FromImage(croppedBitmap))
                {
                    g.DrawImage(originalBitmap, new Rectangle(0, 0, realRect.Width, realRect.Height), realRect, GraphicsUnit.Pixel);
                }

                // Sonucu Yeni Bir Formda Göster (Popup)
                Form resultForm = new Form();
                resultForm.Text = "Kırpılmış Görüntü";
                resultForm.Size = new Size(realRect.Width + 40, realRect.Height + 60);
                resultForm.StartPosition = FormStartPosition.CenterParent;

                PictureBox pcbResult = new PictureBox();
                pcbResult.Image = croppedBitmap;
                pcbResult.Dock = DockStyle.Fill;
                pcbResult.SizeMode = PictureBoxSizeMode.Zoom;

                resultForm.Controls.Add(pcbResult);
                resultForm.ShowDialog(); // Modal olarak aç

                // İşlem bitince seçimi temizle
                selectionRect = Rectangle.Empty;
                pcbOriginal.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kırpma hatası: " + ex.Message);
            }
        }

        // Koordinat Dönüşüm Metodu
        private Rectangle GetImageRectangle(Rectangle pcbRect)
        {
            if (pcbOriginal.Image == null) return Rectangle.Empty;
            Size pcbSize = pcbOriginal.ClientSize;
            Size imgSize = pcbOriginal.Image.Size;
            float scale;
            float offsetX = 0, offsetY = 0;

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

            int realX = (int)((pcbRect.X - offsetX) / scale);
            int realY = (int)((pcbRect.Y - offsetY) / scale);
            int realW = (int)(pcbRect.Width / scale);
            int realH = (int)(pcbRect.Height / scale);

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