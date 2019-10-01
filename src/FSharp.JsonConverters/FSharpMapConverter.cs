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
        public FSharpMapConverter()
        {
            
        }
        
        private class JsonMapConverter<T> : JsonConverter<FSharpMap<string, T>>
        {
            public override FSharpMap<string, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {    
                try
                {
                    return MapModule.OfSeq(reader.DeserializeMapAsTuples<T>(options));
                }
                catch (Exception ex)
                {
                    throw new JsonException(
                        $"Error when deserialize FSharpMap<string, {typeof(T).Name}>", ex);
                }
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
        
        private class JsonArrayOfTuples<TKey, TValue> : JsonConverter<FSharpMap<TKey, TValue>>
        {
            public override FSharpMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options)
            {
                try
                {
                    return MapModule.OfSeq(reader.DeserializeArray<Tuple<TKey, TValue>>(options));
                }
                catch (Exception ex)
                {
                    throw new JsonException(
                        $"Error when deserialize FSharpMap<{typeof(TKey).Name},{typeof(TValue).Name}>", ex);
                }
            }


            public override void Write(Utf8JsonWriter writer, FSharpMap<TKey, TValue> value,
                JsonSerializerOptions options)
                => writer.SerializeToArray(MapModule.ToSeq(value), options);
        }
        
        



        private static readonly Type JsonMapConverterType = typeof(JsonMapConverter<>);
        private static readonly Type JsonArrayOfTuplesType = typeof(JsonArrayOfTuples<,>);
        
        public override bool CanConvert(Type typeToConvert)
            =>  typeToConvert.IsConstructedGeneric(typeof(FSharpMap<,>));

        private JsonConverter MakeConverter(Type type)
            =>
                type.GenericTypeArguments[0] == typeof(string)
                    ? (JsonConverter) Activator.CreateInstance(
                        JsonMapConverterType.MakeGenericType(type.GenericTypeArguments[1]))
                    : (JsonConverter) Activator.CreateInstance(
                            JsonArrayOfTuplesType.MakeGenericType(type.GenericTypeArguments[0],
                                type.GenericTypeArguments[1]));
        
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => MakeConverter(typeToConvert);
    }
}