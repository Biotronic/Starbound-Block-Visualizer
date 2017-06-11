using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StarboundVisualizer.Components.JsonClasses
{
    public class ArrayToObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = Activator.CreateInstance(objectType);
            var array = JArray.Load(reader);
            var n = 0;
            foreach (var property in objectType.GetProperties())
            {
                if (property.SetMethod == null) continue;


                var value = array.SelectToken($"$[{n}]");
                property.SetValue(result, value.ToObject(property.PropertyType));
                n++;
            }
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
