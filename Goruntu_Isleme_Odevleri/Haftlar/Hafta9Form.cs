using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Hafta9Form : Form
    {
        private Panel projePaneli;
        private Button btnGeri;
        private Label lblBaslik;
        private Form anaMenuFormu;

        public Hafta9Form(Form anaMenu)
        {
            InitializeComponent();
            anaMenuFormu = anaMenu;
            this.Text = "Hafta 9 Projeleri";

            lblBaslik = new Label();
            lblBaslik.Text = "Hafta 9 Projeleri";
            lblBaslik.Font = new Font("Arial", 14, FontStyle.Bold);
            lblBaslik.AutoSize = true;
            lblBaslik.Location = new Point(130, 20);
            this.Controls.Add(lblBaslik);

            projePaneli = new Panel();
            projePaneli.Location = new Point(25, 60);
            projePaneli.Size = new Size(380, 320);
            projePaneli.AutoScroll = true;
            projePaneli.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(projePaneli);

            int projeSayisi = 12;
            for (int i = 1; i <= projeSayisi; i++)
            {
                Button projeButon = new Button();

                if (i == 1)
                {
                    projeButon.Text = "Proje 1 (Henüz Eklenmedi)";
                }
                else if (i == 3)
                {
                    projeButon.Text = "Proje 3: İki Resmi Ağırlıklı Toplama (Blend)";
                }
                else if (i == 4)
                {
                    projeButon.Text = "Proje 4: Değişken Işıkta Nesne Ayırma (Arka Plan Çıkarma)";
                }
                else if (i == 5)
                {
                    projeButon.Text = "Proje 5: Hareket Sensörü ve Hedef Takibi";
                }
                else if (i == 7)
                {
                    projeButon.Text = "Proje 7: Otomatik Hedef Takibi ve Eksen Kontrolü";
                }
                else if (i == 8)
                {
                    projeButon.Text = "Proje 8: Gelişmiş Filigran";
                }
                else if (i == 9)
                {
                    projeButon.Text = "Proje 9: Gölge Giderme ve Metin Netleştirme";
                }
                else
                {
                    projeButon.Text = "Proje " + i + " (Henüz Eklenmedi)";
                }

                projeButon.Tag = i;
                projeButon.Size = new Size(320, 45);
                projeButon.Font = new Font("Arial", 10);
                projeButon.Location = new Point(15, 15 + ((i - 1) * 55));
                projeButon.Click += new EventHandler(ProjeButonu_Click);
                projePaneli.Controls.Add(projeButon);
            }

            btnGeri = new Button();
            btnGeri.Text = "Ana Menüye Dön";
            btnGeri.Size = new Size(200, 40);
            btnGeri.Location = new Point(115, 400);
            btnGeri.BackColor = Color.LightCoral;
            btnGeri.Font = new Font("Arial", 10);
            btnGeri.Click += new EventHandler(btnGeri_Click);
            this.Controls.Add(btnGeri);
        }

        private void InitializeComponent()
        {
            this.Name = "Hafta9Form";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(HaftaForm_FormClosed);
        }

        private void ProjeButonu_Click(object sender, EventArgs e)
        {
            Button tiklananButon = (Button)sender;
            int projeNumarasi = (int)tiklananButon.Tag;

            switch (projeNumarasi)
            {
                case 3:
                    Proje3_ResimBirlestirmeForm proje3Form = new Proje3_ResimBirlestirmeForm(this);
                    this.Hide();
                    proje3Form.Show();
                    break;
                case 4:
                    Proje4_NesneAyirmaForm proje4Form = new Proje4_NesneAyirmaForm(this);
                    this.Hide();
                    proje4Form.Show();
                    break;
                case 5:
                    Proje5_HareketTakibiForm proje5Form = new Proje5_HareketTakibiForm(this);
                    this.Hide();
                    proje5Form.Show();
                    break;
                case 7:
                    Proje7_EksenTakibiForm proje7Form = new Proje7_EksenTakibiForm(this);
                    this.Hide();
                    proje7Form.Show();
                    break;
                case 8:
                    Proje8_FiligranForm proje8Form = new Proje8_FiligranForm(this);
                    this.Hide();
                    proje8Form.Show();
                    break;
                case 9:
                    Proje9_MetinNetlestirmeForm proje9Form = new Proje9_MetinNetlestirmeForm(this);
                    this.Hide();
                    proje9Form.Show();
                    break;
                default:
                    MessageBox.Show($"Hafta 9 - Proje {projeNumarasi} henüz eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void HaftaForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            anaMenuFormu.Show();
        }
    }
}