using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Hafta1Form : Form
    {
        private Panel projePaneli;
        private Button btnGeri;
        private Label lblBaslik;
        private Form anaMenuFormu;

        public Hafta1Form(Form anaMenu)
        {
            // Programın Asıl Fonksiyonu
            InitializeComponent();

            anaMenuFormu = anaMenu;
            this.Text = "Hafta 1 Projeleri";

            lblBaslik = new Label();
            lblBaslik.Text = "Hafta 1 Projeleri";
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
                    projeButon.Text = "Proje 1: Gradyan Oluşturma";
                }
                else if (i == 2)
                {
                    projeButon.Text = "Proje 2: Matris ile Daire Çizme";
                }
                else if (i == 3)
                {
                    projeButon.Text = "Proje 3: Örnekleme ve Renk Nicemlemesi";
                }
                else if (i == 4)
                {
                    projeButon.Text = "Proje 4: Mozaik Oluşturma";
                }
                else if (i == 5)
                {
                    projeButon.Text = "Proje 5: RGB Renk Kanalları";
                }
                else if (i == 6)
                {
                    projeButon.Text = "Proje 6: Renk Bantlarının Gri Gösterimi";
                }
                else if (i == 7)
                {
                    projeButon.Text = "Proje 7: Yapay Renkli Görüntüler";
                }
                else if (i == 8)
                {
                    projeButon.Text = "Proje 8: Bit Derinliği Azaltma";
                }
                else
                {
                    projeButon.Text = "Proje " + i;
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

        // Programın Asıl Fonksiyonu.
        private void InitializeComponent()
        {
            this.Name = "Hafta1Form";
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
                    Proje1_GradyanOlusturmaForm proje1Form = new Proje1_GradyanOlusturmaForm(this);
                    this.Hide();
                    proje1Form.Show();
                    break;
                case 2:
                    Proje2_MatrisDaireForm proje2Form = new Proje2_MatrisDaireForm(this);
                    this.Hide();
                    proje2Form.Show();
                    break;
                case 3:
                    Proje3_OrneklemeNicemlemeForm proje3Form = new Proje3_OrneklemeNicemlemeForm(this);
                    this.Hide();
                    proje3Form.Show();
                    break;
                case 4:
                    Proje4_MozaikForm proje4Form = new Proje4_MozaikForm(this);
                    this.Hide();
                    proje4Form.Show();
                    break;
                case 5:
                    Proje5_RGBKanallariForm proje5Form = new Proje5_RGBKanallariForm(this);
                    this.Hide();
                    proje5Form.Show();
                    break;
                case 6:
                    Proje6_GriBantlarForm proje6Form = new Proje6_GriBantlarForm(this);
                    this.Hide();
                    proje6Form.Show();
                    break;
                case 7:
                    Proje7_YapayRenkForm proje7Form = new Proje7_YapayRenkForm(this);
                    this.Hide();
                    proje7Form.Show();
                    break;
                case 8:
                    Proje8_BitDerinligiForm proje8Form = new Proje8_BitDerinligiForm(this);
                    this.Hide();
                    proje8Form.Show();
                    break;
                default:
                    MessageBox.Show($"Hafta 1 - Proje {projeNumarasi} henüz eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

