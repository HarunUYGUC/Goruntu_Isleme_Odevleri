using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Hafta4Form : Form
    {
        private Panel projePaneli;
        private Button btnGeri;
        private Label lblBaslik;
        private Form anaMenuFormu;

        public Hafta4Form(Form anaMenu)
        {
            InitializeComponent();
            anaMenuFormu = anaMenu;
            this.Text = "Hafta 4 Projeleri";

            lblBaslik = new Label();
            lblBaslik.Text = "Hafta 4 Projeleri";
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

            // Hafta 4'te sadece 1 proje var
            Button projeButon = new Button();
            projeButon.Text = "Proje 1: Perspektif Düzeltme";
            projeButon.Tag = 1;
            projeButon.Size = new Size(320, 45);
            projeButon.Font = new Font("Arial", 12);
            projeButon.Location = new Point(15, 15);
            projeButon.Click += new EventHandler(ProjeButonu_Click);
            projePaneli.Controls.Add(projeButon);

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
            this.Name = "Hafta4Form";
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
                    Proje1_PerspektifDuzeltmeForm proje1Form = new Proje1_PerspektifDuzeltmeForm(this);
                    this.Hide();
                    proje1Form.Show();
                    break;
                default:
                    MessageBox.Show("Bu proje henüz eklenmedi.");
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