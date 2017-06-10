using Newtonsoft.Json;

namespace StarboundBlockVisualizer.JsonClasses
{
    [JsonConverter(typeof(ArrayToObjectConverter))]
    public class ReplacementPiece
    {
        public string Piece { get; set; }
        public Point Position { get; set; }

        public override string ToString()
        {
            return $"Match({Position}, {Piece})";
        }
    }
}