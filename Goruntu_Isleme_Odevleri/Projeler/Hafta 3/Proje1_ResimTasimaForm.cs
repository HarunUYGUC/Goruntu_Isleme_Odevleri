using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_ResimTasimaForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnGeri;
        private CheckBox chkDragMode;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Point imageOffset = Point.Empty; // Resmin kayma (pan) miktarı

        // Tıklayarak Taşıma
        private Point firstClickControl = Point.Empty;
        private Point firstClickImage = Point.Empty;
        private Point secondClickControl = Point.Empty;

        // Sürükleyerek Taşıma
        private bool isDragging = false;
        private Point lastMousePosition = Point.Empty;

        public Proje1_ResimTasimaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 1: Resmi Taşıma (Tıklama ve Sürükleme)";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(700, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.DarkGray
            };
            pcbResim.Paint += new PaintEventHandler(pcbResim_Paint);
            pcbResim.MouseClick += new MouseEventHandler(pcbResim_MouseClick);
            pcbResim.MouseDown += new MouseEventHandler(pcbResim_MouseDown);
            pcbResim.MouseMove += new MouseEventHandler(pcbResim_MouseMove);
            pcbResim.MouseUp += new MouseEventHandler(pcbResim_MouseUp);

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 540), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            chkDragMode = new CheckBox() { Text = "Basılı Tutarak Taşıma Modu (b)", Location = new Point(200, 548), AutoSize = true, Font = new Font("Arial", 10) };
            chkDragMode.CheckedChanged += (s, e) => { ResetClicks(); };

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 175, 540), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, btnYukle, chkDragMode, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje1_ResimTasimaForm";
            this.Size = new Size(760, 640);
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
                ResetClicks();
                imageOffset = Point.Empty; // Resmi başa al
                pcbResim.Invalidate(); // Yeniden çiz
            }
        }

        // Tıklayarak Taşıma
        private void pcbResim_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || chkDragMode.Checked) return;

            if (firstClickControl.IsEmpty)
            {
                firstClickControl = e.Location;
                // Tıklanan noktanın resim üzerindeki gerçek koordinatı
                firstClickImage = new Point(e.X - imageOffset.X, e.Y - imageOffset.Y);
                secondClickControl = Point.Empty;
            }
            else
            {
                secondClickControl = e.Location;
                // Resmin yeni ofseti
                // Resmin (firstClickImage.X) noktasının, (secondClickControl.X) konumuna gelmesini istiyoruz.
                // Ofset = Hedef - Kaynak
                imageOffset.X = secondClickControl.X - firstClickImage.X;
                imageOffset.Y = secondClickControl.Y - firstClickImage.Y;
                ResetClicks();
            }
            pcbResim.Invalidate();
        }

        // Sürükleyerek Taşıma
        private void pcbResim_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null || !chkDragMode.Checked || e.Button != MouseButtons.Left) return;
            isDragging = true;
            lastMousePosition = e.Location;
        }

        private void pcbResim_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            int deltaX = e.X - lastMousePosition.X;
            int deltaY = e.Y - lastMousePosition.Y;

            imageOffset.X += deltaX;
            imageOffset.Y += deltaY;

            lastMousePosition = e.Location;
            pcbResim.Invalidate();
        }

        private void pcbResim_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // Görüntüyü ve Kareleri Çizen Metot
        private void pcbResim_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.DarkGray);
            if (originalBitmap == null) return;

            e.Graphics.DrawImage(originalBitmap, imageOffset.X, imageOffset.Y);

            if (!chkDragMode.Checked)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    if (!firstClickControl.IsEmpty)
                        e.Graphics.DrawRectangle(pen, firstClickControl.X - 3, firstClickControl.Y - 3, 6, 6);
                    if (!secondClickControl.IsEmpty)
                        e.Graphics.DrawRectangle(pen, secondClickControl.X - 3, secondClickControl.Y - 3, 6, 6);
                }
            }
        }

        private void ResetClicks()
        {
            firstClickControl = Point.Empty;
            firstClickImage = Point.Empty;
            secondClickControl = Point.Empty;
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