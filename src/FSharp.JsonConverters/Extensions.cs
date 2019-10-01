using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.FSharp.Core;

namespace FSharp.JsonConverters
{
    public static class Extensions
    {
        public static bool IsConstructedGeneric(this Type constructed, Type generic)
            => constructed.IsConstructedGenericType && constructed.GetGenericTypeDefinition() == generic;

        public static IEnumerable<T> DeserializeArray<T>(this ref Utf8JsonReader reader, Utf8JsonReaderFunc<T> func)
        {
            var token = reader.TokenType;
            if(reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Json token must be array start token");
            var list = new List<T>();
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                list.Add(func(ref reader));
                reader.Read();
            }

            return list;
        }


        public static IEnumerable<T> DeserializeArray<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
            => reader.DeserializeArray((ref Utf8JsonReader p) => JsonSerializer.Deserialize<T>(ref p, options));
        
        public static IEnumerable<Tuple<string, T>> DeserializeMapAsTuples<T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Json token must be object start token");
            var list = new List<T>();
            reader.Read();
            var map = new List<Tuple<string, T>>();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                map.Add(Tuple.Create(reader.GetString(), JsonSerializer.Deserialize<T>(ref reader, options)));
                reader.Read();
            }
            return map;
        }

        public static void SerializeToArray<T>(this Utf8JsonWriter writer, IEnumerable<T> sequence,
            Action<Utf8JsonWriter, T> write)
        {
            writer.WriteStartArray();
            foreach (var item in sequence)
                write(writer, item);
            writer.WriteEndArray();
        }

        public static void SerializeToArray<T>(this Utf8JsonWriter writer, IEnumerable<T> sequence,
            JsonSerializerOptions options)
            => writer.SerializeToArray(sequence, (w, i) => JsonSerializer.Serialize(w, i, options));


        public static T AsDefaultGeneric<T>() => default;

        public static object AsDefault(this Type type)
        {
            var info = typeof(Extensions)
                .GetMethod("AsDefaultGeneric")!; 
            return info.MakeGenericMethod(type).Invoke(null, new object[0]);
        }

        private static readonly MethodInfo IsNone = typeof(OptionModule).GetMethod("IsNone");
        
        internal static void WriteFields(this Utf8JsonWriter writer, PropertyInfo[] props, object[] values,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            for (var i = 0; i < values.Length; i++)
            {
                if (props[i].PropertyType.IsConstructedGeneric(typeof(FSharpOption<>))
                    && ((bool) IsNone.MakeGenericMethod(props[i].PropertyType.GetGenericArguments()[0])
                        .Invoke(null, new[] {values[i]})))
                    continue;
                var propName = props[i].Name;
                writer.WritePropertyName(
                    options.PropertyNamingPolicy?.ConvertName(propName) ?? propName);
                JsonSerializer.Serialize(writer, values[i], props[i].PropertyType, options);
            }
            writer.WriteEndObject();
        }
        
        internal static object[] ReadFields(this ref Utf8JsonReader reader, JsonSerializerOptions options, PropertyInfo[] props)
        {
            static object DefaultWithOption(Type type)
            {
                if (type.IsConstructedGeneric(typeof(FSharpOption<>)))
                    return
                        // ReSharper disable once PossibleNullReferenceException
                        typeof(FSharpOption<>)
                            .MakeGenericType(type.GetGenericArguments()[0])
                            .GetProperty("None")
                            .GetValue(null);
                    
                return type.AsDefault();
            }
            var names = props
                .Select((p, i) => (p.Name, i))
                .ToDictionary(p => 
                    options.PropertyNameCaseInsensitive 
                        ? p.Name.ToUpper() 
                        : options.PropertyNamingPolicy?.ConvertName(p.Name) ?? p.Name, p => p.i);
            var values = props.Select(p => DefaultWithOption(p.PropertyType)).ToArray();
            var assigned = props.Select(p => p.PropertyType.IsConstructedGeneric(typeof(FSharpOption<>))).ToArray();
            if(reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Error deserialize fields collection - must be object");
            reader.Read();
            do
            {
                    
                var propNameAsIs = reader.GetString();
                var propName = options.PropertyNameCaseInsensitive
                    ? propNameAsIs.ToUpper()
                    : propNameAsIs;
                if(!names.TryGetValue(propName, out var propIndex))
                    throw new JsonException($"Error deserialize  fields collection - unknownProperty '{propNameAsIs}''");
                values[propIndex] = JsonSerializer.Deserialize(ref reader, props[propIndex].PropertyType, options);
                assigned[propIndex] = true;
                reader.Read();
            } while (reader.TokenType != JsonTokenType.EndObject);
            if(assigned.Any(p => !p))
                throw new JsonException("Error deserialize record  fields collection - not all fields assigned");
            return values;
        }
    }
}