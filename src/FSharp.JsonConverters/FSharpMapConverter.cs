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
        private readonly bool _useArrayTuple;

        public FSharpMapConverter(bool useArrayTuple = false)
        {
            _useArrayTuple = useArrayTuple;
        }
        
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
                   writer.WritePropertyName(options.DictionaryKeyPolicy.ConvertName(item.Key)); 
                   JsonSerializer.Serialize(writer, item.Value, options);
                }
                writer.WriteEndObject();
            }
        }
        
        private class InnerConverter<TKey, TValue> : JsonConverter<FSharpMap<TKey, TValue>>
        {
            private readonly bool _useArrayTuple;

            public InnerConverter(bool useArrayTuple)
            {
                _useArrayTuple = useArrayTuple;
            }

            public override FSharpMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (_useArrayTuple)
                {
                    if (reader.TokenType != JsonTokenType.StartArray)
                        throw new JsonException("Must be a array");
                    var map = MapModule.Empty<TKey, TValue>();
                    reader.Read();
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException("Must be a array");
                        reader.Read();
                        var key = JsonSerializer.Deserialize<TKey>(ref reader, options);
                        reader.Read();
                        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                        reader.Read();
                        reader.Read();
                        
                        map = map.Add(key, value);
                    }
                    return map;
                }
                else
                {
                    if (reader.TokenType != JsonTokenType.StartArray)
                        throw new JsonException("Must be a array");
                    var map = MapModule.Empty<TKey, TValue>();
                    reader.Read();
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType != JsonTokenType.StartObject)
                            throw new JsonException("Must be a object");
                        TKey key = default;
                        TValue value = default;
                        reader.Read();
                        var prop = reader.GetString(); 
                        reader.Read();
                        switch (prop.ToLower())
                        {
                            case "item1":
                                key = JsonSerializer.Deserialize<TKey>(ref reader, options);
                                break;
                            case "item2":
                                value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                                break;
                            default:
                                throw new JsonException("Invalid property name");
                        }

                        reader.Read();
                        prop = reader.GetString();
                        reader.Read();
                        switch (prop.ToLower())
                        {
                            case "item1":
                                key = JsonSerializer.Deserialize<TKey>(ref reader, options);
                                break;
                            case "item2":
                                value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                                break;
                            default:
                                throw new JsonException("Invalid property name");
                        }
                        reader.Read();
                        map = map.Add(key, value);
                        reader.Read();
                    }
                    return map;
                }
                
            }

            public override void Write(Utf8JsonWriter writer, FSharpMap<TKey, TValue> value, JsonSerializerOptions options)
            {
                if (_useArrayTuple)
                {
                    writer.WriteStartArray();
                    foreach (var item in value)
                    {
                        writer.WriteStartArray();
                        JsonSerializer.Serialize(writer, item.Key, options);
                        JsonSerializer.Serialize(writer, item.Value, options);
                        writer.WriteEndArray();
                    }
                    writer.WriteEndArray();
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var item in value)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName(options.PropertyNamingPolicy.ConvertName("Item1"));
                        JsonSerializer.Serialize(writer, item.Key, options);
                        writer.WritePropertyName(options.PropertyNamingPolicy.ConvertName("Item2"));
                        JsonSerializer.Serialize(writer, item.Value, options);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
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
                    : (JsonConverter) Activator.CreateInstance(GenericType.MakeGenericType(type.GenericTypeArguments[0], type.GenericTypeArguments[1]), new object[] {_useArrayTuple});
        
        
        
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => Cache.GetOrAdd(typeToConvert, MakeConverter);
    }
}