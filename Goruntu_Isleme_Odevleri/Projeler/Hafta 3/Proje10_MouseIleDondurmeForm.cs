using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje10_MouseIleDondurmeForm : Form
    {
        private PictureBox pcbCanvas;
        private Button btnYukle, btnSifirla, btnGeri;
        private Label lblBilgi;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private float accumulatedAngle = 0; // Yapılan döndürmelerin toplamı
        private float currentRotationDelta = 0; // Fare hareketiyle oluşan anlık değişim

        private bool isDragging = false;
        private float startMouseAngle = 0; // Tıklama anındaki farenin açısı
        private PointF centerPoint; // Dönme merkezi

        public Proje10_MouseIleDondurmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 10: Mouse ile Etkileşimli Döndürme";

            pcbCanvas = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 50, 50),
                Cursor = Cursors.Hand
            };
            // DoubleBuffered: Titreşimi engellemek için
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, pcbCanvas, new object[] { true });

            pcbCanvas.MouseDown += PcbCanvas_MouseDown;
            pcbCanvas.MouseMove += PcbCanvas_MouseMove;
            pcbCanvas.MouseUp += PcbCanvas_MouseUp;
            pcbCanvas.Paint += PcbCanvas_Paint;

            lblBilgi = new Label()
            {
                Text = "Resmi yükleyin ve mouse ile basılı tutup çevirin.",
                Location = new Point(25, 540),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 570), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnSifirla = new Button() { Text = "Açıyı Sıfırla", Location = new Point(200, 570), Size = new Size(150, 40) };
            btnSifirla.Click += (s, e) => { accumulatedAngle = 0; currentRotationDelta = 0; pcbCanvas.Invalidate(); };

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(475, 570), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbCanvas, lblBilgi, btnYukle, btnSifirla, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje10_MouseIleDondurmeForm";
            this.Size = new Size(670, 680);
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
                // Resmi yükle ve sığacak şekilde yeniden boyutlandır
                Bitmap temp = new Bitmap(dialog.FileName);
                // Resmi PictureBox'ın yarısı kadar boyutta tutalım ki dönerken taşmasın
                int targetWidth = Math.Min(temp.Width, 300);
                int targetHeight = (int)(targetWidth * ((float)temp.Height / temp.Width));
                originalBitmap = new Bitmap(temp, new Size(targetWidth, targetHeight));

                accumulatedAngle = 0;
                currentRotationDelta = 0;

                centerPoint = new PointF(pcbCanvas.Width / 2, pcbCanvas.Height / 2);

                pcbCanvas.Invalidate();
            }
        }

        // FARE OLAYLARI 

        private void PcbCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || e.Button != MouseButtons.Left) return;

            isDragging = true;
            // Tıklanan noktanın merkeze göre açısını hesapla
            startMouseAngle = GetAngleFromPoint(centerPoint, e.Location);
        }

        private void PcbCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || originalBitmap == null) return;

            // Şu anki fare konumunun açısını hesapla
            float currentMouseAngle = GetAngleFromPoint(centerPoint, e.Location);

            // Aradaki fark kadar resmi döndür
            currentRotationDelta = currentMouseAngle - startMouseAngle;

            lblBilgi.Text = $"Açı: {(accumulatedAngle + currentRotationDelta):0.0}°";
            pcbCanvas.Invalidate(); // Yeniden çiz (Paint olayını tetikler)
        }

        private void PcbCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Fare bırakılınca, geçici farkı ana açıya ekle ve farkı sıfırla
                accumulatedAngle += currentRotationDelta;
                currentRotationDelta = 0;
                isDragging = false;
                pcbCanvas.Invalidate();
            }
        }

        // İki nokta arasındaki açıyı (derece cinsinden) hesaplar.
        // Math.Atan2(dy, dx) radyan döndürür, bunu dereceye çeviririz.
        private float GetAngleFromPoint(PointF center, Point target)
        {
            double dx = target.X - center.X;
            double dy = target.Y - center.Y;
            double radians = Math.Atan2(dy, dx);
            return (float)(radians * (180 / Math.PI));
        }

        private void PcbCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (originalBitmap == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float totalAngle = accumulatedAngle + currentRotationDelta;

            // Transformasyon Matrisi Ayarları 
            // Koordinat sisteminin merkezini PictureBox'ın ortasına taşı
            g.TranslateTransform(centerPoint.X, centerPoint.Y);

            // Koordinat sistemini döndür
            g.RotateTransform(totalAngle);

            // Resmin merkezi, koordinat sisteminin merkezine (0,0) gelmeli.
            // Bu yüzden resmi çizmeye (-Width/2, -Height/2) noktasından başlarız.
            int offsetX = -originalBitmap.Width / 2;
            int offsetY = -originalBitmap.Height / 2;

            g.DrawImage(originalBitmap, offsetX, offsetY);

            // Resmin kenarları için kırmızı bir çerçeve
            using (Pen borderPen = new Pen(Color.Red, 3))
            {
                borderPen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(borderPen, offsetX, offsetY, originalBitmap.Width, originalBitmap.Height);
            }

            g.ResetTransform();
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