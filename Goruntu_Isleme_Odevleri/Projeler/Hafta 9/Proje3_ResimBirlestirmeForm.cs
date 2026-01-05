using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_ResimBirlestirmeForm : Form
    {
        // Kontroller
        private PictureBox pcbResim1;
        private PictureBox pcbResim2;
        private PictureBox pcbSonuc;

        private Button btnResimSec1;
        private Button btnResimSec2;
        private Button btnBirlestir;
        private Button btnGeri;

        private TrackBar tbOran1;
        private TrackBar tbOran2;

        private Label lblOran1;
        private Label lblOran2;

        // Resim verileri
        private Bitmap bmpResim1;
        private Bitmap bmpResim2;

        private Form haftaFormu; // Bir önceki formu tutmak için

        public Proje3_ResimBirlestirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: İki Resmi Ağırlıklı Toplama (Blend)";

            // Arayüz elemanlarını oluştur ve ekle
            ArayuzOlustur();
        }

        private void InitializeComponent()
        {
            this.Name = "Proje3_ResimBirlestirmeForm";
            // 3 Resim sığacağı için formu biraz genişlettim
            this.Size = new Size(1000, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void ArayuzOlustur()
        {
            // --- 1. Resim Alanı (Sol) ---
            pcbResim1 = new PictureBox();
            pcbResim1.Location = new Point(20, 20);
            pcbResim1.Size = new Size(250, 250);
            pcbResim1.BorderStyle = BorderStyle.FixedSingle;
            pcbResim1.SizeMode = PictureBoxSizeMode.Zoom;
            pcbResim1.BackColor = Color.LightGray;
            this.Controls.Add(pcbResim1);

            btnResimSec1 = new Button();
            btnResimSec1.Text = "1. Resmi Yükle";
            btnResimSec1.Location = new Point(20, 280);
            btnResimSec1.Size = new Size(120, 30);
            btnResimSec1.Click += new EventHandler(btnResimSec1_Click);
            this.Controls.Add(btnResimSec1);

            // Slider 1 (Dikey Slider - Örnekteki gibi)
            tbOran1 = new TrackBar();
            tbOran1.Orientation = Orientation.Vertical;
            tbOran1.Minimum = 0;
            tbOran1.Maximum = 100; // %0 ile %100 arası
            tbOran1.Value = 50;    // Varsayılan %50
            tbOran1.Location = new Point(280, 20);
            tbOran1.Size = new Size(45, 250);
            tbOran1.TickFrequency = 10;
            tbOran1.Scroll += (s, e) => lblOran1.Text = "%" + tbOran1.Value.ToString();
            this.Controls.Add(tbOran1);

            lblOran1 = new Label();
            lblOran1.Text = "%50";
            lblOran1.Location = new Point(280, 280);
            lblOran1.Size = new Size(40, 20);
            this.Controls.Add(lblOran1);


            // --- 2. Resim Alanı (Orta) ---
            pcbResim2 = new PictureBox();
            pcbResim2.Location = new Point(340, 20);
            pcbResim2.Size = new Size(250, 250);
            pcbResim2.BorderStyle = BorderStyle.FixedSingle;
            pcbResim2.SizeMode = PictureBoxSizeMode.Zoom;
            pcbResim2.BackColor = Color.LightGray;
            this.Controls.Add(pcbResim2);

            btnResimSec2 = new Button();
            btnResimSec2.Text = "2. Resmi Yükle";
            btnResimSec2.Location = new Point(340, 280);
            btnResimSec2.Size = new Size(120, 30);
            btnResimSec2.Click += new EventHandler(btnResimSec2_Click);
            this.Controls.Add(btnResimSec2);

            // Slider 2
            tbOran2 = new TrackBar();
            tbOran2.Orientation = Orientation.Vertical;
            tbOran2.Minimum = 0;
            tbOran2.Maximum = 100;
            tbOran2.Value = 50;
            tbOran2.Location = new Point(600, 20);
            tbOran2.Size = new Size(45, 250);
            tbOran2.TickFrequency = 10;
            tbOran2.Scroll += (s, e) => lblOran2.Text = "%" + tbOran2.Value.ToString();
            this.Controls.Add(tbOran2);

            lblOran2 = new Label();
            lblOran2.Text = "%50";
            lblOran2.Location = new Point(600, 280);
            lblOran2.Size = new Size(40, 20);
            this.Controls.Add(lblOran2);


            // --- Sonuç Alanı (Sağ) ---
            pcbSonuc = new PictureBox();
            pcbSonuc.Location = new Point(660, 20);
            pcbSonuc.Size = new Size(250, 250);
            pcbSonuc.BorderStyle = BorderStyle.FixedSingle;
            pcbSonuc.SizeMode = PictureBoxSizeMode.Zoom;
            pcbSonuc.BackColor = Color.WhiteSmoke;
            this.Controls.Add(pcbSonuc);

            // İşlem Butonları
            btnBirlestir = new Button();
            btnBirlestir.Text = "RESİMLERİ BİRLEŞTİR";
            btnBirlestir.Font = new Font("Arial", 10, FontStyle.Bold);
            btnBirlestir.Location = new Point(660, 280);
            btnBirlestir.Size = new Size(250, 40);
            btnBirlestir.Click += new EventHandler(btnBirlestir_Click);
            this.Controls.Add(btnBirlestir);

            btnGeri = new Button();
            btnGeri.Text = "Menüye Dön";
            btnGeri.BackColor = Color.LightCoral;
            btnGeri.Location = new Point(830, 380);
            btnGeri.Size = new Size(120, 40);
            btnGeri.Click += new EventHandler(btnGeri_Click);
            this.Controls.Add(btnGeri);
        }

        // 1. Resmi Yükleme Butonu
        private void btnResimSec1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Orjinal resmi Bitmap olarak belleğe alıyoruz
                bmpResim1 = new Bitmap(ofd.FileName);
                pcbResim1.Image = bmpResim1;
            }
        }

        // 2. Resmi Yükleme Butonu
        private void btnResimSec2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Orjinal resmi Bitmap olarak belleğe alıyoruz
                bmpResim2 = new Bitmap(ofd.FileName);
                pcbResim2.Image = bmpResim2;
            }
        }

        // ANA ALGORİTMA: İki resmi ölçekleyerek toplama
        private void btnBirlestir_Click(object sender, EventArgs e)
        {
            if (bmpResim1 == null || bmpResim2 == null)
            {
                MessageBox.Show("Lütfen önce her iki resmi de yükleyiniz.");
                return;
            }

            // İşlem yapabilmek için boyutların eşit olması en idealidir.
            // Ödev kapsamında 1. resmin boyutlarını baz alarak işlem yapalım.
            int genislik = bmpResim1.Width;
            int yukseklik = bmpResim1.Height;

            // İkinci resmi, birinci resmin boyutuna göre yeniden boyutlandıralım (Resize)
            Bitmap bmpResim2_Resized = new Bitmap(bmpResim2, new Size(genislik, yukseklik));
            Bitmap sonucResmi = new Bitmap(genislik, yukseklik);

            // Slider değerlerini 0.0 - 1.0 arasına çeviriyoruz
            double olcek1 = tbOran1.Value / 100.0; // Örn: %80 -> 0.8
            double olcek2 = tbOran2.Value / 100.0; // Örn: %40 -> 0.4

            for (int x = 0; x < genislik; x++)
            {
                for (int y = 0; y < yukseklik; y++)
                {
                    Color p1 = bmpResim1.GetPixel(x, y);
                    Color p2 = bmpResim2_Resized.GetPixel(x, y);

                    // Formül: (Renk1 * Ölçek1) + (Renk2 * Ölçek2)
                    int r = (int)(p1.R * olcek1 + p2.R * olcek2);
                    int g = (int)(p1.G * olcek1 + p2.G * olcek2);
                    int b = (int)(p1.B * olcek1 + p2.B * olcek2);

                    // Değer 255'i geçerse, maksimum 255'e sabitliyoruz (Clamp)
                    if (r > 255) r = 255;
                    if (g > 255) g = 255;
                    if (b > 255) b = 255;

                    sonucResmi.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            pcbSonuc.Image = sonucResmi;
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