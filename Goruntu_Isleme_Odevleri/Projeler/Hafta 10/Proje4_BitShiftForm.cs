using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje4_BitShiftForm : Form
    {
        private PictureBox pcbOrijinal;
        private PictureBox pcbSonuc;

        private Label lblOrijinal;
        private Label lblSonuc;
        private Label lblMiktar;

        private Button btnResimYukle;
        private Button btnSolaKaydir; // <<
        private Button btnSagaKaydir; // >>
        private Button btnGeri;

        private NumericUpDown numShiftMiktari; // Kaç bit kaydırılacak?

        private Bitmap kaynakResim;
        private Form parentForm;

        public Proje4_BitShiftForm(Form parent)
        {
            InitializeComponent();
            SetupUI();
            parentForm = parent;
        }

        private void InitializeComponent()
        {
            this.Text = "Proje 4: Bit Shift (Bit Kaydırma) İşlemleri";
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += (s, e) => parentForm.Show();
        }

        private void SetupUI()
        {
            int picSize = 300;
            int startY = 40;

            lblOrijinal = new Label { Text = "Orijinal Resim", Location = new Point(30, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOrijinal = CreateBox(30, startY, picSize);

            lblSonuc = new Label { Text = "İşlenmiş Resim (Shifted)", Location = new Point(30 + picSize + 40, 15), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbSonuc = CreateBox(30 + picSize + 40, startY, picSize);

            int controlX = 30 + (picSize * 2) + 80;

            btnResimYukle = new Button { Text = "Resim Yükle", Location = new Point(controlX, startY), Size = new Size(140, 40), BackColor = Color.LightBlue };
            btnResimYukle.Click += BtnResimYukle_Click;

            lblMiktar = new Label { Text = "Kaydırma Miktarı (Bit):", Location = new Point(controlX, startY + 60), AutoSize = true };

            numShiftMiktari = new NumericUpDown();
            numShiftMiktari.Location = new Point(controlX, startY + 80);
            numShiftMiktari.Size = new Size(140, 30);
            numShiftMiktari.Minimum = 1;
            numShiftMiktari.Maximum = 7; // 8 bit kaydırırsak görüntü tamamen gider
            numShiftMiktari.Value = 1;
            this.Controls.Add(numShiftMiktari);

            btnSolaKaydir = new Button { Text = "Sola Kaydır (<<)\n(Parlaklık/Çarpma)", Location = new Point(controlX, startY + 120), Size = new Size(140, 50), BackColor = Color.LightGreen, Enabled = false };
            btnSolaKaydir.Click += (s, e) => BitShiftIslemi(true); // true = sola

            btnSagaKaydir = new Button { Text = "Sağa Kaydır (>>)\n(Koyuluk/Bölme)", Location = new Point(controlX, startY + 180), Size = new Size(140, 50), BackColor = Color.LightYellow, Enabled = false };
            btnSagaKaydir.Click += (s, e) => BitShiftIslemi(false); // false = sağa

            btnGeri = new Button { Text = "Menüye Dön", Location = new Point(controlX, 450), Size = new Size(140, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.Add(lblOrijinal); this.Controls.Add(pcbOrijinal);
            this.Controls.Add(lblSonuc); this.Controls.Add(pcbSonuc);
            this.Controls.Add(btnResimYukle);
            this.Controls.Add(lblMiktar);
            this.Controls.Add(btnSolaKaydir);
            this.Controls.Add(btnSagaKaydir);
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
                kaynakResim = new Bitmap(temp, pcbOrijinal.Size);
                temp.Dispose();

                pcbOrijinal.Image = kaynakResim;
                btnSolaKaydir.Enabled = true;
                btnSagaKaydir.Enabled = true;
            }
        }

        private void BitShiftIslemi(bool solaKaydir)
        {
            if (kaynakResim == null) return;

            int w = kaynakResim.Width;
            int h = kaynakResim.Height;
            int shiftAmount = (int)numShiftMiktari.Value;

            Bitmap sonucResim = new Bitmap(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color piksel = kaynakResim.GetPixel(x, y);

                    int r = piksel.R;
                    int g = piksel.G;
                    int b = piksel.B;

                    if (solaKaydir) // Left Shift (<<)
                    {
                        // Sola kaydırma çarpma işlemidir, sayı büyür.
                        // (byte) cast işlemi taşan bitleri (MSB) atar, bu da "wrap around" etkisi yapar.
                        // Görüntü işleme derslerinde genelde taşan kısmın atılması istenir.
                        r = (byte)(r << shiftAmount);
                        g = (byte)(g << shiftAmount);
                        b = (byte)(b << shiftAmount);
                    }
                    else // Right Shift (>>)
                    {
                        // Sağa kaydırma bölme işlemidir, sayı küçülür.
                        // En önemsiz bitler (LSB) kaybolur.
                        r = r >> shiftAmount;
                        g = g >> shiftAmount;
                        b = b >> shiftAmount;
                    }

                    sonucResim.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            pcbSonuc.Image = sonucResim;
        }
    }
}