using System.Drawing;
using Newtonsoft.Json;

namespace StarboundVisualizer.Components.JsonClasses
{
    [JsonConverter(typeof(ArrayToObjectConverter))]
    public struct Point
    {
        public Point(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        public long X { get; set; }

        public long Y { get; set; }

        public Size AsSize => new Size((int)X, (int)Y);

        public static Point operator +(Point a, Point b)
        {
            return new Point { X = a.X + b.X, Y = a.Y + b.Y };
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point { X = a.X - b.X, Y = a.Y - b.Y };
        }

        public static Point operator *(Point a, int b)
        {
            return new Point { X = a.X * b, Y = a.Y * b };
        }

        public Rectangle ToRect(Point size)
        {
            return new Rectangle((int)X, (int)Y, (int)size.X, (int)size.Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}