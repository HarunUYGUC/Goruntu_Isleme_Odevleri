using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_BitDuzlemiIslemleriForm : Form
    {
        // UI Elemanları
        private PictureBox pcbKaynak, pcbBit0, pcbBit5, pcbToplam, pcbFark;
        private Label lblKaynak, lblBit0, lblBit5, lblToplam, lblFark;
        private Button btnResimYukle, btnIslemiUygula, btnGeri;
        private OpenFileDialog openFileDialog;
        private Bitmap kaynakResim;
        private Form parentForm;

        public Proje2_BitDuzlemiIslemleriForm(Form parent)
        {
            InitializeComponent();
            parentForm = parent; // parentForm atamasını InitializeComponent sonrasına aldım
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Proje 2: Bit Düzlemi Ekleme ve Çıkarma";
            this.Size = new Size(1050, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Lambda ifadesi içinde parentForm'un null olmadığından emin olmak iyidir
            this.FormClosed += (s, e) => {
                if (parentForm != null && !parentForm.IsDisposed)
                    parentForm.Show();
            };
        }

        private void SetupUI()
        {
            int picSize = 200;
            int gap = 20;
            int startX = 30;
            int startY = 50;
            int labelOffsetY = -25;

            // --- Kaynak Resim ---
            lblKaynak = CreateLabel("Kaynak Resim (Gri)", startX, startY + labelOffsetY);
            pcbKaynak = CreatePictureBox(startX, startY, picSize);

            // --- Bit Düzlemleri ---
            lblBit0 = CreateLabel("0. Bit Düzlemi (Gürültü)", startX + picSize + gap, startY + labelOffsetY);
            pcbBit0 = CreatePictureBox(startX + picSize + gap, startY, picSize);

            lblBit5 = CreateLabel("5. Bit Düzlemi (Yapı)", startX + 2 * (picSize + gap), startY + labelOffsetY);
            pcbBit5 = CreatePictureBox(startX + 2 * (picSize + gap), startY, picSize);

            // --- Sonuçlar (Alt Satır) ---
            int row2Y = startY + picSize + 60;

            lblToplam = CreateLabel("Toplam (Bit 5 + Bit 0)", startX + picSize + gap, row2Y + labelOffsetY);
            pcbToplam = CreatePictureBox(startX + picSize + gap, row2Y, picSize);

            lblFark = CreateLabel("Fark ( |Bit 5 - Bit 0| )", startX + 2 * (picSize + gap), row2Y + labelOffsetY);
            pcbFark = CreatePictureBox(startX + 2 * (picSize + gap), row2Y, picSize);

            // --- Butonlar ---
            btnResimYukle = CreateButton("Resim Yükle", 30, row2Y, 150, 40, Color.LightBlue);
            btnResimYukle.Click += BtnResimYukle_Click;

            btnIslemiUygula = CreateButton("İşlemi Uygula", 30, row2Y + 50, 150, 40, Color.LightGreen);
            btnIslemiUygula.Click += BtnIslemiUygula_Click;
            btnIslemiUygula.Enabled = false;

            btnGeri = CreateButton("Menüye Dön", 30, 550, 150, 40, Color.LightCoral);
            btnGeri.Click += (s, e) => this.Close();

            // --- HATA DÜZELTİLDİ: Filtre formatı ---
            // Eskisi: "Resim Dosyaları|.jpg;.jpeg;.png;.bmp" (Yanlış)
            // Yenisi: "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp" (Doğru)
            openFileDialog = new OpenFileDialog { Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp" };

            this.Controls.Add(lblKaynak); this.Controls.Add(pcbKaynak);
            this.Controls.Add(lblBit0); this.Controls.Add(pcbBit0);
            this.Controls.Add(lblBit5); this.Controls.Add(pcbBit5);
            this.Controls.Add(lblToplam); this.Controls.Add(pcbToplam);
            this.Controls.Add(lblFark); this.Controls.Add(pcbFark);
            this.Controls.Add(btnResimYukle);
            this.Controls.Add(btnIslemiUygula);
            this.Controls.Add(btnGeri);
        }

        // --- Olay İşleyiciler ---

        private void BtnResimYukle_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Dosya kilitleme sorununu önlemek için using bloğu veya kopyalama
                    using (var temp = new Bitmap(openFileDialog.FileName))
                    {
                        // PictureBox boyutuna göre değil, orijinal boyutta tutmak daha sağlıklıdır
                        // Ancak burada önceki kodunuzdaki mantığı (pcbKaynak.Size) koruyorum
                        kaynakResim = new Bitmap(temp, pcbKaynak.Size);
                    }

                    GriTonlamayaCevir(kaynakResim);
                    pcbKaynak.Image = kaynakResim;
                    btnIslemiUygula.Enabled = true;

                    pcbBit0.Image = null;
                    pcbBit5.Image = null;
                    pcbToplam.Image = null;
                    pcbFark.Image = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim yüklenirken hata oluştu: " + ex.Message);
                }
            }
        }

        private void BtnIslemiUygula_Click(object sender, EventArgs e)
        {
            if (kaynakResim == null) return;
            this.Cursor = Cursors.WaitCursor; // İmleci bekleme moduna al

            int w = kaynakResim.Width;
            int h = kaynakResim.Height;

            Bitmap bmpBit0 = new Bitmap(w, h);
            Bitmap bmpBit5 = new Bitmap(w, h);
            Bitmap bmpToplam = new Bitmap(w, h);
            Bitmap bmpFark = new Bitmap(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int griDeger = kaynakResim.GetPixel(x, y).R;

                    // Bit değerleri (0 veya 1)
                    int bit0Value = (griDeger >> 0) & 1;
                    int bit5Value = (griDeger >> 5) & 1;

                    // Görsel değerler (0 veya 255)
                    int pikselBit0 = bit0Value * 255;
                    int pikselBit5 = bit5Value * 255;

                    bmpBit0.SetPixel(x, y, Color.FromArgb(pikselBit0, pikselBit0, pikselBit0));
                    bmpBit5.SetPixel(x, y, Color.FromArgb(pikselBit5, pikselBit5, pikselBit5));

                    // Toplam ve Fark (Görsel değerler üzerinden)
                    int toplam = Math.Min(255, pikselBit5 + pikselBit0);
                    int fark = Math.Abs(pikselBit5 - pikselBit0);

                    bmpToplam.SetPixel(x, y, Color.FromArgb(toplam, toplam, toplam));
                    bmpFark.SetPixel(x, y, Color.FromArgb(fark, fark, fark));
                }
            }

            pcbBit0.Image = bmpBit0;
            pcbBit5.Image = bmpBit5;
            pcbToplam.Image = bmpToplam;
            pcbFark.Image = bmpFark;

            this.Cursor = Cursors.Default; // İmleci normale döndür
        }

        private void GriTonlamayaCevir(Bitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    int gri = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11); // Luma yöntemi daha kalitelidir
                    bmp.SetPixel(x, y, Color.FromArgb(gri, gri, gri));
                }
            }
        }

        // --- UI Yardımcıları ---
        private PictureBox CreatePictureBox(int x, int y, int size)
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
        private Label CreateLabel(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
        }
        private Button CreateButton(string text, int x, int y, int w, int h, Color color)
        {
            return new Button { Text = text, Location = new Point(x, y), Size = new Size(w, h), BackColor = color, Font = new Font("Arial", 10) };
        }
    }
}