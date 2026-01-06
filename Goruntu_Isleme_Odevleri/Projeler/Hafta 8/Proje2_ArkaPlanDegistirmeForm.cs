using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_ArkaPlanDegistirmeForm : Form
    {
        private PictureBox pcbOnPlan, pcbArkaPlan, pcbSonuc, pcbSecilenRenk;
        private Label lblOn, lblArka, lblSonuc, lblBilgi, lblTolerans;
        private Button btnYukleOn, btnYukleArka, btnBirlestir, btnGeri;
        private TrackBar tbTolerans;

        private Bitmap bmpOnPlan, bmpArkaPlan;
        private Color hedefRenk = Color.Empty;
        private Form haftaFormu;

        public Proje2_ArkaPlanDegistirmeForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Arka Plan Değiştirme (Chroma Key)";
            SetupUI();

            this.FormClosed += (s, e) => haftaFormu.Show();
        }

        private void SetupUI()
        {
            this.Size = new Size(900, 450);
            this.StartPosition = FormStartPosition.CenterScreen;

            int margin = 15;
            int pcbW = 260;
            int pcbH = 200;

            lblOn = new Label() { Text = "1. Ön Plan (Kişi)", Location = new Point(margin, margin), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            pcbOnPlan = new PictureBox() { Location = new Point(margin, margin + 20), Size = new Size(pcbW, pcbH), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, Cursor = Cursors.Hand };
            pcbOnPlan.MouseClick += PcbOnPlan_MouseClick;

            btnYukleOn = new Button() { Text = "Kişi Yükle", Location = new Point(margin, margin + 25 + pcbH), Size = new Size(pcbW, 30) };
            btnYukleOn.Click += (s, e) => ResimYukle(ref bmpOnPlan, pcbOnPlan);

            int col2X = margin + pcbW + 15;
            lblArka = new Label() { Text = "2. Arka Plan (Manzara)", Location = new Point(col2X, margin), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            pcbArkaPlan = new PictureBox() { Location = new Point(col2X, margin + 20), Size = new Size(pcbW, pcbH), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            btnYukleArka = new Button() { Text = "Manzara Yükle", Location = new Point(col2X, margin + 25 + pcbH), Size = new Size(pcbW, 30) };
            btnYukleArka.Click += (s, e) => ResimYukle(ref bmpArkaPlan, pcbArkaPlan);

            int col3X = col2X + pcbW + 15;
            lblSonuc = new Label() { Text = "3. Sonuç", Location = new Point(col3X, margin), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            pcbSonuc = new PictureBox() { Location = new Point(col3X, margin + 20), Size = new Size(pcbW, pcbH), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            int ayarY = margin + 30 + pcbH + 40; 

            lblBilgi = new Label() { Text = "<- Önce soldaki resimde duvar rengine tıklayın.", Location = new Point(margin, ayarY + 8), AutoSize = true, ForeColor = Color.Red };
            pcbSecilenRenk = new PictureBox() { Location = new Point(margin + 260, ayarY), Size = new Size(40, 30), BorderStyle = BorderStyle.Fixed3D, BackColor = Color.White };

            lblTolerans = new Label() { Text = "Tolerans: 40", Location = new Point(col2X, ayarY - 15), AutoSize = true };
            tbTolerans = new TrackBar() { Location = new Point(col2X, ayarY), Size = new Size(260, 45), Maximum = 150, Value = 40, TickFrequency = 10 };
            tbTolerans.Scroll += (s, e) => lblTolerans.Text = $"Tolerans: {tbTolerans.Value}";

            btnBirlestir = new Button() { Text = "BİRLEŞTİR", Location = new Point(col3X, ayarY), Size = new Size(150, 40), BackColor = Color.LightGreen, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnBirlestir.Click += BtnBirlestir_Click;

            btnGeri = new Button() { Text = "Geri", Location = new Point(col3X + 160, ayarY), Size = new Size(100, 40), BackColor = Color.LightCoral };
            btnGeri.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblOn, pcbOnPlan, btnYukleOn,
                lblArka, pcbArkaPlan, btnYukleArka,
                lblSonuc, pcbSonuc,
                lblBilgi, pcbSecilenRenk,
                lblTolerans, tbTolerans,
                btnBirlestir, btnGeri
            });
        }

        private void InitializeComponent() { this.Name = "Proje2_ArkaPlanDegistirmeForm"; }

        private void ResimYukle(ref Bitmap hedefBitmap, PictureBox pcb)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                hedefBitmap = new Bitmap(ofd.FileName);
                pcb.Image = hedefBitmap;
            }
        }

        private void PcbOnPlan_MouseClick(object sender, MouseEventArgs e)
        {
            if (bmpOnPlan == null) return;

            float xRatio = (float)bmpOnPlan.Width / pcbOnPlan.ClientSize.Width;
            float yRatio = (float)bmpOnPlan.Height / pcbOnPlan.ClientSize.Height;

            int realX = (int)(e.X * xRatio);
            int realY = (int)(e.Y * yRatio);

            if (realX >= 0 && realX < bmpOnPlan.Width && realY >= 0 && realY < bmpOnPlan.Height)
            {
                hedefRenk = bmpOnPlan.GetPixel(realX, realY);
                pcbSecilenRenk.BackColor = hedefRenk;
                lblBilgi.Text = "Renk seçildi! Şimdi 'Birleştir'e bas.";
                lblBilgi.ForeColor = Color.Green;
            }
        }

        private void BtnBirlestir_Click(object sender, EventArgs e)
        {
            if (bmpOnPlan == null || bmpArkaPlan == null) { MessageBox.Show("Resimleri yükleyin."); return; }
            if (hedefRenk == Color.Empty) { MessageBox.Show("Silinecek rengi seçin."); return; }

            this.Cursor = Cursors.WaitCursor;

            int w = bmpOnPlan.Width;
            int h = bmpOnPlan.Height;
            Bitmap sonuc = new Bitmap(w, h);
            Bitmap arkaPlanResized = new Bitmap(bmpArkaPlan, new Size(w, h));
            int tolerans = tbTolerans.Value;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color pRenk = bmpOnPlan.GetPixel(x, y);
                    int fark = Math.Abs(pRenk.R - hedefRenk.R) + Math.Abs(pRenk.G - hedefRenk.G) + Math.Abs(pRenk.B - hedefRenk.B);

                    if (fark < tolerans * 3)
                        sonuc.SetPixel(x, y, arkaPlanResized.GetPixel(x, y));
                    else
                        sonuc.SetPixel(x, y, pRenk);
                }
            }

            pcbSonuc.Image = sonuc;
            this.Cursor = Cursors.Default;
        }
    }
}