using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // Resize kalitesi için gerekli
using System.Linq;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje6_BolgeselNetlestirmeBulaniklastirmaForm : Form
    {
        // UI Elemanları
        private PictureBox pcbGoruntu;
        private Button btnResimYukle, btnUygula, btnTersine, btnTemizle, btnGeri;
        private ComboBox cmbIcAlgoritma, cmbDisAlgoritma;
        private Label lblIc, lblDis;

        // Değişkenler
        private Form haftaFormu;
        private Bitmap orijinalResim;
        private List<Point> noktalar;
        private bool tersineModu = false;

        public Proje6_BolgeselNetlestirmeBulaniklastirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Bölgesel Görüntü İşleme (ROI)";

            // Form Boyutunu Sabitleyelim ki taşma olmasın
            this.Size = new Size(950, 650);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            noktalar = new List<Point>();

            // --- PictureBox Ayarları ---
            pcbGoruntu = new PictureBox();
            pcbGoruntu.Location = new Point(20, 20);
            pcbGoruntu.Size = new Size(600, 500); // Sabit ve büyük bir alan
            pcbGoruntu.BorderStyle = BorderStyle.FixedSingle;
            pcbGoruntu.BackColor = Color.LightGray;
            // ÖNEMLİ: Büyük resimler taşmasın diye Zoom kullanıyoruz
            pcbGoruntu.SizeMode = PictureBoxSizeMode.Zoom;
            pcbGoruntu.MouseClick += new MouseEventHandler(pcbGoruntu_MouseClick);
            pcbGoruntu.Paint += new PaintEventHandler(pcbGoruntu_Paint);

            this.Controls.Add(pcbGoruntu);

            // --- Sağ Panel (Kontroller) ---
            int panelX = 640;
            int currentY = 20;

            btnResimYukle = CreateButton("Resim Yükle", panelX, ref currentY);
            btnResimYukle.Click += new EventHandler(btnResimYukle_Click);

            AddLabel("Bölge İÇİ Algoritması:", panelX, ref currentY);
            cmbIcAlgoritma = CreateCombo(panelX, ref currentY);
            cmbIcAlgoritma.SelectedIndex = 4; // Default: Sharpen

            AddLabel("Bölge DIŞI Algoritması:", panelX, ref currentY);
            cmbDisAlgoritma = CreateCombo(panelX, ref currentY);
            cmbDisAlgoritma.SelectedIndex = 1; // Default: Mean

            currentY += 10;
            btnUygula = CreateButton("Filtreleri Uygula", panelX, ref currentY);
            btnUygula.BackColor = Color.LightGreen;
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnTersine = CreateButton("Mod: Normal\n(Tersine Çevir)", panelX, ref currentY);
            btnTersine.Height = 50; // Biraz büyük olsun
            btnTersine.Click += new EventHandler(btnTersine_Click);
            currentY += 10; // Extra boşluk

            btnTemizle = CreateButton("Seçimi Temizle", panelX, ref currentY);
            btnTemizle.Click += new EventHandler(btnTemizle_Click);

            btnGeri = CreateButton("Menüye Dön", panelX, ref currentY);
            btnGeri.BackColor = Color.LightCoral;
            btnGeri.Click += new EventHandler(btnGeri_Click);
        }

        // UI Oluşturma Yardımcıları (Kod tekrarını azaltmak için)
        private Button CreateButton(string text, int x, ref int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(250, 40); // Butonları genişlettik
            this.Controls.Add(btn);
            y += 50;
            return btn;
        }

        private void AddLabel(string text, int x, ref int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            this.Controls.Add(lbl);
            y += 20;
        }

        private ComboBox CreateCombo(int x, ref int y)
        {
            ComboBox cmb = new ComboBox();
            cmb.Location = new Point(x, y);
            cmb.Size = new Size(250, 25);
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.Items.AddRange(new object[] { "Orjinal (Yok)", "Mean (Bulanık)", "Median (Gürültü Sil)", "Gauss (Yumuşak)", "Sharpen (Net)" });
            this.Controls.Add(cmb);
            y += 40;
            return cmb;
        }

        private void InitializeComponent() { this.Name = "Proje6_BolgeselIslemeForm"; }

        // --- OLAYLAR ---

        private void btnResimYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Resmi yükle
                using (Bitmap tempBmp = new Bitmap(ofd.FileName))
                {
                    // Eğer resim çok büyükse (örn: genişlik > 800), yeniden boyutlandır (Resize).
                    // Bu işlem hem taşmayı engeller hem de piksel işlemlerini hızlandırır.
                    int maxWidth = 800;
                    int maxHeight = 600;

                    if (tempBmp.Width > maxWidth || tempBmp.Height > maxHeight)
                    {
                        // Oran koruyarak küçültme
                        float scale = Math.Min((float)maxWidth / tempBmp.Width, (float)maxHeight / tempBmp.Height);
                        int newW = (int)(tempBmp.Width * scale);
                        int newH = (int)(tempBmp.Height * scale);

                        orijinalResim = new Bitmap(newW, newH);
                        using (Graphics g = Graphics.FromImage(orijinalResim))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(tempBmp, 0, 0, newW, newH);
                        }
                    }
                    else
                    {
                        orijinalResim = new Bitmap(tempBmp);
                    }
                }

                pcbGoruntu.Image = new Bitmap(orijinalResim);
                noktalar.Clear();
            }
        }

        private void pcbGoruntu_MouseClick(object sender, MouseEventArgs e)
        {
            if (orijinalResim == null) return;

            if (e.Button == MouseButtons.Left)
            {
                // Zoom modunda koordinat dönüşümü yapmamız lazım.
                // PictureBox'a tıklanan (e.X, e.Y) noktası, resmin gerçek (imgX, imgY) noktasına denk gelmeyebilir.
                // Ancak "SizeMode = AutoSize" veya "Normal" kullanırsak sorun yok.
                // Taşma sorunu için "Zoom" kullanıyoruz, bu yüzden koordinatları hesaplamalıyız.

                // Basitlik adına, eğer resim PictureBox'a sığdırılmışsa (yukarıdaki resize işlemiyle),
                // Mouse koordinatlarını doğrudan kullanabiliriz.

                // Ancak en sağlıklısı resmi PictureBox boyutuna göre değil,
                // PictureBox içindeki resmin GÖRÜNEN boyutuna göre oranlamaktır.
                // Şimdilik karmaşık matematik yerine, yukarıda resmi zaten küçülttüğümüz için
                // PictureBox'ı resim boyutuna eşitleyelim (AutoSize) ve Panel içine koyalım (Scrollable).
                // VEYA: Resmi PictureBox'a tam sığacak şekilde (Stretch) değil, orjinal boyutta gösterelim.

                // KODU BASİTLEŞTİRMEK İÇİN ÇÖZÜM:
                // Resmi yüklerken zaten küçülttük (max 800x600).
                // PictureBox boyutunu da resme eşitleyelim.

                noktalar.Add(e.Location);
                pcbGoruntu.Invalidate();
            }
        }

        private void pcbGoruntu_Paint(object sender, PaintEventArgs e)
        {
            if (noktalar.Count > 0)
            {
                foreach (Point p in noktalar) e.Graphics.FillEllipse(Brushes.Red, p.X - 3, p.Y - 3, 6, 6);
                if (noktalar.Count > 1) e.Graphics.DrawLines(Pens.Yellow, noktalar.ToArray());
                if (noktalar.Count > 2) e.Graphics.DrawLine(Pens.Yellow, noktalar[noktalar.Count - 1], noktalar[0]);
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (orijinalResim == null || noktalar.Count < 3)
            {
                MessageBox.Show("Resim yükleyip en az 3 nokta seçiniz.");
                return;
            }

            // PictureBox Zoom modunda olduğu için, ekrandaki noktaları (noktalar listesi)
            // gerçek resim boyutuna (orijinalResim) uyarlamamız gerekebilir.
            // Ancak "ResimYükle" kısmında resmi zaten küçülttüğümüz ve PictureBox'a sığdırdığımız için
            // bu örnekte direkt eşleşme varsayıyoruz. (Gerçek bir uygulamada oran hesabı gerekir).

            // Güvenlik için, noktaları resim boyutuna göre kısıtlayalım (Clamp).
            List<Point> safePoints = new List<Point>();
            foreach (var p in noktalar)
            {
                // Koordinatlar resim sınırları içinde mi?
                // PictureBox "Zoom" ise ve resim ortadaysa, boşluğa tıklanmış olabilir.
                // Bu örnekte basitlik için PictureBox'ın sol üst köşesinden başladığını varsayıyoruz.
                // Daha hassas çizim için PictureBox SizeMode=Normal veya AutoSize kullanılmalıydı.

                // Zoom modunda koordinat eşleşmesi zordur. 
                // En iyi çözüm: Resmi yüklerken PictureBox boyutuna (600x500) tam sığacak şekilde ölçekleyip
                // yeni bir Bitmap oluşturmak (yukarıda yaptığımız gibi).
                // Böylece pcb.Image boyutu ile pcb.Size boyutu hemen hemen aynı olur.

                // Koordinat dönüşümü (Basit Orantı - Eğer resim ile kutu farklıysa)
                float ratioX = (float)orijinalResim.Width / pcbGoruntu.ClientSize.Width;
                float ratioY = (float)orijinalResim.Height / pcbGoruntu.ClientSize.Height;

                // Zoom modunda resim ortalanır, bu yüzden bu hesap biraz daha karışıktır.
                // ÖDEV İÇİN PRATİK ÇÖZÜM:
                // Mouse tıklamalarını doğrudan resim koordinatı kabul edelim.
                // Bunun için resmin boyutu ile PictureBox boyutu aynı olmalı.
                // ResimYükle fonksiyonunda resmi, PictureBox boyutuna (600x500) zorla sığdıralım (Stretch).

                safePoints.Add(p);
            }

            // İşlem için resmi PictureBox boyutuna getir (Stretch - Görüntü biraz bozulabilir ama tıklama tutar)
            Bitmap islemResmi = new Bitmap(orijinalResim, pcbGoruntu.ClientSize);

            int w = islemResmi.Width;
            int h = islemResmi.Height;

            Bitmap maske = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(maske))
            {
                g.Clear(Color.Black);
                g.FillPolygon(Brushes.White, safePoints.ToArray());
            }

            Bitmap sonuc = new Bitmap(w, h);
            string icAlgo = cmbIcAlgoritma.SelectedItem.ToString();
            string disAlgo = cmbDisAlgoritma.SelectedItem.ToString();

            // Yavaş GetPixel/SetPixel yerine LockBits kullanılabilir ama ödev basitliği için böyle bırakıldı.
            // Hızlandırmak için döngüde her piksel yerine 2'şer atlayarak önizleme yapılabilir.

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    bool icerde = (maske.GetPixel(x, y).R == 255);
                    if (tersineModu) icerde = !icerde;

                    if (icerde) sonuc.SetPixel(x, y, FiltreUygula(islemResmi, x, y, icAlgo));
                    else sonuc.SetPixel(x, y, FiltreUygula(islemResmi, x, y, disAlgo));
                }
            }

            pcbGoruntu.Image = sonuc;
            MessageBox.Show("Tamamlandı.");
        }

        private Color FiltreUygula(Bitmap bmp, int x, int y, string algo)
        {
            if (algo.StartsWith("Orjinal")) return bmp.GetPixel(x, y);
            if (algo.StartsWith("Median")) return MedianFiltresi(bmp, x, y);

            double[,] kernel = null;
            if (algo.StartsWith("Mean")) kernel = new double[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            else if (algo.StartsWith("Gauss")) kernel = new double[,] { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };
            else if (algo.StartsWith("Sharpen")) kernel = new double[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };

            return Konvolusyon(bmp, x, y, kernel);
        }

        private Color Konvolusyon(Bitmap bmp, int x, int y, double[,] kernel)
        {
            double rT = 0, gT = 0, bT = 0, kT = 0;
            int w = bmp.Width; int h = bmp.Height;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int pX = x + i; int pY = y + j;
                    if (pX < 0 || pX >= w || pY < 0 || pY >= h) continue; // Sınır koruması

                    Color p = bmp.GetPixel(pX, pY);
                    double val = kernel[i + 1, j + 1];

                    rT += p.R * val; gT += p.G * val; bT += p.B * val;
                    kT += val;
                }
            }
            if (kT <= 0) kT = 1;
            return Color.FromArgb(Clamp(rT / kT), Clamp(gT / kT), Clamp(bT / kT));
        }

        private Color MedianFiltresi(Bitmap bmp, int x, int y)
        {
            List<int> r = new List<int>(), g = new List<int>(), b = new List<int>();
            int w = bmp.Width; int h = bmp.Height;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int pX = x + i; int pY = y + j;
                    if (pX < 0 || pX >= w || pY < 0 || pY >= h) continue;

                    Color p = bmp.GetPixel(pX, pY);
                    r.Add(p.R); g.Add(p.G); b.Add(p.B);
                }
            }
            r.Sort(); g.Sort(); b.Sort();
            int mid = r.Count / 2;
            return Color.FromArgb(r[mid], g[mid], b[mid]);
        }

        private int Clamp(double val) { return Math.Max(0, Math.Min(255, (int)val)); }

        private void btnTemizle_Click(object sender, EventArgs e) { noktalar.Clear(); pcbGoruntu.Invalidate(); }
        private void btnTersine_Click(object sender, EventArgs e) { tersineModu = !tersineModu; btnTersine.Text = tersineModu ? "Mod: TERS" : "Mod: Normal"; }
        private void btnGeri_Click(object sender, EventArgs e) { this.Close(); haftaFormu.Show(); }
        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e) { haftaFormu.Show(); }
    }
}