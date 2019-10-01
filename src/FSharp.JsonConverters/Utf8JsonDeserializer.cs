using System.Text.Json;

namespace FSharp.JsonConverters
{
    public delegate T Utf8JsonDeserializer<out T>(ref Utf8JsonReader reader, JsonSerializerOptions options);
}