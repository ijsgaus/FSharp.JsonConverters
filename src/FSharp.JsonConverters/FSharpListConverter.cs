using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Collections;

namespace FSharp.JsonConverters
{
    public class FSharpListConverter : JsonConverterFactory
    {
        private class InnerConverter<T> : JsonConverter<FSharpList<T>>
        {
            public override FSharpList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    return ListModule.OfSeq(reader.DeserializeArray<T>(options));
                }
                catch (Exception ex)
                {
                    throw new JsonException(
                        $"Error when deserialize FSharpList<{typeof(T).Name}>", ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, FSharpList<T> value, JsonSerializerOptions options)
                => writer.SerializeToArray(value, options);

        }
        
        private static readonly Type GenericType = typeof(InnerConverter<>);
        public override bool CanConvert(Type typeToConvert)
            =>  typeToConvert.IsConstructedGeneric(typeof(FSharpList<>));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter) Activator.CreateInstance(GenericType.MakeGenericType(typeToConvert.GenericTypeArguments[0]));
    }
}