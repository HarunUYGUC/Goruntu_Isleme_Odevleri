using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje10_RenkPaletiForm : Form
    {
        private PictureBox pcbColorPicker, pcbLuminance;
        private Panel pnlCurrentColor;
        private FlowLayoutPanel pnlCustomColors;
        private Button btnAddCustom, btnGeri;
        private Label lblRed, lblGreen, lblBlue, lblCustomColors;
        private Form haftaFormu;

        private Bitmap colorPickerBitmap;
        private Color selectedColor = Color.Green;
        private float selectedHue = 120f, selectedSaturation = 1f;
        private const int MAX_CUSTOM_COLORS = 54;

        public Proje10_RenkPaletiForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm;
            this.Text = "Proje 10: Özel Renk Paleti";

            pcbColorPicker = new PictureBox() { Location = new Point(270, 25), Size = new Size(256, 256), BorderStyle = BorderStyle.FixedSingle };
            pcbLuminance = new PictureBox() { Location = new Point(540, 25), Size = new Size(30, 256), BorderStyle = BorderStyle.FixedSingle };
            pnlCurrentColor = new Panel() { Location = new Point(270, 300), Size = new Size(100, 50), BorderStyle = BorderStyle.Fixed3D };

            lblRed = new Label() { Text = "Kırmızı:", Location = new Point(400, 300), AutoSize = true };
            lblGreen = new Label() { Text = "Yeşil:", Location = new Point(400, 320), AutoSize = true };
            lblBlue = new Label() { Text = "Mavi:", Location = new Point(400, 340), AutoSize = true };

            btnAddCustom = new Button() { Text = "Özel Renklere Ekle", Location = new Point(270, 360), Size = new Size(244, 30) };

            lblCustomColors = new Label() { Text = "Özel Renkler:", Location = new Point(25, 25), Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };

            pnlCustomColors = new FlowLayoutPanel()
            {
                Location = new Point(25, 50),
                Size = new Size(220, 150),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = SystemColors.ControlLight,
                AutoScroll = true 
            };

            btnGeri = new Button() { Text = "Hafta Menüsüne Dön", Location = new Point(this.ClientSize.Width - 175, 400), Size = new Size(150, 40), BackColor = Color.LightCoral };

            this.Controls.AddRange(new Control[] { pcbColorPicker, pcbLuminance, pnlCurrentColor, lblRed, lblGreen, lblBlue, btnAddCustom, lblCustomColors, pnlCustomColors, btnGeri });

            pcbColorPicker.MouseDown += PcbColorPicker_MouseInteraction;
            pcbColorPicker.MouseMove += PcbColorPicker_MouseInteraction;
            pcbLuminance.MouseDown += PcbLuminance_MouseInteraction;
            pcbLuminance.MouseMove += PcbLuminance_MouseInteraction;
            btnAddCustom.Click += btnAddCustom_Click;
            btnGeri.Click += btnGeri_Click; 

            this.Load += Proje10_RenkPaletiForm_Load;
        }

        private void InitializeComponent()
        {
            this.Name = "Proje10_RenkPaletiForm";
            this.Size = new Size(610, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void Proje10_RenkPaletiForm_Load(object sender, EventArgs e)
        {
            GenerateColorPickerImage();
            UpdateLuminancePicker();
            UpdateCurrentColor(selectedColor);
        }

        private void GenerateColorPickerImage()
        {
            colorPickerBitmap = new Bitmap(pcbColorPicker.Width, pcbColorPicker.Height);
            for (int y = 0; y < colorPickerBitmap.Height; y++)
            {
                for (int x = 0; x < colorPickerBitmap.Width; x++)
                {
                    // Pikselin konumunu HSL değerlerine çeviririz:
                    // Yatay konum (x), Renk Tonunu (Hue) belirler. Soldan sağa gittikçe renkler gökkuşağı gibi değişir (0-360 derece).
                    float hue = x / (float)colorPickerBitmap.Width * 360f;

                    // Dikey konum (y), Doygunluğu (Saturation) belirler.
                    // Yukarıdan aşağı gittikçe renkler canlıdan (%100) soluk griye (%0) dönüşür.
                    float saturation = 1f - (y / (float)colorPickerBitmap.Height);

                    // Bu HSL değerlerini, bilgisayarın anlayacağı bir RGB rengine çevirip pikseli boyarız.
                    // Parlaklığı (Luminance) sabit bir değer (0.5f yani %50) olarak ayarlarız ki renklerin en saf halini görelim.
                    colorPickerBitmap.SetPixel(x, y, HslToRgb(hue, saturation, 0.5f));
                }
            }
            pcbColorPicker.Image = colorPickerBitmap;
        }

        private void UpdateLuminancePicker()
        {
            if (pcbLuminance.Image != null) pcbLuminance.Image.Dispose();
            Bitmap lumBitmap = new Bitmap(pcbLuminance.Width, pcbLuminance.Height);
            for (int y = 0; y < lumBitmap.Height; y++)
            {
                float luminance = 1f - (y / (float)lumBitmap.Height);
                Color c = HslToRgb(selectedHue, selectedSaturation, luminance);
                for (int x = 0; x < lumBitmap.Width; x++)
                {
                    lumBitmap.SetPixel(x, y, c);
                }
            }
            pcbLuminance.Image = lumBitmap;
        }

        private void PcbColorPicker_MouseInteraction(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = Math.Max(0, Math.Min(e.X, pcbColorPicker.Width - 1));
                int y = Math.Max(0, Math.Min(e.Y, pcbColorPicker.Height - 1));

                selectedHue = x / (float)pcbColorPicker.Width * 360f;
                selectedSaturation = 1f - (y / (float)pcbColorPicker.Height);

                selectedColor = HslToRgb(selectedHue, selectedSaturation, 0.5f);
                UpdateCurrentColor(selectedColor);
                UpdateLuminancePicker();
            }
        }

        private void PcbLuminance_MouseInteraction(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int y = Math.Max(0, Math.Min(e.Y, pcbLuminance.Height - 1));
                float luminance = 1f - (y / (float)pcbLuminance.Height);
                selectedColor = HslToRgb(selectedHue, selectedSaturation, luminance);
                UpdateCurrentColor(selectedColor);
            }
        }

        private void UpdateCurrentColor(Color c)
        {
            pnlCurrentColor.BackColor = c;
            lblRed.Text = $"Kırmızı: {c.R}";
            lblGreen.Text = $"Yeşil: {c.G}";
            lblBlue.Text = $"Mavi: {c.B}";
        }
        private void btnAddCustom_Click(object sender, EventArgs e)
        {
            if (pnlCustomColors.Controls.Count >= MAX_CUSTOM_COLORS)
            {
                MessageBox.Show("Özel renk paletiniz dolu. Daha fazla renk ekleyemezsiniz.", "Palet Dolu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Panel pnl = new Panel()
            {
                Size = new Size(20, 20),
                BackColor = pnlCurrentColor.BackColor,
                Margin = new Padding(2),
                BorderStyle = BorderStyle.Fixed3D
            };
            pnl.Click += (s, args) => {
                Panel clickedPanel = (Panel)s;
                selectedColor = clickedPanel.BackColor;
                UpdateCurrentColor(selectedColor);
                selectedHue = selectedColor.GetHue();
                selectedSaturation = selectedColor.GetSaturation();
                UpdateLuminancePicker();
            };
            pnlCustomColors.Controls.Add(pnl);

            if (pnlCustomColors.Controls.Count >= MAX_CUSTOM_COLORS)
            {
                btnAddCustom.Enabled = false;
            }
        }

        public static Color HslToRgb(float h, float s, float l)
        {
            float r, g, b;
            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                Func<float, float, float, float> hue2rgb = (p1, q1, t) =>
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1 / 6.0) return p1 + (q1 - p1) * 6 * t;
                    if (t < 1 / 2.0) return q1;
                    if (t < 2 / 3.0) return p1 + (q1 - p1) * (2 / 3.0f - t) * 6;
                    return p1;
                };

                float q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                h /= 360f;
                r = hue2rgb(p, q, h + 1 / 3.0f);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1 / 3.0f);
            }
            return Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            haftaFormu.Show();
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
