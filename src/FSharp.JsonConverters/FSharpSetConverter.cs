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
                try
                {
                    return SetModule.OfSeq(reader.DeserializeArray<T>(options));
                }
                catch (Exception ex)
                {
                    throw new JsonException(
                        $"Error when deserialize FSharpSet<{typeof(T).Name}>", ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, FSharpSet<T> value, JsonSerializerOptions options)
                => writer.SerializeToArray(value, options);
        }
        
        private static readonly Type GenericType = typeof(InnerConverter<>);
        
        public override bool CanConvert(Type typeToConvert)
            =>  typeToConvert.IsConstructedGeneric(typeof(FSharpSet<>));

        private JsonConverter MakeConverter(Type type)
            => (JsonConverter) Activator.CreateInstance(GenericType.MakeGenericType(type.GenericTypeArguments[0]));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter) Activator.CreateInstance(
                GenericType.MakeGenericType(typeToConvert.GenericTypeArguments[0]));
    }
}