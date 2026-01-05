using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Goruntu_Isleme_Odevleri
{
    public partial class Proje2_KaydirmaForm : Form
    {
        private Panel pnlBottom;
        private PictureBox pcbCanvas;
        private Button btnYukle, btnSifirla, btnGeri;
        private Label lblBilgi;
        private Form haftaFormu; 

        private Rectangle limitRect;
        private const int LIMIT_WIDTH = 500;
        private const int LIMIT_HEIGHT = 400;

        private Bitmap originalBitmap;
        private Bitmap displayBitmap;

        private Point lastMousePos;
        private bool isDraggingHandle = false;
        private bool isMovingImage = false;
        private ActiveHandle currentHandle = ActiveHandle.None;

        private int shiftX_Amount = 0;
        private int shiftY_Amount = 0;
        private int imgOffsetX = 0;
        private int imgOffsetY = 0;

        private Rectangle[] handleRects = new Rectangle[8];
        private const int HANDLE_SIZE = 12;

        private enum ActiveHandle
        {
            None,
            TopLeft, TopMiddle, TopRight,
            MiddleLeft, MiddleRight,
            BottomLeft, BottomMiddle, BottomRight
        }

        public Proje2_KaydirmaForm(Form parentForm)
        {
            InitializeComponent();
            haftaFormu = parentForm; // Ana menüyü kaydet
            this.Text = "Proje 2: Serbest Taşıma ve Kaydırma";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupInterface();

            this.FormClosed += new FormClosedEventHandler(ProjeForm_FormClosed);
        }

        private void InitializeComponent() { this.Name = "Proje2_KaydirmaForm"; }

        private void SetupInterface()
        {
            pnlBottom = new Panel() { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };

            lblBilgi = new Label()
            {
                Text = "Karelerden tutup EĞİN (Shear). Resmin ortasından tutup TAŞIYIN (Move).",
                Location = new Point(20, 10),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnYukle = new Button() { Text = "Resim Yükle", Location = new Point(20, 35), Size = new Size(120, 35), BackColor = Color.LightBlue };
            btnYukle.Click += BtnYukle_Click;

            btnSifirla = new Button() { Text = "Sıfırla", Location = new Point(150, 35), Size = new Size(120, 35), BackColor = Color.LightYellow };
            btnSifirla.Click += (s, e) => {
                shiftX_Amount = 0; shiftY_Amount = 0;
                imgOffsetX = 0; imgOffsetY = 0;
                ApplyBalancedShear();
            };

            btnGeri = new Button() { Text = "Geri Dön", Location = new Point(840, 35), Size = new Size(120, 35), BackColor = Color.LightCoral };
            btnGeri.Click += new EventHandler(btnGeri_Click);

            pnlBottom.Controls.AddRange(new Control[] { lblBilgi, btnYukle, btnSifirla, btnGeri });

            pcbCanvas = new PictureBox() { Dock = DockStyle.Fill, BackColor = Color.LightGray, Cursor = Cursors.Default };

            int midX = (this.ClientSize.Width) / 2;
            int midY = (this.ClientSize.Height - 80) / 2;
            limitRect = new Rectangle(midX - LIMIT_WIDTH / 2, midY - LIMIT_HEIGHT / 2, LIMIT_WIDTH, LIMIT_HEIGHT);

            pcbCanvas.MouseDown += PcbCanvas_MouseDown;
            pcbCanvas.MouseMove += PcbCanvas_MouseMove;
            pcbCanvas.MouseUp += PcbCanvas_MouseUp;
            pcbCanvas.Paint += PcbCanvas_Paint;

            this.Controls.Add(pcbCanvas);
            this.Controls.Add(pnlBottom);

            displayBitmap = new Bitmap(1, 1);
        }

        private void BtnYukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap temp = new Bitmap(dialog.FileName);
                int targetW = (int)(LIMIT_WIDTH * 0.8);
                int targetH = (int)(LIMIT_HEIGHT * 0.8);
                float scale = Math.Min((float)targetW / temp.Width, (float)targetH / temp.Height);
                originalBitmap = new Bitmap(temp, new Size((int)(temp.Width * scale), (int)(temp.Height * scale)));
                temp.Dispose();

                if (originalBitmap.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    Bitmap clone = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(clone)) { g.DrawImage(originalBitmap, 0, 0); }
                    originalBitmap = clone;
                }

                shiftX_Amount = 0; shiftY_Amount = 0;
                imgOffsetX = 0; imgOffsetY = 0;

                displayBitmap = new Bitmap(pcbCanvas.Width, pcbCanvas.Height, PixelFormat.Format32bppArgb);
                ApplyBalancedShear();
            }
        }

        private void btnGeri_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProjeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (haftaFormu != null)
            {
                haftaFormu.Show();
            }
        }

        private void ApplyBalancedShear()
        {
            if (originalBitmap == null) return;

            using (Graphics g = Graphics.FromImage(displayBitmap)) { g.Clear(Color.Transparent); }

            int srcW = originalBitmap.Width;
            int srcH = originalBitmap.Height;
            double centerX = srcW / 2.0;
            double centerY = srcH / 2.0;

            int startDrawX = limitRect.X + (limitRect.Width - srcW) / 2 + imgOffsetX;
            int startDrawY = limitRect.Y + (limitRect.Height - srcH) / 2 + imgOffsetY;

            BitmapData srcData = originalBitmap.LockBits(new Rectangle(0, 0, srcW, srcH), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = displayBitmap.LockBits(new Rectangle(0, 0, displayBitmap.Width, displayBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int srcBytes = srcData.Stride * srcH;
            byte[] srcBuffer = new byte[srcBytes];
            Marshal.Copy(srcData.Scan0, srcBuffer, 0, srcBytes);

            int dstBytes = dstData.Stride * displayBitmap.Height;
            byte[] dstBuffer = new byte[dstBytes];

            int srcStride = srcData.Stride;
            int dstStride = dstData.Stride;

            for (int y = 0; y < srcH; y++)
            {
                double normalizedY = (centerY - y) / centerY;
                int xShift = (int)(shiftX_Amount * normalizedY);

                for (int x = 0; x < srcW; x++)
                {
                    double normalizedX = (centerX - x) / centerX;
                    int yShift = (int)(shiftY_Amount * normalizedX);

                    int destX = x + xShift + startDrawX;
                    int destY = y + yShift + startDrawY;

                    if (limitRect.Contains(destX, destY))
                    {
                        int srcIndex = (y * srcStride) + (x * 4);
                        int dstIndex = (destY * dstStride) + (destX * 4);

                        dstBuffer[dstIndex] = srcBuffer[srcIndex];
                        dstBuffer[dstIndex + 1] = srcBuffer[srcIndex + 1];
                        dstBuffer[dstIndex + 2] = srcBuffer[srcIndex + 2];
                        dstBuffer[dstIndex + 3] = srcBuffer[srcIndex + 3];
                    }
                }
            }

            Marshal.Copy(dstBuffer, 0, dstData.Scan0, dstBytes);
            originalBitmap.UnlockBits(srcData);
            displayBitmap.UnlockBits(dstData);

            UpdateHandlePositions(startDrawX, startDrawY, centerX, centerY);
            pcbCanvas.Image = displayBitmap;
            pcbCanvas.Invalidate();
        }

        private void UpdateHandlePositions(int offX, int offY, double cX, double cY)
        {
            int w = originalBitmap.Width;
            int h = originalBitmap.Height;
            int hs = HANDLE_SIZE;
            int hs2 = hs / 2;

            Point[] basePoints = {
                new Point(0, 0), new Point(w/2, 0), new Point(w, 0),
                new Point(0, h/2), new Point(w, h/2),
                new Point(0, h), new Point(w/2, h), new Point(w, h)
            };

            for (int i = 0; i < 8; i++)
            {
                int x = basePoints[i].X;
                int y = basePoints[i].Y;

                double normY = (cY - y) / cY;
                int xShift = (int)(shiftX_Amount * normY);

                double normX = (cX - x) / cX;
                int yShift = (int)(shiftY_Amount * normX);

                int finalX = x + xShift + offX;
                int finalY = y + yShift + offY;

                handleRects[i] = new Rectangle(finalX - hs2, finalY - hs2, hs, hs);
            }
        }

        private void PcbCanvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.DimGray, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Solid }, limitRect);

            if (originalBitmap == null) return;

            Pen linePen = new Pen(Color.Blue, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            e.Graphics.DrawLine(linePen, Center(handleRects[0]), Center(handleRects[2]));
            e.Graphics.DrawLine(linePen, Center(handleRects[5]), Center(handleRects[7]));
            e.Graphics.DrawLine(linePen, Center(handleRects[0]), Center(handleRects[5]));
            e.Graphics.DrawLine(linePen, Center(handleRects[2]), Center(handleRects[7]));

            foreach (Rectangle r in handleRects)
            {
                bool isInside = limitRect.Contains(Center(r));
                Brush fillBrush = isInside ? Brushes.White : Brushes.OrangeRed;
                e.Graphics.FillRectangle(fillBrush, r);
                e.Graphics.DrawRectangle(Pens.Black, r);
            }
        }
        private Point Center(Rectangle r) => new Point(r.X + r.Width / 2, r.Y + r.Height / 2);

        private void PcbCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalBitmap == null) return;
            currentHandle = ActiveHandle.None;
            isDraggingHandle = false;
            isMovingImage = false;

            for (int i = 0; i < 8; i++)
            {
                Rectangle hit = handleRects[i]; hit.Inflate(5, 5);
                if (hit.Contains(e.Location))
                {
                    currentHandle = (ActiveHandle)(i + 1);
                    isDraggingHandle = true;
                    lastMousePos = e.Location;
                    return;
                }
            }

            isMovingImage = true;
            lastMousePos = e.Location;
            pcbCanvas.Cursor = Cursors.SizeAll;
        }

        private void PcbCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHandle && !isMovingImage)
            {
                bool overHandle = false;
                if (originalBitmap != null)
                {
                    foreach (Rectangle r in handleRects)
                    {
                        Rectangle hit = r; hit.Inflate(5, 5);
                        if (hit.Contains(e.Location)) { overHandle = true; break; }
                    }
                }
                pcbCanvas.Cursor = overHandle ? Cursors.Hand : Cursors.Default;
                return;
            }

            int deltaX = e.X - lastMousePos.X;
            int deltaY = e.Y - lastMousePos.Y;

            if (isMovingImage)
            {
                imgOffsetX += deltaX;
                imgOffsetY += deltaY;
                ApplyBalancedShear();
            }
            else if (isDraggingHandle)
            {
                switch (currentHandle)
                {
                    case ActiveHandle.TopLeft:
                    case ActiveHandle.TopMiddle:
                    case ActiveHandle.TopRight:
                    case ActiveHandle.BottomLeft:
                    case ActiveHandle.BottomMiddle:
                    case ActiveHandle.BottomRight:
                        shiftX_Amount += deltaX; break;
                }
                switch (currentHandle)
                {
                    case ActiveHandle.TopLeft:
                    case ActiveHandle.MiddleLeft:
                    case ActiveHandle.BottomLeft:
                    case ActiveHandle.TopRight:
                    case ActiveHandle.MiddleRight:
                    case ActiveHandle.BottomRight:
                        shiftY_Amount += deltaY; break;
                }
                ApplyBalancedShear();
            }

            lastMousePos = e.Location;
        }

        private void PcbCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingHandle = false;
            isMovingImage = false;
            currentHandle = ActiveHandle.None;
            pcbCanvas.Cursor = Cursors.Default;
        }
    }
}