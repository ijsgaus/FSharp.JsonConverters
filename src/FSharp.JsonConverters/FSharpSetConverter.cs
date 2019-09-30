using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Collections;

namespace FSharp.JsonConverters
{
    public class FSharpSetConverter : JsonConverterFactory
    {
        private class InnerConverter<T> : JsonConverter<FSharpSet<T>>
        {
            public override FSharpSet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException("Must be a array");
                var lst = new List<T>();
                reader.Read();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    lst.Add(JsonSerializer.Deserialize<T>(ref reader, options));
                    reader.Read();
                }
                return SetModule.OfSeq(lst);
            }

            public override void Write(Utf8JsonWriter writer, FSharpSet<T> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                foreach (var item in value)
                    JsonSerializer.Serialize(writer, item, options);
                writer.WriteEndArray();
            }
        }
        
        private static readonly Type GenericType = typeof(InnerConverter<>);
        private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new ConcurrentDictionary<Type, JsonConverter>();
        
        public override bool CanConvert(Type typeToConvert)
            =>  typeToConvert.IsGenericType && typeToConvert.IsConstructedGenericType &&
                typeToConvert.GetGenericTypeDefinition() == typeof(FSharpSet<>);

        private JsonConverter MakeConverter(Type type)
            => (JsonConverter) Activator.CreateInstance(GenericType.MakeGenericType(type.GenericTypeArguments[0]));
        
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => Cache.GetOrAdd(typeToConvert, MakeConverter);
    }
}