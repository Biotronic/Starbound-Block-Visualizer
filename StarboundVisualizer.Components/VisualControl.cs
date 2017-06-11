using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using StarboundVisualizer.Components.JsonClasses;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Point = StarboundVisualizer.Components.JsonClasses.Point;

namespace StarboundVisualizer.Components
{
    public partial class VisualControl : GLControl
    {
        private Material _material;
        public int ColorIndex { get; set; }
        public Size TextureSize { get; set; }
        
        public Material Material
        {
            get { return _material; }
            set
            {
                if (_material == value) return;
                
                _material = value;
                Texture = LoadTexture();
            }
        }

        public bool[,] Array { get; set; }
        private int Texture { get; set; }

        public VisualControl() : base(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 32, 32), 3, 0, GraphicsContextFlags.Default)
        {
            InitializeComponent();
            BackColor = Color.Transparent;
        }

        private int GetVariant(Point pos)
        {
            return (int) (pos.X * 2473 + pos.Y) % Material.Variants;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) return;
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnResize(EventArgs e)
        {
            if (DesignMode) return;

            MakeCurrent();
            GL.ColorMask(true, true, true, true);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.ClearColor(0, 0, 0, 0);
            GL.Ortho(0, Width / 2f, 0, Height / 2f, -1, 1);
            GL.Viewport(0, 0, Width, Height);
            SwapBuffers();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode) return;

            DrawImage();
            SwapBuffers();
        }

        private void DrawImage()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            if (Array == null) return;
            if (Material == null) return;

            for (var y = 0; y < Array.GetLength(1); ++y)
            {
                for (var x = 0; x < Array.GetLength(0); ++x)
                {
                    if (!Array[x, y]) continue;

                    foreach (var match in Material.Matches)
                    {
                        Apply(new Point(x, y), match);
                    }
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Flush();
        }

        private void Apply(Point pos, Match rule)
        {
            if (!IsMatch(pos, rule)) return;

            foreach (var subMatch in rule.SubMatches)
            {
                Apply(pos, subMatch);
            }

            foreach (var piece in rule.Pieces)
            {
                DrawPiece(piece, pos);
            }
        }

        private void DrawPiece(ReplacementPiece piece, Point pos)
        {
            var p = pos * 8 + piece.Position;
            var tile = GetTile(piece.Piece, pos);

            GL.PushMatrix();
            GL.Translate(p.X, p.Y, 0);
            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(TexCoord(tile, 0, 1));
            GL.Vertex2(Scale(tile, 0, 0));
            GL.TexCoord2(TexCoord(tile, 1, 1));
            GL.Vertex2(Scale(tile, 1, 0));
            GL.TexCoord2(TexCoord(tile, 1, 0));
            GL.Vertex2(Scale(tile, 1, 1));
            GL.TexCoord2(TexCoord(tile, 0, 0));
            GL.Vertex2(Scale(tile, 0, 1));

            GL.End();
            GL.PopMatrix();
        }

        private static Vector2 Scale(Rectangle source, int w, int h)
        {
            var x = source.Width * w;
            var y = source.Height * h;

            return new Vector2(x, y);
        }

        private Vector2 TexCoord(Rectangle source, int w, int h)
        {
            var x = source.X + source.Width * w;
            var y = source.Y + source.Height * h;

            return new Vector2(x / (float)NextPowerOfTwo(TextureSize.Width), y / (float)NextPowerOfTwo(TextureSize.Height));
        }

        private Rectangle GetTile(string pieceName, Point pos)
        {
            var piece = Material.Pieces[pieceName];
            return (piece.TexturePosition + piece.ColorStride * ColorIndex + piece.VariantStride * GetVariant(pos)).ToRect(piece.TextureSize);
        }

        private bool IsMatch(Point pos, Match rule)
        {
            return rule.MatchAllPoints.All(DoesMatch(pos));
        }

        private Func<MatchPoint, bool> DoesMatch(Point pos)
        {
            return a =>
            {
                var p = a.Position + new Point((int)pos.X, (int)pos.Y);
                if (p.X < 0 || p.X >= Array.GetLength(0)) return false;
                if (p.Y < 0 || p.Y >= Array.GetLength(1)) return false;
                if (a.MatchType == "Shadows" || a.MatchType == "NotShadows") return false;

                return Array[p.X, p.Y] == (a.MatchType == "EqualsSelf");
            };
        }

        public void Repaint()
        {
            Invalidate();
        }

        private int LoadTexture()
        {
            var bitmap = new Bitmap(Material.TextureSource);
            TextureSize = bitmap.Size;
            var texture = GL.GenTexture();
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, NextPowerOfTwo(bitmap.Width), NextPowerOfTwo(bitmap.Height), 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, bitmap.Width, bitmap.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }

        private static int NextPowerOfTwo(int x)
        {
            return (int)Math.Pow(2, Math.Ceiling(Math.Log(x) / Math.Log(2)));
        }

        public Bitmap Screenshot()
        {
            DrawImage();
            var bitmap = new Bitmap(Width, Height);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

            bitmap.UnlockBits(bitmapData);

            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }
    }
}
