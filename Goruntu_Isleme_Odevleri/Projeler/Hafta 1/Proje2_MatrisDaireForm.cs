using System;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_MatrisDaireForm : Form
    {
        private PictureBox pcbGoruntu;
        private Button btnOlustur;
        private Button btnGeri;
        private Form haftaFormu;

        public Proje2_MatrisDaireForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 2: Matris ile Daire Çizme";

            pcbGoruntu = new PictureBox();
            pcbGoruntu.Location = new Point(25, 25);
            pcbGoruntu.Size = new Size(512, 512);
            pcbGoruntu.BorderStyle = BorderStyle.FixedSingle;
            pcbGoruntu.BackColor = Color.LightGray;
            pcbGoruntu.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(pcbGoruntu);

            btnOlustur = new Button();
            btnOlustur.Text = "Daireyi Oluştur";
            btnOlustur.Size = new Size(150, 40);
            btnOlustur.Font = new Font("Arial", 10);
            btnOlustur.Location = new Point(560, 220);
            btnOlustur.Click += new EventHandler(btnOlustur_Click);
            this.Controls.Add(btnOlustur);

            btnGeri = new Button();
            btnGeri.Text = "Hafta Menüsüne Dön";
            btnGeri.Size = new Size(150, 40);
            btnGeri.Font = new Font("Arial", 10);
            btnGeri.BackColor = Color.LightCoral;
            btnGeri.Location = new Point(560, 280);
            btnGeri.Click += new EventHandler(btnGeri_Click);
            this.Controls.Add(btnGeri);
        }

        private void InitializeComponent()
        {
            this.Name = "Proje2_MatrisDaireForm";
            this.Size = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void btnOlustur_Click(object sender, EventArgs e)
        {
            int matrisBoyutu = 512;
            int merkezNoktasi = matrisBoyutu / 2;
            int yariCap = 100;

            Bitmap daireResmi = new Bitmap(matrisBoyutu, matrisBoyutu);

            for (int j = 0; j < matrisBoyutu; j++)
            {
                for (int i = 0; i < matrisBoyutu; i++)
                {
                    double uzakliginKaresi = Math.Pow(i - merkezNoktasi, 2) + Math.Pow(j - merkezNoktasi, 2);

                    if (uzakliginKaresi < Math.Pow(yariCap, 2))
                    {
                        daireResmi.SetPixel(i, j, Color.White);
                    }
                    else
                    {
                        daireResmi.SetPixel(i, j, Color.Black);
                    }
                }
            }

            pcbGoruntu.Image = daireResmi;
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }
    }
}
