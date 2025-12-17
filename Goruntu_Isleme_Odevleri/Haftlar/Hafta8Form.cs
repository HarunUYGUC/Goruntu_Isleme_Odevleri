using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Hafta8Form : Form
    {
        private Panel projePaneli;
        private Button btnGeri;
        private Label lblBaslik;
        private Form anaMenuFormu;

        public Hafta8Form(Form anaMenu)
        {
            InitializeComponent();
            anaMenuFormu = anaMenu;
            this.Text = "Hafta 8 Projeleri";

            lblBaslik = new Label();
            lblBaslik.Text = "Hafta 8 Projeleri";
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

            int projeSayisi = 4;
            for (int i = 1; i <= projeSayisi; i++)
            {
                Button projeButon = new Button();

                if (i == 1)
                {
                    projeButon.Text = "Proje 1: Ceviz/Fındık Sayma ve Sınıflandırma";
                }
                else if (i == 2)
                {
                    projeButon.Text = "Proje 2: Arka Plan Değiştirme (Chroma Key)";
                }
                else if (i == 3)
                {
                    projeButon.Text = "Proje 3: Morfolojik İşlemler (Erosion & Dilation)";
                }
                else if (i == 4)
                {
                    projeButon.Text = "Proje 4: Para Sayma ve Yazı/Tura Tespiti";
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
            this.Name = "Hafta8Form";
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
                    Proje1_CevizFindikForm proje1Form = new Proje1_CevizFindikForm(this);
                    this.Hide();
                    proje1Form.Show();
                    break;
                case 2:
                    Proje2_ArkaPlanDegistirmeForm proje2Form = new Proje2_ArkaPlanDegistirmeForm(this);
                    this.Hide();
                    proje2Form.Show();
                    break;
                case 3:
                    Proje3_MorfolojiForm proje3Form = new Proje3_MorfolojiForm(this);
                    this.Hide();
                    proje3Form.Show();
                    break;
                case 4:
                    Proje4_ParaSaymaForm proje4Form = new Proje4_ParaSaymaForm(this);
                    this.Hide();
                    proje4Form.Show();
                    break;
                default:
                    MessageBox.Show($"Hafta 8 - Proje {projeNumarasi} henüz eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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