using System;
using System.Collections.Concurrent;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Core;

namespace FSharp.JsonConverters
{
    public class FSharpOptionConverter :JsonConverterFactory
    {
        private class InnerConverter<T> : JsonConverter<FSharpOption<T>>
        {
            public override FSharpOption<T> Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options) 
                    =>
                    reader.TokenType == JsonTokenType.Null
                        ? FSharpOption<T>.None
                        : FSharpOption<T>.Some(JsonSerializer.Deserialize<T>(ref reader, options));

            public override void Write(Utf8JsonWriter writer, FSharpOption<T> value, JsonSerializerOptions options)
            {
                if(OptionModule.IsNone(value))
                    writer.WriteNullValue();
                else
                    JsonSerializer.Serialize(writer, value.Value, options);
            }
        }

        private static readonly Type GenericType = typeof(InnerConverter<>);

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsConstructedGeneric(typeof(FSharpOption<>));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter) Activator.CreateInstance(GenericType.MakeGenericType(typeToConvert.GenericTypeArguments[0]));
    }
}