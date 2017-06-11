using Newtonsoft.Json;

namespace StarboundVisualizer.Components.JsonClasses
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