using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje8_FiligranForm : Form
    {

        private PictureBox pcbKaynak, pcbSonuc;
        private Label lblKaynak, lblSonuc, lblAyarlar;
        private Button btnResimYukle, btnKaydet, btnRenkSec, btnGeri;


        private TextBox txtMetin;
        private TrackBar tbSaydamlik, tbAci, tbBoyut, tbKonumX, tbKonumY;
        private Label lblSaydamlik, lblAci, lblBoyut, lblKonumX, lblKonumY;


        private Bitmap bmpKaynak;
        private Color yaziRengi = Color.Red;
        private Form haftaFormu;

        public Proje8_FiligranForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 8: Gelişmiş Filigran";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();
            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void InitializeComponent() { this.Name = "Proje8_FiligranForm"; }

        private void SetupUI()
        {
            int margin = 20;
            int boxSize = 400;

            // 1. Kaynak Resim
            lblKaynak = new Label() { Text = "1. Kaynak Resim", Location = new Point(margin, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbKaynak = CreateBox(margin, 45, boxSize, boxSize);
            btnResimYukle = new Button() { Text = "Resim Yükle", Location = new Point(margin, 45 + boxSize + 10), Size = new Size(boxSize, 40) };
            btnResimYukle.Click += (s, e) => LoadImageSafe(ref bmpKaynak, pcbKaynak);

            // 2. Sonuç Resim
            int x2 = margin + boxSize + 30;
            lblSonuc = new Label() { Text = "2. Filigranlı Sonuç", Location = new Point(x2, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pcbSonuc = CreateBox(x2, 45, boxSize, boxSize);

            // 3. Ayar Paneli
            int x3 = x2 + boxSize + 20;
            int ctrlW = 250;
            int currentY = 45;

            lblAyarlar = new Label() { Text = "AYARLAR", Location = new Point(x3, 20), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkBlue };
            this.Controls.Add(lblAyarlar);

            // Metin
            Label l1 = new Label() { Text = "Metin:", Location = new Point(x3, currentY), AutoSize = true };
            txtMetin = new TextBox() { Text = "FİLİGRAN", Location = new Point(x3, currentY + 20), Size = new Size(ctrlW, 30) };
            txtMetin.TextChanged += UpdateWatermark;
            currentY += 60;

            // Konum X (Yatay)
            lblKonumX = new Label() { Text = "Yatay Konum (X): %50", Location = new Point(x3, currentY), AutoSize = true };
            tbKonumX = new TrackBar() { Location = new Point(x3, currentY + 20), Size = new Size(ctrlW, 45), Minimum = 0, Maximum = 100, Value = 50, TickStyle = TickStyle.None };
            tbKonumX.Scroll += (s, e) => { lblKonumX.Text = $"Yatay Konum (X): %{tbKonumX.Value}"; UpdateWatermark(s, e); };
            currentY += 60;

            // Konum Y (Dikey)
            lblKonumY = new Label() { Text = "Dikey Konum (Y): %50", Location = new Point(x3, currentY), AutoSize = true };
            tbKonumY = new TrackBar() { Location = new Point(x3, currentY + 20), Size = new Size(ctrlW, 45), Minimum = 0, Maximum = 100, Value = 50, TickStyle = TickStyle.None };
            tbKonumY.Scroll += (s, e) => { lblKonumY.Text = $"Dikey Konum (Y): %{tbKonumY.Value}"; UpdateWatermark(s, e); };
            currentY += 60;

            // Boyut
            lblBoyut = new Label() { Text = "Boyut: 60", Location = new Point(x3, currentY), AutoSize = true };
            tbBoyut = new TrackBar() { Location = new Point(x3, currentY + 20), Size = new Size(ctrlW, 45), Minimum = 10, Maximum = 200, Value = 60, TickStyle = TickStyle.None };
            tbBoyut.Scroll += (s, e) => { lblBoyut.Text = $"Boyut: {tbBoyut.Value}"; UpdateWatermark(s, e); };
            currentY += 60;

            // Saydamlık
            lblSaydamlik = new Label() { Text = "Saydamlık: 128", Location = new Point(x3, currentY), AutoSize = true };
            tbSaydamlik = new TrackBar() { Location = new Point(x3, currentY + 20), Size = new Size(ctrlW, 45), Minimum = 0, Maximum = 255, Value = 128, TickStyle = TickStyle.None };
            tbSaydamlik.Scroll += (s, e) => { lblSaydamlik.Text = $"Saydamlık: {tbSaydamlik.Value}"; UpdateWatermark(s, e); };
            currentY += 60;

            // Açı
            lblAci = new Label() { Text = "Açı: -45°", Location = new Point(x3, currentY), AutoSize = true };
            tbAci = new TrackBar() { Location = new Point(x3, currentY + 20), Size = new Size(ctrlW, 45), Minimum = -180, Maximum = 180, Value = -45, TickStyle = TickStyle.None };
            tbAci.Scroll += (s, e) => { lblAci.Text = $"Açı: {tbAci.Value}°"; UpdateWatermark(s, e); };
            currentY += 60;

            // Renk Butonu
            btnRenkSec = new Button() { Text = "Yazı Rengi Seç", Location = new Point(x3, currentY), Size = new Size(ctrlW, 35), BackColor = Color.LightGray };
            btnRenkSec.Click += BtnRenkSec_Click;
            currentY += 50;

            // Kaydet
            btnKaydet = new Button() { Text = "RESMİ KAYDET", Location = new Point(x3, currentY), Size = new Size(ctrlW, 45), BackColor = Color.LightGreen, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnKaydet.Click += BtnKaydet_Click;

            // Çıkış
            btnGeri = new Button() { Text = "Çıkış", Location = new Point(this.Width - 120, this.Height - 80), Size = new Size(100, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblKaynak, pcbKaynak, btnResimYukle,
                lblSonuc, pcbSonuc,
                lblAyarlar, l1, txtMetin,
                lblKonumX, tbKonumX, lblKonumY, tbKonumY,
                lblBoyut, tbBoyut, lblSaydamlik, tbSaydamlik, lblAci, tbAci,
                btnRenkSec, btnKaydet, btnGeri
            });
        }

        private PictureBox CreateBox(int x, int y, int w, int h)
        {
            return new PictureBox() { Location = new Point(x, y), Size = new Size(w, h), BorderStyle = BorderStyle.Fixed3D, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
        }

        // RESİM YÜKLEME
        private void LoadImageSafe(ref Bitmap targetBmp, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap src = new Bitmap(ofd.FileName))
                {

                    Bitmap safe = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(safe)) g.DrawImage(src, 0, 0);

                    if (targetBmp != null) targetBmp.Dispose();
                    targetBmp = (Bitmap)safe.Clone();
                }
                pcb.Image = targetBmp;
                UpdateWatermark(null, null);
            }
        }

        private void BtnRenkSec_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                yaziRengi = cd.Color;
                btnRenkSec.BackColor = cd.Color;
                UpdateWatermark(null, null);
            }
        }

        // FİLİGRAN EKLEME
        private void UpdateWatermark(object sender, EventArgs e)
        {
            if (bmpKaynak == null) return;


            Bitmap tempBmp = (Bitmap)bmpKaynak.Clone();

            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                string text = txtMetin.Text;
                int fontSize = tbBoyut.Value;
                int opacity = tbSaydamlik.Value;
                float angle = tbAci.Value;


                float posX = (tempBmp.Width * tbKonumX.Value) / 100f;
                float posY = (tempBmp.Height * tbKonumY.Value) / 100f;

                Font font = new Font("Arial", fontSize, FontStyle.Bold);
                Color transparentColor = Color.FromArgb(opacity, yaziRengi);
                SolidBrush brush = new SolidBrush(transparentColor);

                // Yazı boyutunu ölç
                SizeF textSize = g.MeasureString(text, font);

                // 1. Önce döndürme merkezini  taşı
                g.TranslateTransform(posX, posY);

                // 2. Döndür
                g.RotateTransform(angle);

                // 3. Yazıyı Çiz
                g.DrawString(text, font, brush, -(textSize.Width / 2), -(textSize.Height / 2));

                g.ResetTransform();
            }

            pcbSonuc.Image = tempBmp;
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (pcbSonuc.Image == null) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Resim Dosyaları|*.jpg;*.png;*.bmp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pcbSonuc.Image.Save(sfd.FileName);
                MessageBox.Show("Kaydedildi.");
            }
        }
    }
}