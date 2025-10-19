using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje1_GradyanOlusturmaForm : Form
    {
        private PictureBox pcbGoruntu;
        private Button btnOlustur;
        private Button btnGeri;
        private Form haftaFormu; // Bir önceki formu (Hafta1Form) tutmak için

        public Proje1_GradyanOlusturmaForm(Form parentForm)
        {
            // Programın Asıl Fonksiyonu
            InitializeComponent();

            haftaFormu = parentForm;
            this.Text = "Proje 1: Gradyan Oluşturma";

            // Resmi gösterecek PictureBox
            pcbGoruntu = new PictureBox();
            pcbGoruntu.Location = new Point(25, 25);
            pcbGoruntu.Size = new Size(256, 256);
            pcbGoruntu.BorderStyle = BorderStyle.FixedSingle;
            pcbGoruntu.BackColor = Color.LightGray;
            pcbGoruntu.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(pcbGoruntu);

            btnOlustur = new Button();
            btnOlustur.Text = "Gradyan Oluştur";
            btnOlustur.Size = new Size(150, 40);
            btnOlustur.Font = new Font("Arial", 10);
            btnOlustur.Location = new Point(300, 100);
            btnOlustur.Click += new EventHandler(btnOlustur_Click);
            this.Controls.Add(btnOlustur);

            btnGeri = new Button();
            btnGeri.Text = "Hafta Menüsüne Dön";
            btnGeri.Size = new Size(150, 40);
            btnGeri.Font = new Font("Arial", 10);
            btnGeri.BackColor = Color.LightCoral;
            btnGeri.Location = new Point(300, 160);
            btnGeri.Click += new EventHandler(btnGeri_Click);
            this.Controls.Add(btnGeri);
        }

        // Programın Asıl Fonksiyonu
        private void InitializeComponent()
        {
            this.Name = "Proje1_GriTonlamaForm";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void btnOlustur_Click(object sender, EventArgs e)
        {
            int matrisBoyutu = 256;

            Bitmap gradyanResmi = new Bitmap(matrisBoyutu, matrisBoyutu);

            for (int x = 0; x < matrisBoyutu; x++)
            {
                for (int y = 0; y < matrisBoyutu; y++)
                {
                    // x=0 ise renk=0 (siyah), x=255 ise renk=255 (beyaz)
                    int renkDegeri = x;

                    // Gri tonlama için R, G ve B değerlerinin hepsi aynı olmalı
                    Color yeniRenk = Color.FromArgb(renkDegeri, renkDegeri, renkDegeri);

                    gradyanResmi.SetPixel(x, y, yeniRenk);
                }
            }

            // Oluşturulan resmi PictureBox'ta gösteriyoruz
            pcbGoruntu.Image = gradyanResmi;
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Formu kapat.
        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show(); // Bir önceki menüyü (Hafta1Form) tekrar göster
        }
    }
}

