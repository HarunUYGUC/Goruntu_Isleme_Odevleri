using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_MantiksalOperatorForm : Form
    {
        private PictureBox pcbResim1, pcbResim2, pcbSonuc;
        private Button btnYukle1, btnYukle2, btnIsle, btnGeri;
        private ComboBox cmbIslem;
        private Label lblR1, lblR2, lblSonuc, lblIslem, lblEsik;
        private TrackBar tbEsik;
        private Form haftaFormu;

        private Bitmap img1, img2;

        public Proje1_MantiksalOperatorForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Ödev 1: Mantıksal Operatörler ile Hareket Algılama";
            this.Size = new Size(1100, 600);
            SetupUI();
        }

        private void SetupUI()
        {
            int margin = 20;
            int pcbSize = 300;

            lblR1 = new Label() { Text = "Görüntü 1 (Önceki An)", Location = new Point(margin, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResim1 = CreatePictureBox(margin, 50, pcbSize);
            btnYukle1 = new Button() { Text = "Yükle 1", Location = new Point(margin, 360), Size = new Size(100, 30) };
            btnYukle1.Click += (s, e) => LoadImage(1);

            lblR2 = new Label() { Text = "Görüntü 2 (Şu Anki An)", Location = new Point(margin + 320, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResim2 = CreatePictureBox(margin + 320, 50, pcbSize);
            btnYukle2 = new Button() { Text = "Yükle 2", Location = new Point(margin + 320, 360), Size = new Size(100, 30) };
            btnYukle2.Click += (s, e) => LoadImage(2);

            lblSonuc = new Label() { Text = "Sonuç (Hareket)", Location = new Point(margin + 640, 20), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.Red };
            pcbSonuc = CreatePictureBox(margin + 640, 50, pcbSize);

            int ctrlY = 420;

            lblEsik = new Label() { Text = "Binary Eşik Değeri (Siyah/Beyaz Ayrımı): 128", Location = new Point(margin, ctrlY), AutoSize = true, Width = 300 };
            tbEsik = new TrackBar() { Location = new Point(margin, ctrlY + 25), Width = 300, Minimum = 0, Maximum = 255, Value = 128 };
            tbEsik.Scroll += (s, e) => lblEsik.Text = $"Binary Eşik Değeri: {tbEsik.Value}";

            lblIslem = new Label() { Text = "Operatör Seçimi:", Location = new Point(350, ctrlY + 10), AutoSize = true };
            cmbIslem = new ComboBox() { Location = new Point(460, ctrlY + 8), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbIslem.Items.AddRange(new object[] { "XOR (Fark/Hareket)", "AND (Kesişim)", "OR (Birleşim)", "NAND", "NOR" });
            cmbIslem.SelectedIndex = 0; // Varsayılan XOR (En iyi hareket algılayıcı)

            btnIsle = new Button() { Text = "MANTIKSAL İŞLEMİ UYGULA", Location = new Point(640, 420), Size = new Size(300, 50), BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnIsle.Click += BtnIsle_Click;

            btnGeri = new Button() { Text = "Menüye Dön", Location = new Point(950, 500), Size = new Size(120, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblR1, pcbResim1, btnYukle1,
                lblR2, pcbResim2, btnYukle2,
                lblSonuc, pcbSonuc,
                lblEsik, tbEsik, lblIslem, cmbIslem, btnIsle, btnGeri
            });

            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private PictureBox CreatePictureBox(int x, int y, int size)
        {
            return new PictureBox()
            {
                Location = new Point(x, y),
                Size = new Size(size, size),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };
        }

        private void InitializeComponent() { this.Name = "Proje1_MantiksalOperatorlerForm"; }

        private void LoadImage(int imgIndex)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(ofd.FileName);
                Bitmap resized = new Bitmap(bmp, new Size(300, 300));

                if (imgIndex == 1) { img1 = resized; pcbResim1.Image = img1; }
                else { img2 = resized; pcbResim2.Image = img2; }
            }
        }

        private void BtnIsle_Click(object sender, EventArgs e)
        {
            if (img1 == null || img2 == null)
            {
                MessageBox.Show("Lütfen her iki resmi de yükleyin.");
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            string op = cmbIslem.SelectedItem.ToString();
            int threshold = tbEsik.Value;

            Bitmap result = new Bitmap(img1.Width, img1.Height);

            for (int y = 0; y < img1.Height; y++)
            {
                for (int x = 0; x < img1.Width; x++)
                {
                    // 1. Pikselleri al ve Griye çevir
                    Color c1 = img1.GetPixel(x, y);
                    Color c2 = img2.GetPixel(x, y);
                    int gray1 = (int)(c1.R * 0.3 + c1.G * 0.59 + c1.B * 0.11);
                    int gray2 = (int)(c2.R * 0.3 + c2.G * 0.59 + c2.B * 0.11);

                    // 2. Binary (0 veya 1) yap. 
                    // Binary mantık: Değer > Eşik ise 1 (Beyaz), değilse 0 (Siyah).

                    bool bit1 = gray1 > threshold; // True = Beyaz(1), False = Siyah(0)
                    bool bit2 = gray2 > threshold;

                    bool resultBit = false;

                    // 3. Mantıksal Operatör Uygula
                    switch (op)
                    {
                        case "AND (Kesişim)": resultBit = bit1 & bit2; break;
                        case "OR (Birleşim)": resultBit = bit1 | bit2; break;
                        case "XOR (Fark/Hareket)": resultBit = bit1 ^ bit2; break; // Farklıysa 1 olur
                        case "NAND": resultBit = !(bit1 & bit2); break;
                        case "NOR": resultBit = !(bit1 | bit2); break;
                    }

                    // 4. Sonucu Pixel Olarak Ata
                    // True ise Beyaz (255), False ise Siyah (0)
                    Color finalColor = resultBit ? Color.White : Color.Black;
                    result.SetPixel(x, y, finalColor);
                }
            }

            pcbSonuc.Image = result;
            this.Cursor = Cursors.Default;
        }
    }
}