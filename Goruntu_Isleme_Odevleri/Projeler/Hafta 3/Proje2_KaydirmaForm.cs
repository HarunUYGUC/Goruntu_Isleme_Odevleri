using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_KaydirmaForm : Form
    {
        private PictureBox pcbResim;
        private Button btnYukle, btnGeri;
        private Form haftaFormu;

        private Bitmap originalBitmap;
        private Bitmap displayBitmap;
        private Point lastMousePos;
        private bool isDragging = false;
        private int offsetX = 0;
        private int offsetY = 0;

        public Proje2_KaydirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Kaydırma ve Döngüsel Taşıma";

            pcbResim = new PictureBox()
            {
                Location = new Point(25, 25),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.AutoSize
            };

            pcbResim.MouseDown += new MouseEventHandler(pcbResim_MouseDown);
            pcbResim.MouseMove += new MouseEventHandler(pcbResim_MouseMove);
            pcbResim.MouseUp += new MouseEventHandler(pcbResim_MouseUp);

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(25, 550), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(475, 550), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            this.Controls.AddRange(new Control[] { pcbResim, btnYukle, btnGeri });
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_KaydirmaForm";
            this.Size = new Size(700, 650);
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

                if (originalBitmap.Width > 600 || originalBitmap.Height > 500)
                {
                    float scale = Math.Min(600f / originalBitmap.Width, 500f / originalBitmap.Height);
                    originalBitmap = new Bitmap(originalBitmap, new Size((int)(originalBitmap.Width * scale), (int)(originalBitmap.Height * scale)));
                }

                displayBitmap = new Bitmap(originalBitmap);
                pcbResim.Image = displayBitmap;
                pcbResim.Size = displayBitmap.Size;
                offsetX = 0;
                offsetY = 0;
            }
        }

        private void pcbResim_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;
            isDragging = true;
            lastMousePos = e.Location;
        }

        private void pcbResim_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || originalBitmap == null) return;

            int deltaX = e.X - lastMousePos.X;
            int deltaY = e.Y - lastMousePos.Y;

            offsetX += deltaX;
            offsetY += deltaY;

            // Ofset değerlerini resim boyutları içinde tut (Döngüsel Kaydırma)
            // Negatif değerler için mod alma işlemi biraz farklıdır, bu yüzden önce genişliği ekliyoruz.
            offsetX = (offsetX % originalBitmap.Width + originalBitmap.Width) % originalBitmap.Width;
            offsetY = (offsetY % originalBitmap.Height + originalBitmap.Height) % originalBitmap.Height;

            ApplyCyclicShift();

            lastMousePos = e.Location;
        }

        private void pcbResim_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // Piksel Tabanlı Döngüsel Kaydırma (Wrapping)
        private void ApplyCyclicShift()
        {
            Bitmap newBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    int sourceX = (x - offsetX + originalBitmap.Width) % originalBitmap.Width;
                    int sourceY = (y - offsetY + originalBitmap.Height) % originalBitmap.Height;

                    Color pixel = originalBitmap.GetPixel(sourceX, sourceY);
                    newBitmap.SetPixel(x, y, pixel);
                }
            }

            pcbResim.Image = newBitmap;
            if (displayBitmap != null && displayBitmap != originalBitmap) displayBitmap.Dispose();
            displayBitmap = newBitmap;
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