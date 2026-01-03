using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_MantiksalOperatorForm : Form
    {
        // UI Elemanları (Template'e uygun isimlendirme)
        private PictureBox pcbSensorA, pcbSensorB, pcbResult;
        private Label lblSensorA, lblSensorB, lblResult;
        private Button btnAND, btnOR, btnXOR, btnGeri;
        private Label lblInfo; // Bilgilendirme etiketi

        // Mantıksal işlem için kaynak resimler
        private Bitmap bitmapA;
        private Bitmap bitmapB;

        private Form haftaFormu;

        public Proje1_MantiksalOperatorForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 1: Mantıksal Operatörler (Sensör Senaryosu)";

            // 1. Arayüzü Kur (Template Yapısı)
            SetupUI();

            // 2. Kaynak Resimleri Oluştur (Daire ve Kare)
            GenerateSourceImages();

            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void InitializeComponent()
        {
            this.Name = "Proje1_MantiksalOperatorForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }

        // --- ARAYÜZ OLUŞTURMA (Template Mantığı) ---
        private void SetupUI()
        {
            int margin = 20;
            int pcbSize = 250; // Resim kutusu boyutu

            // Form Boyutunu dinamik ayarla (3 resim yan yana)
            this.Size = new Size((margin * 4) + (pcbSize * 3) + 20, 600);

            // --- 1. SENSÖR A (SOL) ---
            lblSensorA = new Label() { Text = "Sensör A (Hareket)", Location = new Point(margin, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbSensorA = new PictureBox() { Location = new Point(margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };

            // --- 2. SENSÖR B (ORTA) ---
            lblSensorB = new Label() { Text = "Sensör B (Isı)", Location = new Point(margin + pcbSize + margin, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbSensorB = new PictureBox() { Location = new Point(margin + pcbSize + margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };

            // --- 3. SONUÇ (SAĞ) ---
            lblResult = new Label() { Text = "Sonuç (Karar)", Location = new Point(margin + (pcbSize + margin) * 2, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin + (pcbSize + margin) * 2, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };

            // --- KONTROLLER (ALT KISIM) ---
            int ctrlY = margin + pcbSize + 50;
            int btnWidth = 120;
            int btnHeight = 50;

            // Butonları ortalamak için başlangıç X hesabı
            int totalWidth = this.ClientSize.Width;
            int startX = (totalWidth - (btnWidth * 4 + margin * 3)) / 2;

            btnAND = new Button() { Text = "VE (AND)\n(Kesişim)", Location = new Point(startX, ctrlY), Size = new Size(btnWidth, btnHeight), BackColor = Color.LightBlue };
            btnAND.Click += (s, e) => ApplyLogicOperation("AND");

            btnOR = new Button() { Text = "VEYA (OR)\n(Birleşim)", Location = new Point(startX + btnWidth + margin, ctrlY), Size = new Size(btnWidth, btnHeight), BackColor = Color.LightGreen };
            btnOR.Click += (s, e) => ApplyLogicOperation("OR");

            btnXOR = new Button() { Text = "YA DA (XOR)\n(Fark)", Location = new Point(startX + (btnWidth + margin) * 2, ctrlY), Size = new Size(btnWidth, btnHeight), BackColor = Color.LightYellow };
            btnXOR.Click += (s, e) => ApplyLogicOperation("XOR");

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(startX + (btnWidth + margin) * 3, ctrlY), Size = new Size(btnWidth, btnHeight), BackColor = Color.LightCoral };
            btnGeri.Click += BtnGeri_Click;

            lblInfo = new Label() { Text = "Lütfen bir mantıksal işlem seçin...", Location = new Point(margin, ctrlY + btnHeight + 20), Size = new Size(800, 30), Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.Blue, TextAlign = ContentAlignment.MiddleCenter };

            this.Controls.AddRange(new Control[] { lblSensorA, pcbSensorA, lblSensorB, pcbSensorB, lblResult, pcbResult, btnAND, btnOR, btnXOR, btnGeri, lblInfo });
        }

        // --- MANTIK VE İŞLEVSELLİK ---

        private void GenerateSourceImages()
        {
            int w = 200, h = 200;
            bitmapA = new Bitmap(w, h);
            bitmapB = new Bitmap(w, h);

            // A Resmi: Daire
            using (Graphics g = Graphics.FromImage(bitmapA))
            {
                g.Clear(Color.Black);
                g.FillEllipse(Brushes.White, 20, 20, 160, 160);
            }

            // B Resmi: Artı/Kare şekli
            using (Graphics g = Graphics.FromImage(bitmapB))
            {
                g.Clear(Color.Black);
                g.FillRectangle(Brushes.White, 80, 0, 40, 200); // Dikey
                g.FillRectangle(Brushes.White, 0, 80, 200, 40); // Yatay
            }

            pcbSensorA.Image = bitmapA;
            pcbSensorB.Image = bitmapB;
        }

        private void ApplyLogicOperation(string opType)
        {
            int w = bitmapA.Width;
            int h = bitmapA.Height;
            Bitmap resultBmp = new Bitmap(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Pikselleri al
                    Color cA = bitmapA.GetPixel(x, y);
                    Color cB = bitmapB.GetPixel(x, y);

                    // Thresholding (Siyah/Beyaz Ayrımı)
                    // R değeri 128'den büyükse True (Beyaz), değilse False (Siyah)
                    bool valA = cA.R > 128;
                    bool valB = cB.R > 128;
                    bool resVal = false;

                    switch (opType)
                    {
                        case "AND": resVal = valA && valB; break;
                        case "OR": resVal = valA || valB; break;
                        case "XOR": resVal = valA ^ valB; break;
                    }

                    // Sonucu renge çevir
                    resultBmp.SetPixel(x, y, resVal ? Color.White : Color.Black);
                }
            }

            pcbResult.Image = resultBmp;
            lblInfo.Text = $"İşlem Tamamlandı: {opType}";
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}