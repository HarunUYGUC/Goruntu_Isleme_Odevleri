using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class MainForm : Form
    {
        private Panel haftaPaneli;
        private Label lblBaslik;

        public MainForm()
        {
            InitializeComponent();

            this.Text = "Görüntü İşleme Projeleri, Ana Menü";

            lblBaslik = new Label();
            lblBaslik.Text = "Lütfen Bir Hafta Seçiniz";
            lblBaslik.Font = new Font("Arial", 14, FontStyle.Bold);
            lblBaslik.AutoSize = true;
            lblBaslik.Location = new Point(95, 20);
            this.Controls.Add(lblBaslik);

            haftaPaneli = new Panel();
            haftaPaneli.Location = new Point(25, 60);
            haftaPaneli.Size = new Size(380, 380);
            haftaPaneli.AutoScroll = true;
            haftaPaneli.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(haftaPaneli);

            int butonSayisi = 12;
            int baslangicY = 15;
            int bosluk = 10;
            int butonYukseklik = 45;

            for (int i = 1; i <= butonSayisi; i++)
            {
                Button yeniButon = new Button();
                yeniButon.Text = "Hafta " + i;
                yeniButon.Name = "btnHafta" + i;
                yeniButon.Tag = i;
                yeniButon.Size = new Size(320, butonYukseklik);
                yeniButon.Font = new Font("Arial", 12);
                int yKonumu = baslangicY + ((i - 1) * (butonYukseklik + bosluk));
                yeniButon.Location = new Point(15, yKonumu);

                yeniButon.Click += new EventHandler(HaftaButonu_Click);
                haftaPaneli.Controls.Add(yeniButon);
            }
        }

        private void InitializeComponent()
        {
            this.Name = "MainForm";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void HaftaButonu_Click(object sender, EventArgs e)
        {
            Button tiklananButon = (Button)sender;
            int haftaNumarasi = (int)tiklananButon.Tag;

            switch (haftaNumarasi)
            {
                case 1:
                    Hafta1Form hafta1Form = new Hafta1Form(this);
                    this.Hide();
                    hafta1Form.Show();
                    break;
                case 2:
                    Hafta2Form hafta2Form = new Hafta2Form(this);
                    this.Hide();
                    hafta2Form.Show();
                    break;
                case 3:
                    Hafta3Form hafta3Form = new Hafta3Form(this);
                    this.Hide();
                    hafta3Form.Show();
                    break;
                case 4:
                    Hafta4Form hafta4Form = new Hafta4Form(this);
                    this.Hide();
                    hafta4Form.Show();
                    break;
                default:
                    MessageBox.Show($"Hafta {haftaNumarasi} projeleri henüz eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }
}

