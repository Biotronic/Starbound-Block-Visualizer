using System.Collections.Generic;
using System.Linq;

namespace StarboundVisualizer.Components.JsonClasses
{
    public class Match
    {
        public List<MatchPoint> MatchAllPoints { get; set; }
        public List<ReplacementPiece> Pieces { get; set; }
        public List<Match> SubMatches { get; set; }

        public Match()
        {
            MatchAllPoints = new List<MatchPoint>();
            Pieces = new List<ReplacementPiece>();
            SubMatches = new List<Match>();
        }

        public override string ToString()
        {
            return $"Match([{string.Join(", ", MatchAllPoints.Select(a => a.ToString()))}], " +
                   $"[{string.Join(", ", Pieces.Select(a => a.ToString()))}], " +
                   $"[{string.Join(", ", SubMatches.Select(a => a.ToString()))}])";
        }
    }
}