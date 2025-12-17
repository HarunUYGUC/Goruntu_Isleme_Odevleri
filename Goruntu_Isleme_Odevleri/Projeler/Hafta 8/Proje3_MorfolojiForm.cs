using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje3_MorfolojiForm : Form
    {
        private PictureBox pcbOriginal, pcbSonuc;
        private Label lblBaslik, lblRenkAyari;
        private Button btnYukle, btnGurultuEkle, btnUygula, btnGeri;

        private NumericUpDown numRMin, numRMax, numGMin, numGMax, numBMin, numBMax;

        private GroupBox grpSablon;
        private GroupBox grpIslem;

        private RadioButton rbKare, rbArti;
        private RadioButton rbGenislet, rbDaralt;
        private CheckBox chkRenkFiltresi;

        private Bitmap originalBitmap;
        private Form haftaFormu;

        public Proje3_MorfolojiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 3: Renkli Morfolojik İşlemler (Düzeltilmiş)";
            SetupUI();

            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void SetupUI()
        {
            this.Size = new Size(1150, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            int margin = 20;

            pcbOriginal = new PictureBox() { Location = new Point(margin, 50), Size = new Size(350, 350), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            pcbSonuc = new PictureBox() { Location = new Point(margin + 370, 50), Size = new Size(350, 350), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            lblBaslik = new Label() { Text = "Orijinal ve İşlenmiş Görüntü", Location = new Point(margin, 20), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold) };

            int ctrlX = 750;
            int ctrlY = 50;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(ctrlX, ctrlY), Size = new Size(150, 40) };
            btnYukle.Click += BtnYukle_Click;

            btnGurultuEkle = new Button() { Text = "Tuz-Biber Ekle", Location = new Point(ctrlX + 160, ctrlY), Size = new Size(150, 40) };
            btnGurultuEkle.Click += BtnGurultuEkle_Click;

            ctrlY += 60;
            chkRenkFiltresi = new CheckBox() { Text = "Sadece Aşağıdaki Renkleri İşle", Location = new Point(ctrlX, ctrlY), AutoSize = true, ForeColor = Color.Red, Font = new Font("Arial", 9, FontStyle.Bold) };

            ctrlY += 30;
            lblRenkAyari = new Label() { Text = "Renk Aralığı (Sadece filtre seçiliyse)", Location = new Point(ctrlX, ctrlY), AutoSize = true, Font = new Font("Arial", 9) };

            ctrlY += 25;
            CreateRangeControl("Kırmızı (R):", ref numRMin, ref numRMax, ctrlX, ctrlY);
            ctrlY += 35;
            CreateRangeControl("Yeşil (G):", ref numGMin, ref numGMax, ctrlX, ctrlY);
            ctrlY += 35;
            CreateRangeControl("Mavi (B):", ref numBMin, ref numBMax, ctrlX, ctrlY);

            ctrlY += 50;
            grpSablon = new GroupBox() { Text = "Şablon Türü (Kernel)", Location = new Point(ctrlX, ctrlY), Size = new Size(320, 60), Font = new Font("Arial", 9, FontStyle.Bold) };

            rbKare = new RadioButton() { Text = "9'luk Kare (3x3)", Location = new Point(15, 25), Checked = true, AutoSize = true };
            rbArti = new RadioButton() { Text = "5'lik Artı (+)", Location = new Point(160, 25), AutoSize = true };
            grpSablon.Controls.Add(rbKare);
            grpSablon.Controls.Add(rbArti);

            ctrlY += 70;
            grpIslem = new GroupBox() { Text = "Morfolojik İşlem", Location = new Point(ctrlX, ctrlY), Size = new Size(320, 80), Font = new Font("Arial", 9, FontStyle.Bold) };

            rbGenislet = new RadioButton() { Text = "Genişlet (Dilation - Siyahları Yok Et)", Location = new Point(15, 25), Checked = true, AutoSize = true };
            rbDaralt = new RadioButton() { Text = "Daralt (Erosion - Beyazları Yok Et)", Location = new Point(15, 50), AutoSize = true };
            grpIslem.Controls.Add(rbGenislet);
            grpIslem.Controls.Add(rbDaralt);

            ctrlY += 90;
            btnUygula = new Button() { Text = "UYGULA (RENKLİ)", Location = new Point(ctrlX, ctrlY), Size = new Size(320, 50), BackColor = Color.LightGreen, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnUygula.Click += BtnUygula_Click;

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX, ctrlY + 60), Size = new Size(320, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                pcbOriginal, pcbSonuc, lblBaslik, btnYukle, btnGurultuEkle,
                chkRenkFiltresi, lblRenkAyari,
                grpSablon, grpIslem, 
                btnUygula, btnGeri
            });

            this.Controls.Add(numRMin); this.Controls.Add(numRMax);
            this.Controls.Add(numGMin); this.Controls.Add(numGMax);
            this.Controls.Add(numBMin); this.Controls.Add(numBMax);
        }

        private void CreateRangeControl(string text, ref NumericUpDown min, ref NumericUpDown max, int x, int y)
        {
            Label l = new Label() { Text = text, Location = new Point(x, y + 2), Size = new Size(80, 20) };
            this.Controls.Add(l);

            min = new NumericUpDown() { Minimum = 0, Maximum = 255, Value = 0, Location = new Point(x + 90, y), Size = new Size(50, 20) };
            max = new NumericUpDown() { Minimum = 0, Maximum = 255, Value = 255, Location = new Point(x + 160, y), Size = new Size(50, 20) };
        }

        private void InitializeComponent() { this.Name = "Proje3_MorfolojiForm"; }

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(ofd.FileName);
                pcbOriginal.Image = originalBitmap;
            }
        }

        private void BtnGurultuEkle_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            Bitmap noisy = new Bitmap(originalBitmap);
            Random rnd = new Random();
            for (int i = 0; i < 5000; i++)
            {
                int x = rnd.Next(noisy.Width);
                int y = rnd.Next(noisy.Height);
                noisy.SetPixel(x, y, rnd.Next(2) == 0 ? Color.White : Color.Black);
            }
            originalBitmap = noisy;
            pcbOriginal.Image = originalBitmap;
        }

        private void BtnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            pcbSonuc.Image = ApplyColorMorphology(originalBitmap, rbGenislet.Checked, rbKare.Checked, chkRenkFiltresi.Checked);

            this.Cursor = Cursors.Default;
        }

        // --- RENKLİ MORFOLOJİ ALGORİTMASI ---
        private Bitmap ApplyColorMorphology(Bitmap src, bool isDilation, bool isSquare, bool useColorFilter)
        {
            int w = src.Width;
            int h = src.Height;
            Bitmap dest = new Bitmap(w, h);

            int rMin = (int)numRMin.Value; int rMax = (int)numRMax.Value;
            int gMin = (int)numGMin.Value; int gMax = (int)numGMax.Value;
            int bMin = (int)numBMin.Value; int bMax = (int)numBMax.Value;

            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    Color merkezRenk = src.GetPixel(x, y);

                    // Renk Filtresi Kontrolü
                    if (useColorFilter)
                    {
                        if (!(merkezRenk.R >= rMin && merkezRenk.R <= rMax &&
                              merkezRenk.G >= gMin && merkezRenk.G <= gMax &&
                              merkezRenk.B >= bMin && merkezRenk.B <= bMax))
                        {
                            dest.SetPixel(x, y, merkezRenk);
                            continue;
                        }
                    }

                    byte bestR = isDilation ? (byte)0 : (byte)255;
                    byte bestG = isDilation ? (byte)0 : (byte)255;
                    byte bestB = isDilation ? (byte)0 : (byte)255;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (!isSquare && Math.Abs(i) == 1 && Math.Abs(j) == 1) continue;

                            Color c = src.GetPixel(x + i, y + j);

                            if (isDilation)
                            {
                                if (c.R > bestR) bestR = c.R;
                                if (c.G > bestG) bestG = c.G;
                                if (c.B > bestB) bestB = c.B;
                            }
                            else
                            {
                                if (c.R < bestR) bestR = c.R;
                                if (c.G < bestG) bestG = c.G;
                                if (c.B < bestB) bestB = c.B;
                            }
                        }
                    }
                    dest.SetPixel(x, y, Color.FromArgb(bestR, bestG, bestB));
                }
            }
            return dest;
        }
    }
}