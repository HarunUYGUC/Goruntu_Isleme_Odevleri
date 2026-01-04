using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_BitTemizlemeForm : Form
    {
        private PictureBox pcbOrijinal;
        private PictureBox pcbTemizlenmis; // 0. bit atılmış hali
        private PictureBox pcbAtilan;      // Sadece 0. bit (Ne attığımızı görmek için)

        private Label lblOrijinal;
        private Label lblTemizlenmis;
        private Label lblAtilan;

        private Button btnResimYukle;
        private Button btnIslemYap;
        private Button btnGeri;

        private Bitmap kaynakResim;
        private Form parentForm;

        public Proje3_BitTemizlemeForm(Form parent)
        {
            InitializeComponent();
            SetupUI();
            parentForm = parent;
        }

        private void InitializeComponent()
        {
            this.Text = "Proje 3: 0. Bit (LSB) Temizleme";
            this.Size = new Size(1000, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += (s, e) => parentForm.Show();
        }

        private void SetupUI()
        {
            int size = 250;
            int y = 50;

            lblOrijinal = new Label { Text = "Orijinal Resim", Location = new Point(30, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOrijinal = CreateBox(30, y, size);

            lblTemizlenmis = new Label { Text = "0. Bit Temizlendi (Bits 1-7)", Location = new Point(300, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Green };
            pcbTemizlenmis = CreateBox(300, y, size);

            lblAtilan = new Label { Text = "Atılan 0. Bit (Gürültü)", Location = new Point(570, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Red };
            pcbAtilan = CreateBox(570, y, size);

            btnResimYukle = new Button { Text = "Resim Yükle", Location = new Point(850, 50), Size = new Size(120, 50), BackColor = Color.LightBlue };
            btnResimYukle.Click += BtnResimYukle_Click;

            btnIslemYap = new Button { Text = "0. Biti Sil\n(Diğerlerini Tut)", Location = new Point(850, 110), Size = new Size(120, 60), BackColor = Color.LightGreen, Enabled = false };
            btnIslemYap.Click += BtnIslemYap_Click;

            btnGeri = new Button { Text = "Menüye Dön", Location = new Point(850, 400), Size = new Size(120, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.Add(lblOrijinal); this.Controls.Add(pcbOrijinal);
            this.Controls.Add(lblTemizlenmis); this.Controls.Add(pcbTemizlenmis);
            this.Controls.Add(lblAtilan); this.Controls.Add(pcbAtilan);
            this.Controls.Add(btnResimYukle);
            this.Controls.Add(btnIslemYap);
            this.Controls.Add(btnGeri);
        }

        private PictureBox CreateBox(int x, int y, int size)
        {
            return new PictureBox
            {
                Location = new Point(x, y),
                Size = new Size(size, size),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };
        }

        private void BtnResimYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap temp = new Bitmap(ofd.FileName);
                kaynakResim = new Bitmap(temp, pcbOrijinal.Size); // Resmi kutuya sığdır
                temp.Dispose();

                // İşlem kolaylığı için griye çeviriyoruz
                MakeGrayscale(kaynakResim);

                pcbOrijinal.Image = kaynakResim;
                btnIslemYap.Enabled = true;

                // Eski sonuçları temizle
                pcbTemizlenmis.Image = null;
                pcbAtilan.Image = null;
            }
        }

        private void BtnIslemYap_Click(object sender, EventArgs e)
        {
            if (kaynakResim == null) return;

            int w = kaynakResim.Width;
            int h = kaynakResim.Height;

            Bitmap bmpTemiz = new Bitmap(w, h); // 1-7 bitleri
            Bitmap bmpAtilan = new Bitmap(w, h); // Sadece 0. bit

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color pixel = kaynakResim.GetPixel(x, y);
                    int gray = pixel.R; // Gri olduğu için R=G=B

                    // --- MANTIKSAL OPERATÖR İŞLEMİ ---

                    // 1. ADIM: 0. Biti Yok Et (Diğerlerini Koru)
                    // 254 sayısı binary olarak: 11111110
                    // VE (AND) işlemi yaparsak son bit 0 olur, diğerleri aynen kalır.
                    int temizDeger = gray & 254;

                    // 2. ADIM: Sadece 0. Biti Al (Atılanı görmek için)
                    // 1 sayısı binary olarak: 00000001
                    int atilanBit = gray & 1;
                    // Görselleştirmek için 0 olanı siyah, 1 olanı beyaz yap (Contrast Stretching)
                    int atilanGorsel = atilanBit * 255;

                    bmpTemiz.SetPixel(x, y, Color.FromArgb(temizDeger, temizDeger, temizDeger));
                    bmpAtilan.SetPixel(x, y, Color.FromArgb(atilanGorsel, atilanGorsel, atilanGorsel));
                }
            }

            pcbTemizlenmis.Image = bmpTemiz;
            pcbAtilan.Image = bmpAtilan;

            MessageBox.Show("İşlem Tamamlandı!\n\nOrtadaki resim: Orijinal resmin 0. biti silinmiş halidir (Gözle fark edilemeyebilir).\nSağdaki resim: Silinen gürültüdür.", "Bilgi");
        }

        // Yardımcı Metot: Gri Tonlama
        private void MakeGrayscale(Bitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    int avg = (c.R + c.G + c.B) / 3;
                    bmp.SetPixel(x, y, Color.FromArgb(avg, avg, avg));
                }
            }
        }
    }
}