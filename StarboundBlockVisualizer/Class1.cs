using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StarboundBlockVisualizer
{/*
    public class Material
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public int[] ParticleColor { get; set; }
        public string ItemDrop { get; set; }
        public string Description { get; set; }
        public string Shortdescription { get; set; }
        public string FootstepSound { get; set; }
        public int Health { get; set; }
        public string Category { get; set; }
        public string RenderTemplate { get; set; }
        public RenderParameters RenderParameters { get; set; }
    }

    public class RenderParameters
    {
        public string Texture { get; set; }
        public int Variants { get; set; }
        public bool LightTransparent { get; set; }
        public bool OccludesBelow { get; set; }
        public bool MultiColored { get; set; }
        public int ZLevel { get; set; }
    }

    public class Config
    {
        public Dictionary<string, Piece> Pieces { get; set; }
        public string RepresentativePiece { get; set; }
        public Dictionary<string, Rule> Matches { get; set; }
        [JsonConverter(typeof(MatchListConverter<string, List<Match>>))]
        public List<Dictionary<string, List<Match>>> Matches { get; set; }
    }

    public class Rule
    {
        public RuleEntry[] Entries { get; set; }
    }

    public class RuleEntry
    {
        public string Type;
        public bool Inverse;
    }

    public class MatchListConverter<TKey, TValue> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Console.WriteLine(PrettyTypeName(objectType));

            string s = reader.Path;

            Debug.Assert(reader.TokenType == JsonToken.StartArray);
            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.StartArray);
            reader.Read();
            var result = new List<Dictionary<TKey, TValue>>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                var item = new Dictionary<TKey, TValue>();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    var name = serializer.Deserialize<TKey>(reader);
                    reader.Read();
                    Debug.Assert(reader.TokenType != JsonToken.EndArray);
                    item[name] = serializer.Deserialize<TValue>(reader);
                    reader.Read();
                }
                result.Add(item);
            }
            reader.Read();
            return result;
        }

        public static string PrettyTypeName(Type t)
        {
            if (!t.IsGenericType) return t.Name;

            var prefix = t.Name.Substring(0, t.Name.IndexOf('`'));
            return $"{prefix}<{string.Join(", ", t.GetGenericArguments().Select(PrettyTypeName))}>";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<Dictionary<string, List<Match>>>);
        }
    }

    public class PointConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new Point();
            Debug.Assert(reader.TokenType == JsonToken.StartArray);
            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.Integer);
            result.X = (long)reader.Value;
            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.Integer);
            result.Y = (long)reader.Value;
            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.EndArray);

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Point);
        }
    }*/
}
