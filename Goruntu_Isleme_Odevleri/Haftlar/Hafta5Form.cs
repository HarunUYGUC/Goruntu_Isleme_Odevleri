using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Hafta5Form : Form
    {
        private Panel projePaneli;
        private Button btnGeri;
        private Label lblBaslik;
        private Form anaMenuFormu;

        public Hafta5Form(Form anaMenu)
        {
            InitializeComponent();
            anaMenuFormu = anaMenu;
            this.Text = "Hafta 5 Projeleri";

            lblBaslik = new Label();
            lblBaslik.Text = "Hafta 5 Projeleri";
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

            int projeSayisi = 8;
            for (int i = 1; i <= projeSayisi; i++)
            {
                Button projeButon = new Button();

                if (i == 1)
                {
                    projeButon.Text = "Proje 1: Filtreleme Karşılaştırması";
                }
                else if (i == 2)
                {
                    projeButon.Text = "Proje 2: Özel Bulanıklaştırma Algoritması";
                }
                else if (i == 3)
                {
                    projeButon.Text = "Proje 3: Bölgesel Bulanıklaştırma (Buzlu Cam)";
                }
                else if (i == 4)
                {
                    projeButon.Text = "Proje 4: Gauss Filtresinde Standart Sapma (Sigma) Etkisi";
                }
                else if (i == 5)
                {
                    projeButon.Text = "Proje 5: Bina Netleştirme (Arka Plan Bulanıklaştırma)";
                }
                else if (i == 6)
                {
                    projeButon.Text = "Proje 6: Mouse Takip Eden Dinamik Blur";
                }
                else if (i == 7)
                {
                    projeButon.Text = "Proje 7: En Sık Tekrar Eden Renk (Mode) Filtresi";
                }
                else if (i == 8)
                {
                    projeButon.Text = "Proje 8: Piramit (Lineer) Bulanıklaştırma";
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
            this.Name = "Hafta5Form";
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
                    Proje1_FiltrelemeKarsilastirmaForm proje1Form = new Proje1_FiltrelemeKarsilastirmaForm(this);
                    this.Hide();
                    proje1Form.Show();
                    break;
                case 2:
                    Proje2_OzelBulaniklastirmaForm proje2Form = new Proje2_OzelBulaniklastirmaForm(this);
                    this.Hide();
                    proje2Form.Show();
                    break;
                case 3:
                    Proje3_BuzluCamForm proje3Form = new Proje3_BuzluCamForm(this);
                    this.Hide();
                    proje3Form.Show();
                    break;
                case 4:
                    Proje4_GaussSigmaForm proje4Form = new Proje4_GaussSigmaForm(this);
                    this.Hide();
                    proje4Form.Show();
                    break;
                case 5:
                    Proje5_BinaBulaniklastirmaForm proje5Form = new Proje5_BinaBulaniklastirmaForm(this);
                    this.Hide();
                    proje5Form.Show();
                    break;
                case 6:
                    Proje6_MouseBlurForm proje6Form = new Proje6_MouseBlurForm(this);
                    this.Hide();
                    proje6Form.Show();
                    break;
                case 7:
                    Proje7_ModFiltresiForm proje7Form = new Proje7_ModFiltresiForm(this);
                    this.Hide();
                    proje7Form.Show();
                    break;
                case 8:
                    Proje8_PiramitBulaniklastirmaForm proje8Form = new Proje8_PiramitBulaniklastirmaForm(this);
                    this.Hide();
                    proje8Form.Show();
                    break;
                default:
                    MessageBox.Show($"Hafta 5 - Proje {projeNumarasi} henüz eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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