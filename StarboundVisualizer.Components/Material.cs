using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarboundVisualizer.Components.JsonClasses;

namespace StarboundVisualizer.Components
{
    public class Material
    {
        public Dictionary<string, Piece> Pieces { get; }

        public List<Match> Matches { get; }

        public string TextureSource { get; set; }
        public string MaterialName { get; set; }
        public string Location { get; set; }
        public int Variants { get; set; }

        public Material(string path)
        {
            var fixPath = PathFinder.FindFile(path);

            var material = ParseFile(path);
            var renderTemplatePath = (string)material.SelectToken("$.renderTemplate");
            var renderTemplate = ParseFile(PathFinder.FindFile(renderTemplatePath, Path.GetDirectoryName(fixPath)));
            TextureSource = PathFinder.FindFile((string)material.SelectToken("$.renderParameters.texture"), Path.GetDirectoryName(fixPath));
            MaterialName = (string)material.SelectToken("$.materialName");
            Variants = (int)material.SelectToken("$.renderParameters.variants");

            var piecesToken = renderTemplate.SelectToken("$.pieces");

            Pieces = piecesToken.ToObject<Dictionary<string, Piece>>();
            Matches = renderTemplate.SelectToken("$.matches[0][1]").ToObject<List<Match>>();

            Location = fixPath;
        }

        private static JObject ParseFile(string filename)
        {
            using (var fs = File.OpenRead(filename))
            using (var sr = new StreamReader(fs))
            using (var jr = new JsonTextReader(sr))
                return JObject.Load(jr);
        }
    }
}