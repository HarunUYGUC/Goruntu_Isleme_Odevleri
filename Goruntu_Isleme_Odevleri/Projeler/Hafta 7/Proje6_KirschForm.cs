using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje6_KirschForm : Form
    {
        private PictureBox pcbOriginal, pcbResult;
        private Label lblOriginal, lblResult;
        private Button btnYukle, btnUygula, btnGeri;
        private TrackBar tbThreshold; // Eşik Değeri
        private Label lblThreshold;
        private Form haftaFormu;
        private Bitmap originalBitmap;

        // 8 Farklı Yön Matrisini bu listede tutacağız.
        private List<int[,]> kirschMaskeleri = new List<int[,]>();

        public Proje6_KirschForm(Form parentForm)
        {
            InitializeComponent();
            MaskeleriOlustur(); // Program açılırken matrisleri yükle
            haftaFormu = parentForm;
            this.Text = "Proje 6: Kirsch Compass Algoritması (Liste Yapısı)";

            int pcbSize = 350;
            int margin = 20;

            // Sol Taraf (Orijinal)
            lblOriginal = new Label() { Text = "Orijinal Resim", Location = new Point(margin, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbOriginal = new PictureBox() { Location = new Point(margin, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            // Sağ Taraf (Sonuç)
            lblResult = new Label() { Text = "Kirsch Kenar Sonucu (MaxD)", Location = new Point(margin + pcbSize + 20, margin), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };
            pcbResult = new PictureBox() { Location = new Point(margin + pcbSize + 20, margin + 25), Size = new Size(pcbSize, pcbSize), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };

            // Kontroller (Alt Kısım)
            int ctrlX = margin;
            int ctrlY = margin + pcbSize + 40;

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(ctrlX, ctrlY), Size = new Size(150, 40) };
            btnYukle.Click += new EventHandler(btnYukle_Click);

            lblThreshold = new Label() { Text = "Eşik (Hassasiyet): 100", Location = new Point(ctrlX + 170, ctrlY - 15), AutoSize = true };
            tbThreshold = new TrackBar() { Location = new Point(ctrlX + 170, ctrlY + 5), Size = new Size(200, 45), Minimum = 0, Maximum = 1000, Value = 100, TickFrequency = 50 };
            tbThreshold.Scroll += (s, e) => { lblThreshold.Text = $"Eşik (Hassasiyet): {tbThreshold.Value}"; };

            btnUygula = new Button() { Text = "Kirsch Uygula", Location = new Point(ctrlX + 390, ctrlY), Size = new Size(150, 40), BackColor = Color.LightGreen, Enabled = false };
            btnUygula.Click += new EventHandler(btnUygula_Click);

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(ctrlX + 560, ctrlY), Size = new Size(150, 40), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            // Form Boyutu
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.Controls.AddRange(new Control[] {
                lblOriginal, pcbOriginal, lblResult, pcbResult,
                btnYukle, lblThreshold, tbThreshold, btnUygula, btnGeri
            });

            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void InitializeComponent() { this.Name = "Proje6_KirschForm"; }

        // --- 1. LİSTE YAPISINI OLUŞTURMA ---
        private void MaskeleriOlustur()
        {
            // Kuzey (N)
            kirschMaskeleri.Add(new int[,] { { 5, 5, 5 }, { -3, 0, -3 }, { -3, -3, -3 } });
            // Kuzey Batı (NW)
            kirschMaskeleri.Add(new int[,] { { 5, 5, -3 }, { 5, 0, -3 }, { -3, -3, -3 } });
            // Batı (W)
            kirschMaskeleri.Add(new int[,] { { 5, -3, -3 }, { 5, 0, -3 }, { 5, -3, -3 } });
            // Güney Batı (SW)
            kirschMaskeleri.Add(new int[,] { { -3, -3, -3 }, { 5, 0, -3 }, { 5, 5, -3 } });
            // Güney (S)
            kirschMaskeleri.Add(new int[,] { { -3, -3, -3 }, { -3, 0, -3 }, { 5, 5, 5 } });
            // Güney Doğu (SE)
            kirschMaskeleri.Add(new int[,] { { -3, -3, -3 }, { -3, 0, 5 }, { -3, 5, 5 } });
            // Doğu (E)
            kirschMaskeleri.Add(new int[,] { { -3, -3, 5 }, { -3, 0, 5 }, { -3, -3, 5 } });
            // Kuzey Doğu (NE)
            kirschMaskeleri.Add(new int[,] { { -3, 5, 5 }, { -3, 0, 5 }, { -3, -3, -3 } });
        }

        private void btnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                originalBitmap = new Bitmap(dialog.FileName);
                pcbOriginal.Image = originalBitmap;
                btnUygula.Enabled = true;
            }
        }

        private void btnUygula_Click(object sender, EventArgs e)
        {
            if (originalBitmap == null) return;
            this.Cursor = Cursors.WaitCursor;

            // İşlemi Başlat
            pcbResult.Image = KirschAlgoritmasiUygula(originalBitmap, tbThreshold.Value);

            this.Cursor = Cursors.Default;
        }

        // --- 2. ANA ALGORİTMA (Türkçe Değişken İsimleriyle) ---
        private Bitmap KirschAlgoritmasiUygula(Bitmap girisResmi, int esikDegeri)
        {
            int w = girisResmi.Width;
            int h = girisResmi.Height;
            Bitmap cikisResmi = new Bitmap(w, h);

            // Resim üzerinde gezinme
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    // KOMŞULARI ALMA (İstenen Değişken Düzenlemesi)
                    // cl, cr, cu yerine sol, sag, ust kullanımı:
                    Color cSol = girisResmi.GetPixel(x - 1, y);     // Left
                    Color cSag = girisResmi.GetPixel(x + 1, y);     // Right
                    Color cUst = girisResmi.GetPixel(x, y - 1);     // Up
                    Color cAlt = girisResmi.GetPixel(x, y + 1);     // Down
                    Color cSolUst = girisResmi.GetPixel(x - 1, y - 1); // Up-Left
                    Color cSagUst = girisResmi.GetPixel(x + 1, y - 1); // Up-Right
                    Color cSolAlt = girisResmi.GetPixel(x - 1, y + 1); // Down-Left
                    Color cSagAlt = girisResmi.GetPixel(x + 1, y + 1); // Down-Right

                    // Listeyi kullanarak en büyük değeri bulma
                    int enBuyukKenarDegeri = GetMaxKirschResponse(
                        cSol.R, cSag.R, cUst.R, cAlt.R,
                        cSolUst.R, cSagUst.R, cSolAlt.R, cSagAlt.R
                    );

                    // Eşik (Threshold) Kontrolü
                    if (enBuyukKenarDegeri > esikDegeri)
                    {
                        cikisResmi.SetPixel(x, y, Color.Yellow);
                    }
                    else
                    {
                        cikisResmi.SetPixel(x, y, Color.Black);
                    }
                }
            }
            return cikisResmi;
        }

        // --- 3. LİSTE ÜZERİNDE DÖNGÜ (List Yapısının Kullanımı) ---
        private int GetMaxKirschResponse(int sol, int sag, int ust, int alt, int solUst, int sagUst, int solAlt, int sagAlt)
        {
            int maxDeger = int.MinValue;

            foreach (int[,] matris in kirschMaskeleri)
            {
                // Konvolüsyon İşlemi: (Matris Değeri * Piksel Değeri)
                // Matris[Satır, Sütun]
                long toplam =
                      (matris[0, 0] * solUst) + (matris[0, 1] * ust) + (matris[0, 2] * sagUst)
                    + (matris[1, 0] * sol) + (matris[1, 2] * sag)
                    + (matris[2, 0] * solAlt) + (matris[2, 1] * alt) + (matris[2, 2] * sagAlt);

                int sonuc = Math.Abs((int)toplam);

                if (sonuc > maxDeger)
                {
                    maxDeger = sonuc;
                }
            }

            return maxDeger;
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