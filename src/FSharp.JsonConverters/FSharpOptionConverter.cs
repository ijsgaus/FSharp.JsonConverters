using System;
using System.Collections.Concurrent;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Core;

namespace FSharp.JsonConverters
{
    public class OptionConverter :JsonConverterFactory
    {
        private class InnerOptionConverter<T> : JsonConverter<FSharpOption<T>>
        {
            public override FSharpOption<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return FSharpOption<T>.None;
                return FSharpOption<T>.Some(JsonSerializer.Deserialize<T>(ref reader, options));
            }

            public override void Write(Utf8JsonWriter writer, FSharpOption<T> value, JsonSerializerOptions options)
            {
                if(OptionModule.IsNone(value))
                    writer.WriteNullValue();
                else
                {
                    JsonSerializer.Serialize(writer, value.Value, options);
                }
            }

            
            
            
        }

        private static readonly Type GenericType = typeof(InnerOptionConverter<>);
        private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new ConcurrentDictionary<Type, JsonConverter>();
        
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType && typeToConvert.IsConstructedGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(FSharpOption<>);

        private JsonConverter MakeConverter(Type type)
            => (JsonConverter) Activator.CreateInstance(GenericType.MakeGenericType(type.GenericTypeArguments[0]));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => Cache.GetOrAdd(typeToConvert, MakeConverter);
    }
}