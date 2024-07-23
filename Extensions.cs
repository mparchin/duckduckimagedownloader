using System.Text.Json;
using System.Text.Json.Serialization;

namespace duckduckimagedownloader
{
    public static class Extensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        public static string ToJson(this object obj) =>
            JsonSerializer.Serialize(obj, _jsonOptions);

    }
}