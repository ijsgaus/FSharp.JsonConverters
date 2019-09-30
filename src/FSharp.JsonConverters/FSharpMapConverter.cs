using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Collections;

namespace FSharp.JsonConverters
{
    public class FSharpMapConverter : JsonConverterFactory
    {
        private class InnerStringConverter<T> : JsonConverter<FSharpMap<string, T>>
        {
            public override FSharpMap<string, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Must be a object");
                var map = MapModule.Empty<string, T>();
                reader.Read();
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    map = map.Add(reader.GetString(), JsonSerializer.Deserialize<T>(ref reader, options));
                    reader.Read();
                }
                return map;
            }

            public override void Write(Utf8JsonWriter writer, FSharpMap<string, T> value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                foreach (var item in value)
                {
                   writer.WritePropertyName(item.Key); 
                   JsonSerializer.Serialize(writer, item.Value, options);
                }
                writer.WriteEndObject();
            }
        }
        
        private class InnerConverter<TKey, TValue> : JsonConverter<FSharpMap<TKey, TValue>>
        {
            public override FSharpMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var pairs = JsonSerializer.Deserialize< KeyValuePair<TKey, TValue>[]>(ref reader, options);
                return MapModule.OfSeq(pairs.Select(t => Tuple.Create(t.Key, t.Value)));
            }

            public override void Write(Utf8JsonWriter writer, FSharpMap<TKey, TValue> value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value.ToList(), options);
            }
        }
        
        private static readonly Type GenericStringType = typeof(InnerStringConverter<>);
        private static readonly Type GenericType = typeof(InnerConverter<,>);
        
        private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new ConcurrentDictionary<Type, JsonConverter>();
        
        public override bool CanConvert(Type typeToConvert)
            =>  typeToConvert.IsGenericType && typeToConvert.IsConstructedGenericType &&
                typeToConvert.GetGenericTypeDefinition() == typeof(FSharpMap<,>);

        private JsonConverter MakeConverter(Type type)
            =>
                type.GenericTypeArguments[0] == typeof(string)
                    ? (JsonConverter) Activator.CreateInstance(
                        GenericStringType.MakeGenericType(type.GenericTypeArguments[1]))
                    : (JsonConverter) Activator.CreateInstance(GenericType.MakeGenericType(type.GenericTypeArguments[0], type.GenericTypeArguments[1]));
        
        
        
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => Cache.GetOrAdd(typeToConvert, MakeConverter);
    }
}