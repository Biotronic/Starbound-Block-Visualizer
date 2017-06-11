using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using StarboundVisualizer.Components;
using Point = StarboundVisualizer.Components.JsonClasses.Point;

namespace StarboundBlockVisualizer
{
    public partial class Form1 : Form
    {
        private bool _down;

        public Form1()
        {
            InitializeComponent();

            textBox1.Text = string.Join(Environment.NewLine, PathFinder.Paths);

            pictureBox2.ClientSize = new Size(256, 256);

            cmbColor.SelectedIndex = 0;

            visualControl1.Array = new bool[16, 16];

            visualControl1.Array[3, 3] = true;
            visualControl1.Array[3, 4] = true;
            visualControl1.Array[3, 5] = true;
            visualControl1.Array[4, 3] = true;
            visualControl1.Array[4, 4] = true;
            visualControl1.Array[4, 5] = true;
            visualControl1.Array[5, 3] = true;
            visualControl1.Array[5, 4] = true;
            visualControl1.Array[5, 5] = true;

            Application.Idle += Application_Idle;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawRawImage(pictureBox2);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            visualControl1.Repaint();
        }

        private void udVariant_ValueChanged(object sender, EventArgs e)
        {
            visualControl1.ColorIndex = cmbColor.SelectedIndex;
        }

        private void DrawRawImage(Control control)
        {
            var array = visualControl1.Array;

            var xf = array.GetLength(0) / (float)control.ClientSize.Width;
            var yf = array.GetLength(1) / (float)control.ClientSize.Height;

            var gfx = control.CreateGraphics();
            gfx.Clear(SystemColors.ButtonFace);

            var brush = new SolidBrush(Color.Black);

            for (var y = 0; y < array.GetLength(1); ++y)
            {
                for (var x = 0; x < array.GetLength(0); ++x)
                {
                    if (array[x, y])
                    {
                        gfx.FillRectangle(brush,
                            new Rectangle((int)(x / xf), control.ClientSize.Height - (int)((y + 1) / yf), (int)(1 / xf), (int)(1 / yf)));
                    }
                }
            }
        }

        private Point GetArrayIndex(bool[,] array, Control control, int x, int y)
        {

            var xf = array.GetLength(0) / (float)control.Width;
            var yf = array.GetLength(1) / (float)control.Height;

            x = (int)(x * xf);
            y = (int)((pictureBox2.ClientSize.Height - y) * yf);

            return new Point(x, y);
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left)) return;

            var pos = GetArrayIndex(visualControl1.Array, pictureBox2, e.X, e.Y);
            _down = !visualControl1.Array[pos.X, pos.Y];

            pictureBox2_MouseMove(sender, e);
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left)) return;
            var pos = GetArrayIndex(visualControl1.Array, pictureBox2, e.X, e.Y);

            if (visualControl1.Array[pos.X, pos.Y] == _down) return;


            visualControl1.Array[pos.X, pos.Y] = _down;

            DrawRawImage(pictureBox2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            var dir = Path.GetDirectoryName(openFileDialog1.FileName);

            Debug.Assert(dir != null, "dir != null");
            foreach (var file in Directory.EnumerateFiles(dir, "*.material", SearchOption.AllDirectories))
            {
                var material = new Material(file);
                visualControl1.Material = material;
                var shot = Crop(visualControl1.Screenshot());
                shot.Save(Path.Combine(textBox2.Text, material.MaterialName + ".png"));
            }
        }

        public static Bitmap Crop(Bitmap bmp)
        {
            var w = bmp.Width;
            var h = bmp.Height;

            Func<int, bool> allWhiteRow = row =>
            {
                for (var i = 0; i < w; ++i)
                    if (bmp.GetPixel(i, row).A != 0)
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = col =>
            {
                for (var i = 0; i < h; ++i)
                    if (bmp.GetPixel(col, i).A != 0)
                        return false;
                return true;
            };

            var topmost = 0;
            for (var row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                    topmost = row;
                else break;
            }

            var bottommost = 0;
            for (var row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                    bottommost = row;
                else break;
            }

            int leftmost = 0, rightmost = 0;
            for (var col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                    leftmost = col;
                else
                    break;
            }

            for (var col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                    rightmost = col;
                else
                    break;
            }

            if (rightmost == 0) rightmost = w;
            if (bottommost == 0) bottommost = h;

            var croppedWidth = rightmost - leftmost;
            var croppedHeight = bottommost - topmost;

            if (croppedWidth == 0)
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0)
            {
                topmost = 0;
                croppedHeight = h;
            }
            
            var target = new Bitmap(croppedWidth, croppedHeight);
            using (var g = Graphics.FromImage(target))
            {
                g.DrawImage(bmp,
                    new RectangleF(0, 0, croppedWidth, croppedHeight),
                    new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                    GraphicsUnit.Pixel);
            }
            return target;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            visualControl1.Material = new Material(openFileDialog1.FileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;

            textBox1.Text += Environment.NewLine + folderBrowserDialog1.SelectedPath;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PathFinder.Paths = textBox1.Lines.ToList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;

            textBox2.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}
