using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Reflection;

namespace FSharp.JsonConverters
{
    public class TupleAsArrayConverter : JsonConverterFactory
    {
        private class TupleAsArray<T> : JsonConverter<T>
        {
            private readonly Type[] _tupleTypes = typeof(T).GetGenericArguments();
            private readonly Converter<object[], object> _toTuple =
                FSharpValue.PreComputeTupleConstructor(typeof(T));
            
            private readonly Converter<object, object[]> _fromTuple =
                FSharpValue.PreComputeTupleReader(typeof(T));

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var values = _tupleTypes.Select(p => Extensions.AsDefault(p)).ToArray();
                if(reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException("Error deserialize tuple - must be array");
                reader.Read();
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = JsonSerializer.Deserialize(ref reader, _tupleTypes[i], options);
                    reader.Read();
                }
                return (T) _toTuple(values);
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var values = _fromTuple(value);
                writer.WriteStartArray();
                for (var i = 0; i < values.Length; i++)
                {
                    JsonSerializer.Serialize(writer, values[i], _tupleTypes[i], options);
                }
                writer.WriteEndArray();
            }
        }

        private static readonly Type TupleAsObjectType = typeof(TupleAsArray<>); 

        public override bool CanConvert(Type typeToConvert)
            => FSharpType.IsTuple(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter) Activator.CreateInstance(TupleAsObjectType.MakeGenericType(typeToConvert));
    }
}