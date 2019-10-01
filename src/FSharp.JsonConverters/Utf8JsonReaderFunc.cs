using System.Text.Json;

namespace FSharp.JsonConverters
{
    public delegate T Utf8JsonReaderFunc<out T>(ref Utf8JsonReader reader);
}