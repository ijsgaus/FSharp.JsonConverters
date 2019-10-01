using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;

namespace FSharp.JsonConverters
{
    public class FSharpRecordConverter : JsonConverterFactory
    {
        private class RecordConverter<T> : JsonConverter<T>
        {
            private readonly PropertyInfo[] _props = FSharpType.GetRecordFields(typeof(T), FSharpOption<BindingFlags>.None);
            private readonly Converter<object[], object> _toRecord =
                FSharpValue.PreComputeRecordConstructor(typeof(T), FSharpOption<BindingFlags>.None);
            private readonly Converter<object, object[]> _fromRecord =
                FSharpValue.PreComputeRecordReader(typeof(T), FSharpOption<BindingFlags>.None);
            
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    var values = reader.ReadFields(options, _props);
                    return (T) _toRecord(values);
                }
                catch (Exception ex)
                {
                    throw new JsonException($"Error deserialize record {typeof(T).Name}", ex);
                }
                
                
            }

            
            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var values = _fromRecord(value);
                writer.WriteFields(_props, values, options);
            }
        }

        
        
        public override bool CanConvert(Type typeToConvert)
            => FSharpType.IsRecord(typeToConvert, FSharpOption<BindingFlags>.None);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter) Activator.CreateInstance(typeof(RecordConverter<>).MakeGenericType(typeToConvert));
    }
}