using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Hafta3Form : Form
    {
        private Panel projePaneli;
        private Button btnGeri;
        private Label lblBaslik;
        private Form anaMenuFormu;

        public Hafta3Form(Form anaMenu)
        {
            InitializeComponent();
            anaMenuFormu = anaMenu;
            this.Text = "Hafta 3 Projeleri";

            lblBaslik = new Label();
            lblBaslik.Text = "Hafta 3 Projeleri";
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

            int projeSayisi = 13;
            for (int i = 1; i <= projeSayisi; i++)
            {
                Button projeButon = new Button();

                if (i == 1)
                {
                    projeButon.Text = "Proje 1: Resmi Taşıma (Tıklama ve Sürükleme)";
                }
                else if (i == 2)
                {
                    projeButon.Text = "Proje 2: Kaydırma ve Döngüsel Taşıma";
                }
                else if (i == 3)
                {
                    projeButon.Text = "Proje 3: Eksen Etrafında Aynalama";
                }
                else if (i == 4)
                {
                    projeButon.Text = "Proje 4: Resim Büyütme (Zooming)";
                }
                else if (i == 5)
                {
                    projeButon.Text = "Proje 5: Resim Kırpma Aracı";
                }
                else if (i == 6)
                {
                    projeButon.Text = "Proje 6: Resim Ölçekleme (Scaling)";
                }
                else if (i == 7)
                {
                    projeButon.Text = "Proje 7: Resmi Döndürme (Rotation)";
                }
                else if (i == 8)
                {
                    projeButon.Text = "Proje 8: Çapa Noktaları ile Ölçekleme";
                }
                else if (i == 9)
                {
                    projeButon.Text = "Proje 9: Yeniden Örnekleme Karşılaştırması";
                }
                else if (i == 10)
                {
                    projeButon.Text = "Proje 10: Mouse ile Etkileşimli Döndürme";
                }
                else if (i == 11)
                {
                    projeButon.Text = "Proje 11: Kaydırma (Shear) ile Döndürme";
                }
                else
                {
                    projeButon.Text = "Proje " + i;
                    projeButon.Enabled = false;
                }

                projeButon.Tag = i;
                projeButon.Size = new Size(320, 45);
                projeButon.Font = new Font("Arial", 12);
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
            this.Name = "Hafta3Form";
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
                case 1:
                    Proje1_ResimTasimaForm proje1Form = new Proje1_ResimTasimaForm(this);
                    this.Hide();
                    proje1Form.Show();
                    break;
                case 2:
                    Proje2_KaydirmaForm proje2Form = new Proje2_KaydirmaForm(this);
                    this.Hide();
                    proje2Form.Show();
                    break;
                case 3:
                    Proje3_AynalamaForm proje3Form = new Proje3_AynalamaForm(this);
                    this.Hide();
                    proje3Form.Show();
                    break;
                case 4:
                    Proje4_ResimBuyutmeForm proje4Form = new Proje4_ResimBuyutmeForm(this);
                    this.Hide();
                    proje4Form.Show();
                    break;
                case 5:
                    Proje5_KirpmaForm proje5Form = new Proje5_KirpmaForm(this);
                    this.Hide();
                    proje5Form.Show();
                    break;
                case 6:
                    Proje6_OlceklemeForm proje6Form = new Proje6_OlceklemeForm(this);
                    this.Hide();
                    proje6Form.Show();
                    break;
                case 7:
                    Proje7_DondurmeForm proje7Form = new Proje7_DondurmeForm(this);
                    this.Hide();
                    proje7Form.Show();
                    break;
                case 8:
                    Proje8_OlceklemeKutucukForm proje8Form = new Proje8_OlceklemeKutucukForm(this);
                    this.Hide();
                    proje8Form.Show();
                    break;
                case 9:
                    Proje9_InterpolasyonForm proje9Form = new Proje9_InterpolasyonForm(this);
                    this.Hide();
                    proje9Form.Show();
                    break;
                case 10:
                    Proje10_MouseIleDondurmeForm proje10Form = new Proje10_MouseIleDondurmeForm(this);
                    this.Hide();
                    proje10Form.Show();
                    break;
                case 11:
                    Proje11_AcisizDondurmeForm proje11Form = new Proje11_AcisizDondurmeForm(this);
                    this.Hide();
                    proje11Form.Show();
                    break;
                default:
                    MessageBox.Show($"Hafta 3 - Proje {projeNumarasi} henüz eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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