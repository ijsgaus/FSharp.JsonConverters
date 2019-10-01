using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;

namespace FSharp.JsonConverters
{
    public class FSharpUnionConverter : JsonConverterFactory
    {
        private class ErasedUnionConverter<T> : JsonConverter<T>
        {
            private readonly PropertyInfo[] _props;
            private readonly bool _isSingleValue;
            private bool _isTupleValue;
            private Converter<object[], object> _toCase;
            private Converter<object, object[]> _fromCase;
            private Converter<object[], object> _toTuple;
            private Converter<object, object[]> _fromTuple;
            private Type _tupleType;


            public ErasedUnionConverter(UnionCaseInfo info, PropertyInfo[] props)
            {
                _props = props;
                _isSingleValue = IsSingleValueCase(props);
                _isTupleValue = IsTupleCase(props);
                _toCase = FSharpValue.PreComputeUnionConstructor(info, FSharpOption<BindingFlags>.None);
                _fromCase = FSharpValue.PreComputeUnionReader(info, FSharpOption<BindingFlags>.None);
                if (_isTupleValue)
                {
                    _tupleType = MakeTupleType(props);
                    _toTuple = FSharpValue.PreComputeTupleConstructor(_tupleType);
                    _fromTuple = FSharpValue.PreComputeTupleReader(_tupleType);
                }
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    if (_isSingleValue)
                        return (T) _toCase(new[]
                            {JsonSerializer.Deserialize(ref reader, _props[0].PropertyType, options)});
                    if (_isTupleValue)
                    {
                        var tuple = JsonSerializer.Deserialize(ref reader, _tupleType, options);
                        return (T) _toCase(_fromTuple(tuple));
                    }
                    return (T) _toCase(reader.ReadFields(options, _props));
                }
                catch (Exception ex)
                {
                    throw new JsonException($"Error deserializing {typeof(T)}", ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var values = _fromCase(value);
                if (_isSingleValue)
                    JsonSerializer.Serialize(writer, values[0], _props[0].PropertyType, options);
                else if (_isTupleValue)
                {
                    var tuple = _toTuple(values);
                    JsonSerializer.Serialize(writer, tuple, _tupleType, options);
                }
                else
                {
                    writer.WriteFields(_props, values, options);
                }
            }
        }

        private class StringUnionConverter<T> : JsonConverter<T>
        {
            private readonly (string, Converter<object[], object>)[] _readers;

            private readonly Converter<object, int> _tagReader;
            private readonly Dictionary<int, string> _nameByTag;

            public StringUnionConverter(UnionCaseInfo[] cases)
            {
                _readers = cases
                    .Select(p => (p.Name, (Converter<object[], object>) FSharpValue.PreComputeUnionConstructor(p, FSharpOption<BindingFlags>.None)))
                    .ToArray();
                _tagReader = FSharpValue.PreComputeUnionTagReader(typeof(T), FSharpOption<BindingFlags>.None);
                _nameByTag =
                    cases.ToDictionary(p => p.Tag, p => p.Name);
                //_cases = cases;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var str = reader.GetString();
                if (options.PropertyNameCaseInsensitive)
                {
                    var strUpper = str.ToUpper();
                    var cvt = _readers.Where(p => p.Item1.ToUpper() == strUpper).Select(p => p.Item2).FirstOrDefault();
                    if(cvt == null)
                        throw new JsonException($"Unknown union case '{str}' for union {typeof(T)}");
                    return (T) cvt(new object[0]);
                }
                else
                {
                    var cvt =
                        _readers.Where(p => (options.PropertyNamingPolicy?.ConvertName(p.Item1) ?? p.Item1) == str)
                            .Select(p => p.Item2).FirstOrDefault();
                    if(cvt == null)
                        throw new JsonException($"Unknown union case '{str}' for union {typeof(T)}");
                    return (T) cvt(new object[0]);
                }

            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var tag = _tagReader(value);
                var name = _nameByTag[tag];
                writer.WriteStringValue(options.PropertyNamingPolicy?.ConvertName(name) ?? name);
            }
        }


        private class UnionConverter<T> : JsonConverter<T>
        {
            private class CaseData
            {
                public CaseData(UnionCaseInfo info, PropertyInfo[] props)
                {
                    Name = info.Name;
                    Props = props;
                    ToCase = FSharpValue.PreComputeUnionConstructor(info, FSharpOption<BindingFlags>.None);
                    FromCase = FSharpValue.PreComputeUnionReader(info, FSharpOption<BindingFlags>.None);
                    IsEmpty = props.Length == 0;
                    IsSingleCase = IsSingleValueCase(props);
                    if (IsTupleCase(props))
                    {
                        TupleType = MakeTupleType(props);
                        ToTuple = FSharpValue.PreComputeTupleConstructor(TupleType);
                        FromTuple = FSharpValue.PreComputeTupleReader(TupleType);
                    }

                }

                public readonly string Name;
                public PropertyInfo[] Props;
                public readonly Converter<object[], object> ToCase;
                public readonly Converter<object, object[]> FromCase;
                public readonly bool IsEmpty;
                public readonly bool IsSingleCase;
                public readonly Type TupleType;
                public readonly Converter<object[], object> ToTuple;
                public readonly Converter<object, object[]> FromTuple;
            }
            
            private readonly Dictionary<int, CaseData> _caseByTag;
            private readonly Converter<object, int> _tagReader;
            
            

            public UnionConverter((UnionCaseInfo, PropertyInfo[])[] cases)
            {
                _tagReader = FSharpValue.PreComputeUnionTagReader(typeof(T), FSharpOption<BindingFlags>.None);
                _caseByTag =
                    cases.ToDictionary(
                        p => p.Item1.Tag,
                        p => new CaseData(p.Item1, p.Item2));
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                CaseData FindCaseData(string caseName)
                {
                    Func<string,bool> CompareName()
                    {
                        if (options.PropertyNameCaseInsensitive)
                        {
                            var nameUp = caseName.ToUpper();
                            return nm => nm.ToUpper() == nameUp;
                        }

                        return nm => (options.PropertyNamingPolicy?.ConvertName(nm) ?? nm) == caseName;
                    }

                    var compare = CompareName();

                    var res =  _caseByTag.Values.FirstOrDefault(p => compare(p.Name));
                    if(res == null)
                        throw new JsonException($"Cannot deserialize {typeof(T)} - unknown case '{caseName}'");
                    return res;
                }
                
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException($"Cannot deserialize {typeof(T)} - must be json object");
                reader.Read();
                var propName = reader.GetString();
                if(propName != "$type")
                    throw new JsonException($"Cannot deserialize {typeof(T)} - first property must be '$type'");
                reader.Read();
                var propValue = reader.GetString();
                var data = FindCaseData(propValue);
                T result;
                if (data.IsEmpty)
                {
                    result = (T) data.ToCase(new object[0]);
                }
                else
                {
                    reader.Read();
                    var bodyName = reader.GetString();
                    if (bodyName != "$payload")
                        throw new JsonException($"Cannot deserialize {typeof(T)} - second property must be '$payload'");
                    reader.Read();

                    if (data.IsSingleCase)
                        result =
                            (T) data.ToCase(new[]
                            {
                                JsonSerializer.Deserialize(ref reader, data.Props[0].PropertyType, options)
                            });
                    else if (data.TupleType != null)
                    {
                        var tuple = JsonSerializer.Deserialize(ref reader, data.TupleType, options);
                        result = (T) data.ToCase(data.FromTuple(tuple));
                    }
                    else
                    {
                        var values = reader.ReadFields(options, data.Props);
                        result = (T) data.ToCase(values);
                    }
                }

                reader.Read();
                return result;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var data = _caseByTag[_tagReader(value)];
                 
                writer.WriteStartObject();
                writer.WriteString("$type", options.PropertyNamingPolicy?.ConvertName(data.Name) ?? data.Name);
                if (!data.IsEmpty)
                {
                    var values = data.FromCase(value);
                    writer.WritePropertyName("$payload");
                    if (data.IsSingleCase)
                    {
                        JsonSerializer.Serialize(writer, values[0], data.Props[0].PropertyType, options);
                    }
                    else
                    if(data.TupleType != null)
                    {
                        var tuple = data.ToTuple(values);
                        JsonSerializer.Serialize(writer, tuple, data.TupleType, options);
                    }
                    else
                    {
                        writer.WriteFields(data.Props, values, options);
                    }
                }
                writer.WriteEndObject();
            }
        }
        
        
        
        private static bool IsErasedUnion((UnionCaseInfo, PropertyInfo[])[] cases)
            => cases.Length == 1 && cases[0].Item2.Length != 0;
        
        private static bool IsStringUnion((UnionCaseInfo, PropertyInfo[])[] cases)
            => cases.All(p => p.Item2.Length == 0);

        private static bool IsSingleValueCase(PropertyInfo[] info)
            => info.Length == 1;
        
        private static bool IsTupleCase(PropertyInfo[] info)
            => info.Length > 1 && info.All(p => p.Name.StartsWith("Item"));
        
                
                    
        
        public override bool CanConvert(Type typeToConvert)
            => FSharpType.IsUnion(typeToConvert, FSharpOption<BindingFlags>.None);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var desc = FSharpType.GetUnionCases(typeToConvert, FSharpOption<BindingFlags>.None)
                .Select(p => (p, p.GetFields())).ToArray();
            if(IsErasedUnion(desc))
                return (JsonConverter) Activator.CreateInstance(
                    typeof(ErasedUnionConverter<>).MakeGenericType(typeToConvert),
                    new object[] {desc[0].Item1, desc[0].Item2});
            if(IsStringUnion(desc))
                return (JsonConverter) Activator.CreateInstance(
                    typeof(StringUnionConverter<>).MakeGenericType(typeToConvert),
                    new object[] {desc.Select(p => p.Item1).ToArray() });
            return (JsonConverter) Activator.CreateInstance(
                typeof(UnionConverter<>).MakeGenericType(typeToConvert),
                new object[] {desc});
        }

        internal static Type MakeTupleType(PropertyInfo[] props)
        {
            return props.Length switch
            {
                2 => typeof(ValueTuple<,>).MakeGenericType(Enumerable.Select<PropertyInfo, Type>(props, p => p.PropertyType).ToArray()),
                3 => typeof(ValueTuple<,,>).MakeGenericType(Enumerable.Select<PropertyInfo, Type>(props, p => p.PropertyType).ToArray()),
                4 => typeof(ValueTuple<,,,>).MakeGenericType(Enumerable.Select<PropertyInfo, Type>(props, p => p.PropertyType).ToArray()),
                5 => typeof(ValueTuple<,,,,>).MakeGenericType(Enumerable.Select<PropertyInfo, Type>(props, p => p.PropertyType).ToArray()),
                6 => typeof(ValueTuple<,,,,,>).MakeGenericType(Enumerable.Select<PropertyInfo, Type>(props, p => p.PropertyType).ToArray()),
                7 => typeof(ValueTuple<,,,,,,>).MakeGenericType(Enumerable.Select<PropertyInfo, Type>(props, p => p.PropertyType).ToArray()),
                _ => throw new JsonException("Unsupported tuple typed union case")
            };
        }
    }
}