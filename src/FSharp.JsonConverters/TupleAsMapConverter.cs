using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;

namespace FSharp.JsonConverters
{
    
    public class TupleAsMapConverter : JsonConverterFactory
    {
        private class TupleAsObject<T> : JsonConverter<T>
        {
            private readonly Type[] _tupleTypes = typeof(T).GetGenericArguments();
            private readonly Converter<object[], object> _toTuple =
                FSharpValue.PreComputeTupleConstructor(typeof(T));
            
            private readonly Converter<object, object[]> _fromTuple =
                FSharpValue.PreComputeTupleReader(typeof(T));

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var values = _tupleTypes.Select(p => p.AsDefault()).ToArray();
                var assigned = _tupleTypes.Select(_ => false).ToArray();
                if(reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Error deserialize tuple - must be object");
                reader.Read();
                do
                {
                    
                    var propIndex = int.Parse(reader.GetString().Substring(4)) - 1;
                    values[propIndex] = JsonSerializer.Deserialize(ref reader, _tupleTypes[propIndex], options);
                    assigned[propIndex] = true;
                    reader.Read();
                } while (reader.TokenType != JsonTokenType.EndObject);
                if(assigned.Any(p => !p))
                    throw new JsonException("Error deserialize tuple - not all fields assigned");
                return (T) _toTuple(values);
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var values = _fromTuple(value);
                writer.WriteStartObject();
                for (var i = 0; i < values.Length; i++)
                {
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName($"Item{i+1}") ?? $"item{i+1}");
                    JsonSerializer.Serialize(writer, values[i], _tupleTypes[i], options);
                }
                writer.WriteEndObject();
            }
        }

        private static readonly Type TupleAsObjectType = typeof(TupleAsObject<>); 

        public override bool CanConvert(Type typeToConvert)
            => FSharpType.IsTuple(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter) Activator.CreateInstance(TupleAsObjectType.MakeGenericType(typeToConvert));
    }
}