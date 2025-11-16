using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje8_OlceklemeKutucukForm : Form
    {
        private PictureBox pcbCanvas;
        private Button btnYukle, btnGeri;
        private Label lblInfo;
        private Form haftaFormu;

        private Bitmap originalImage;
        private Rectangle imgRect; // Resmin ekrandaki güncel konumu ve boyutu
        private const int HANDLE_SIZE = 8;

        // Hangi tutamağın tutulduğunu anlamak için Enum
        private enum HandleType { None, TopLeft, TopMiddle, TopRight, MiddleLeft, MiddleRight, BottomLeft, BottomMiddle, BottomRight, Body }
        private HandleType currentDragHandle = HandleType.None;

        private Point dragStartPoint;
        private Rectangle dragStartRect;
        private bool isImageLoaded = false;

        public Proje8_OlceklemeKutucukForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 8: Çapa Noktaları ile Ölçekleme";

            pcbCanvas = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Cursor = Cursors.Default
            };
            // DoubleBuffered: Titreşimi engellemek için çizimi hafızada yapar
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, pcbCanvas, new object[] { true });

            pcbCanvas.Paint += PcbCanvas_Paint;
            pcbCanvas.MouseDown += PcbCanvas_MouseDown;
            pcbCanvas.MouseMove += PcbCanvas_MouseMove;
            pcbCanvas.MouseUp += PcbCanvas_MouseUp;

            lblInfo = new Label()
            {
                Text = "Resim yükleyin. Köşelerden orantılı, kenarlardan serbest boyutlandırın.",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Italic)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(575, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbCanvas, lblInfo, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje8_OlceklemeKutucukForm";
            this.Size = new Size(770, 680);
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
                originalImage = new Bitmap(dialog.FileName);
                // Resmi merkeze ve makul bir boyuta yerleştir
                int w = Math.Min(originalImage.Width, 300);
                int h = (int)(w * ((float)originalImage.Height / originalImage.Width));
                imgRect = new Rectangle(200, 150, w, h);

                isImageLoaded = true;
                pcbCanvas.Invalidate();
            }
        }

        // Çizim İşlemleri
        private void PcbCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (!isImageLoaded) return;

            e.Graphics.DrawImage(originalImage, imgRect);

            using (Pen borderPen = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                e.Graphics.DrawRectangle(borderPen, imgRect);
            }

            foreach (HandleType handle in Enum.GetValues(typeof(HandleType)))
            {
                if (handle == HandleType.None || handle == HandleType.Body) continue;
                Rectangle r = GetHandleRect(handle);
                e.Graphics.FillRectangle(Brushes.White, r);
                e.Graphics.DrawRectangle(Pens.Black, r);
            }
        }

        // Tutamakların koordinatlarını hesaplayan yardımcı metot
        private Rectangle GetHandleRect(HandleType handle)
        {
            int x = 0, y = 0;
            int hw = HANDLE_SIZE / 2; // Yarı genişlik

            switch (handle)
            {
                case HandleType.TopLeft: x = imgRect.Left; y = imgRect.Top; break;
                case HandleType.TopMiddle: x = imgRect.Left + imgRect.Width / 2; y = imgRect.Top; break;
                case HandleType.TopRight: x = imgRect.Right; y = imgRect.Top; break;
                case HandleType.MiddleLeft: x = imgRect.Left; y = imgRect.Top + imgRect.Height / 2; break;
                case HandleType.MiddleRight: x = imgRect.Right; y = imgRect.Top + imgRect.Height / 2; break;
                case HandleType.BottomLeft: x = imgRect.Left; y = imgRect.Bottom; break;
                case HandleType.BottomMiddle: x = imgRect.Left + imgRect.Width / 2; y = imgRect.Bottom; break;
                case HandleType.BottomRight: x = imgRect.Right; y = imgRect.Bottom; break;
            }
            return new Rectangle(x - hw, y - hw, HANDLE_SIZE, HANDLE_SIZE);
        }

        // Hangi tutamağın üzerinde olduğumuzu bulan metot (Hit Test)
        private HandleType GetHandleAtPoint(Point p)
        {
            foreach (HandleType handle in Enum.GetValues(typeof(HandleType)))
            {
                if (handle == HandleType.None || handle == HandleType.Body) continue;
                if (GetHandleRect(handle).Contains(p)) return handle;
            }
            if (imgRect.Contains(p)) return HandleType.Body; // Resmin kendisine tıklanırsa taşıma yapılır
            return HandleType.None;
        }

        // Fare Olayları
        private void PcbCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isImageLoaded) return;
            currentDragHandle = GetHandleAtPoint(e.Location);
            if (currentDragHandle != HandleType.None)
            {
                dragStartPoint = e.Location;
                dragStartRect = imgRect; // Başlangıçtaki resim durumunu kaydet
            }
        }

        private void PcbCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Fare imlecini değiştir
            HandleType hoverHandle = GetHandleAtPoint(e.Location);
            SetCursorForHandle(hoverHandle);

            // Sürükleme işlemi yoksa çık
            if (currentDragHandle == HandleType.None) return;

            int deltaX = e.X - dragStartPoint.X;
            int deltaY = e.Y - dragStartPoint.Y;

            // Yeni dikdörtgeni hesapla
            int x = dragStartRect.X;
            int y = dragStartRect.Y;
            int w = dragStartRect.Width;
            int h = dragStartRect.Height;

            // Orijinal en/boy oranı
            float aspectRatio = (float)dragStartRect.Width / dragStartRect.Height;

            switch (currentDragHandle)
            {
                case HandleType.Body: // Taşıma
                    x += deltaX;
                    y += deltaY;
                    break;

                // KÖŞELER (ORANTILI BÜYÜTME/KÜÇÜLTME)
                case HandleType.BottomRight:
                    w += deltaX;
                    h = (int)(w / aspectRatio); // Oranı koru
                    break;
                case HandleType.BottomLeft:
                    w -= deltaX;
                    h = (int)(w / aspectRatio); // Oranı koru
                    x = dragStartRect.Right - w; // Sağ kenar sabit, sol değişir
                    break;
                case HandleType.TopRight:
                    w += deltaX;
                    h = (int)(w / aspectRatio); // Oranı koru
                    y = dragStartRect.Bottom - h; // Alt kenar sabit, üst değişir
                    break;
                case HandleType.TopLeft:
                    w -= deltaX;
                    h = (int)(w / aspectRatio); // Oranı koru
                    x = dragStartRect.Right - w;
                    y = dragStartRect.Bottom - h;
                    break;

                // KENARLAR (ORANTISIZ SÜNDÜRME)
                case HandleType.MiddleRight:
                    w += deltaX; // Sadece genişlik değişir
                    break;
                case HandleType.BottomMiddle:
                    h += deltaY; // Sadece yükseklik değişir
                    break;
                case HandleType.MiddleLeft:
                    x += deltaX;
                    w -= deltaX;
                    break;
                case HandleType.TopMiddle:
                    y += deltaY;
                    h -= deltaY;
                    break;
            }

            // Minimum boyut kontrolü (resim kaybolmasın diye)
            if (w < 20) w = 20;
            if (h < 20) h = 20;

            imgRect = new Rectangle(x, y, w, h);
            lblInfo.Text = $"Boyut: {w}x{h}";
            pcbCanvas.Invalidate();
        }

        private void PcbCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            currentDragHandle = HandleType.None;
        }

        private void SetCursorForHandle(HandleType handle)
        {
            switch (handle)
            {
                case HandleType.TopLeft:
                case HandleType.BottomRight:
                    pcbCanvas.Cursor = Cursors.SizeNWSE; break;
                case HandleType.TopRight:
                case HandleType.BottomLeft:
                    pcbCanvas.Cursor = Cursors.SizeNESW; break;
                case HandleType.TopMiddle:
                case HandleType.BottomMiddle:
                    pcbCanvas.Cursor = Cursors.SizeNS; break;
                case HandleType.MiddleLeft:
                case HandleType.MiddleRight:
                    pcbCanvas.Cursor = Cursors.SizeWE; break;
                case HandleType.Body:
                    pcbCanvas.Cursor = Cursors.SizeAll; break;
                default:
                    pcbCanvas.Cursor = Cursors.Default; break;
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