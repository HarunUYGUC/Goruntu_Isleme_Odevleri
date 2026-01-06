using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje10_MouseIleDondurmeForm : Form
    {
        private Panel pnlContainer;
        private PictureBox pcbCanvas;
        private Button btnYukle, btnSifirla, btnGeri;
        private Label lblBilgi;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private float accumulatedAngle = 0;
        private float currentRotationDelta = 0;

        private bool isDragging = false;
        private float startMouseAngle = 0;
        private PointF rotationCenter;

        public Proje10_MouseIleDondurmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 10: Mouse ile Etkileşimli Döndürme";

            // 1. PANEL (Kapsayıcı)
            pnlContainer = new Panel()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.FromArgb(40, 40, 40),
                AutoScroll = true 
            };

            // 2. PICTUREBOX (Tuval)
            pcbCanvas = new PictureBox()
            {
                Size = new Size(100, 100),
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.Normal 
            };

            // Titreşimi önlemek için DoubleBuffered ayarı
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, pcbCanvas, new object[] { true });

            // Olayları Bağla
            pcbCanvas.MouseDown += PcbCanvas_MouseDown;
            pcbCanvas.MouseMove += PcbCanvas_MouseMove;
            pcbCanvas.MouseUp += PcbCanvas_MouseUp;
            pcbCanvas.Paint += PcbCanvas_Paint;

            // PictureBox'ı Panelin içine ekle
            pnlContainer.Controls.Add(pcbCanvas);

            // Diğer Kontroller
            lblBilgi = new Label()
            {
                Text = "Resmi yükleyin ve mouse ile çevirin. (Taşma olmaz)",
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

            this.Controls.AddRange(new Control[] { pnlContainer, lblBilgi, btnYukle, btnSifirla, btnGeri });
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
                originalBitmap = new Bitmap(dialog.FileName);

                // Resmin köşegen uzunluğunu hesapla (Pisagor)
                int diagonal = (int)Math.Ceiling(Math.Sqrt(
                    (originalBitmap.Width * originalBitmap.Width) +
                    (originalBitmap.Height * originalBitmap.Height)));

                int safeSize = diagonal + 50;

                pcbCanvas.Size = new Size(safeSize, safeSize);

                rotationCenter = new PointF(safeSize / 2, safeSize / 2);

                accumulatedAngle = 0;
                currentRotationDelta = 0;

                pnlContainer.AutoScrollPosition = new Point(0, 0);

                pcbCanvas.Invalidate();
            }
        }

        private void PcbCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || e.Button != MouseButtons.Left) return;
            isDragging = true;
            startMouseAngle = GetAngleFromPoint(rotationCenter, e.Location);
        }

        private void PcbCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || originalBitmap == null) return;

            float currentMouseAngle = GetAngleFromPoint(rotationCenter, e.Location);
            currentRotationDelta = currentMouseAngle - startMouseAngle;

            lblBilgi.Text = $"Açı: {(accumulatedAngle + currentRotationDelta):0.0}°";
            pcbCanvas.Invalidate();
        }

        private void PcbCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                accumulatedAngle += currentRotationDelta;
                currentRotationDelta = 0;
                isDragging = false;
                pcbCanvas.Invalidate();
            }
        }

        private float GetAngleFromPoint(PointF center, Point target)
        {
            double dx = target.X - center.X;
            double dy = target.Y - center.Y;
            double radians = Math.Atan2(dy, dx);
            return (float)(radians * (180 / Math.PI));
        }

        // --- ÇİZİM İŞLEMİ ---
        private void PcbCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (originalBitmap == null) return;

            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float totalAngle = accumulatedAngle + currentRotationDelta;

            g.TranslateTransform(rotationCenter.X, rotationCenter.Y);

            g.RotateTransform(totalAngle);

            int offsetX = -originalBitmap.Width / 2;
            int offsetY = -originalBitmap.Height / 2;

            g.DrawImage(originalBitmap, offsetX, offsetY);

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