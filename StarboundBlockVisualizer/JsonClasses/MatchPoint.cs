using Newtonsoft.Json;

namespace StarboundBlockVisualizer.JsonClasses
{
    [JsonConverter(typeof(ArrayToObjectConverter))]
    public class MatchPoint
    {
        public Point Position { get; set; }
        public string MatchType { get; set; }

        public override string ToString()
        {
            return $"Match({Position}, {MatchType})";
        }
    }
}