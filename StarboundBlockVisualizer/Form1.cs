using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarboundBlockVisualizer.JsonClasses;
using Point = StarboundBlockVisualizer.JsonClasses.Point;

namespace StarboundBlockVisualizer
{
    public partial class Form1 : Form
    {
        private static readonly string[] Paths = { @"D:\GitHub\FrackinUniverse\", @"E:\Starbound\unpacked\" };
        private Image Image { get; set; }
        private Dictionary<string, Piece> Pieces { get; set; }
        public List<Match> Matches { get; set; }
        private bool _ready = false;

        private readonly bool[,] _array = new bool[16, 16];
        private bool _down;

        public Form1()
        {
            InitializeComponent();

            pictureBox2.ClientSize = new Size(256, 256);

            Foo();
        }

        private void Foo()
        {
            const string materialPath = @"\tiles\materials\avalitechpanel.material";

            var material = ParseFile(materialPath);
            var renderTemplatePath = (string)material.SelectToken("$.renderTemplate");
            var renderTemplate = ParseFile(renderTemplatePath, Path.GetDirectoryName(FindFile(materialPath)));
            var texturePath = (string)material.SelectToken("$.renderParameters.texture");

            var piecesToken = renderTemplate.SelectToken("$.pieces");

            Pieces = piecesToken.ToObject<Dictionary<string, Piece>>();
            Matches = renderTemplate.SelectToken("$.matches[0][1]").ToObject<List<Match>>();
            Image = Image.FromFile(FindFile(texturePath, Path.GetDirectoryName(FindFile(materialPath))));

            udVariant.Minimum = 0;
            udVariant.Maximum = (long)material.SelectToken("$.renderParameters.variants") - 1;

            cmbColor.SelectedIndex = 0;
            _ready = true;


            var sb = new StringBuilder();
            foreach (var pair in Pieces)
            {
                //sb.AppendLine($".{pair.Key} {{\n" +
                //              $"    width:  {pair.Value.TextureSize.X}px;\n" +
                //              $"    height: {pair.Value.TextureSize.Y}px;\n" +
                //              $"    background-position: {-pair.Value.TexturePosition.X}px {-pair.Value.TexturePosition.Y}px;\n"+
                //              $"}}\n");

                //if (string.IsNullOrEmpty(pair.Value.Texture))
                //sb.AppendLine($"<div><div class=\"{pair.Key}\"></div>{pair.Key}</div>");
            }
            textBox1.Text = sb.ToString();
        }

        private static string FindFile(string filename, string basePath = null)
        {
            filename = filename.Replace('/', '\\');
            if (filename.StartsWith(@"\"))
            {
                foreach (var path in Paths)
                {
                    var combined = Path.Combine(path, filename.Substring(1));
                    if (File.Exists(combined)) return combined;
                }
            }
            if (basePath != null)
            {
                return Path.Combine(basePath, filename);
            }
            return null;
        }

        private static JObject ParseFile(string filename, string basePath = null)
        {
            filename = FindFile(filename, basePath);

            using (var fs = File.OpenRead(filename))
            using (var sr = new StreamReader(fs))
            using (var jr = new JsonTextReader(sr))
                return JObject.Load(jr);
        }

        private void udVariant_ValueChanged(object sender, System.EventArgs e)
        {
            DrawFancyImage(_array, pictureBox1);
        }

        private void DrawFancyImage(bool[,] array, Control control)
        {
            var gfx = control.CreateGraphics();
            gfx.Clear(SystemColors.ButtonFace);

            for (var y = 0; y < array.GetLength(1); ++y)
            {
                for (var x = 0; x < array.GetLength(0); ++x)
                {
                    if (!array[x, y]) continue;

                    foreach (var match in Matches)
                    {
                        Apply(gfx, Image, array, new Point(x, y), match);
                    }
                }
            }
        }

        private void Apply(Graphics gfx, Image texture, bool[,] array, Point pos, Match rule)
        {
            if (!IsMatch(array, pos, rule)) return;

            Debug.Assert(rule.MatchAllPoints.Count <= 1);
            Debug.Assert(rule.Pieces.Count <= 1);
            //textBox1.Text += $@"pos: {pos} + {piece.Position}, {piece.Piece} {Environment.NewLine}";
            
            foreach (var subMatch in rule.SubMatches)
            {
                Apply(gfx, texture, array, pos, subMatch);
            }

            foreach (var piece in rule.Pieces)
            {
                DrawPiece(piece, gfx, texture, pos);
            }
        }

        private static readonly string[] Adds = { "bottomRightEdge", "bottomLeftEdge", "cornerLR", "cornerLL" };
        private static readonly string[] Subs = { "topRightEdge", "topLeftEdge", "cornerUR", "cornerUL" };

        private void DrawPiece(ReplacementPiece piece, Graphics gfx, Image texture, Point pos)
        {
            var p = (pos * 8 + piece.Position);
            var tile = GetTile(piece.Piece);

            var p2 = piece.Position;

            if (Adds.Contains(piece.Piece))
            {
                p.Y += 12;
                p2.Y += 12;
            }

            if (Subs.Contains(piece.Piece))
            {
                p.Y -= 12;
                p2.Y -= 12;
            }


            //// ReSharper disable once LocalizableElement
            //textBox1.Text +=
            //    $"<div class=\"{piece.Piece}\" style=\"left: {p.X}px; top: {p.Y}px;\"></div>";

            gfx.DrawImage(texture, p.ToRect(new Point(tile.Width, tile.Height)), tile, GraphicsUnit.Pixel);
        }

        private Rectangle GetTile(string pieceName)
        {
            var variant = (int)udVariant.Value;
            var color = cmbColor.SelectedIndex;
            var piece = Pieces[pieceName];
            return (piece.TexturePosition + piece.ColorStride * color + piece.VariantStride * variant).ToRect(piece.TextureSize);
        }

        private static bool IsMatch(bool[,] array, Point pos, Match rule)
        {
            return rule.MatchAllPoints.All(DoesMatch(array, pos));
        }

        private static Func<MatchPoint, bool> DoesMatch(bool[,] array, Point pos)
        {
            return a =>
            {
                var p = a.Position + new Point((int)pos.X, (int)pos.Y);
                if (p.X < 0 || p.X >= array.GetLength(0)) return false;
                if (p.Y < 0 || p.Y >= array.GetLength(1)) return false;
                if (a.MatchType == "Shadows" || a.MatchType == "NotShadows") return false;

                return array[p.X, p.Y] == (a.MatchType == "EqualsSelf");
            };
        }

        private static void DrawRawImage(bool[,] array, Control control)
        {
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
                            new Rectangle((int)(x / xf), control.ClientSize.Height - (int)((y+1) / yf), (int)(1 / xf), (int)(1 / yf)));
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

            var pos = GetArrayIndex(_array, pictureBox2, e.X, e.Y);
            _down = !_array[pos.X, pos.Y];

            pictureBox2_MouseMove(sender, e);
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left)) return;
            var pos = GetArrayIndex(_array, pictureBox2, e.X, e.Y);

            if (_array[pos.X, pos.Y] == _down) return;


            _array[pos.X, pos.Y] = _down;
            
            DrawRawImage(_array, pictureBox2);
            DrawFancyImage(_array, pictureBox1);
        }
    }
}
