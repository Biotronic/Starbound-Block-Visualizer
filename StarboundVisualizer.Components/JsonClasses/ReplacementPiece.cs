using Newtonsoft.Json;

namespace StarboundVisualizer.Components.JsonClasses
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