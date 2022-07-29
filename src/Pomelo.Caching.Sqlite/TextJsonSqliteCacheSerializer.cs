using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using Pomelo.Caching.Sqlite;


namespace Pomelo.Caching.Sqlite
{
    public class TextJsonSqliteCacheSerializer : ISqliteCacheSerializer
    {
        public static readonly JsonSerializerOptions Default;
        public static readonly JsonSerializerOptions CamelCase;

        static TextJsonSqliteCacheSerializer()
        {
            Default = new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
#if !NETSTANDARD2_0
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#endif
            };
            Default.Converters.Add(new JsonStringEnumConverter());

            CamelCase = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                // Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
#if !NETSTANDARD2_0
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#endif
            };
            CamelCase.Converters.Add(new JsonStringEnumConverter());
        }

        public String SerializeObject(Object value)
        {
            return JsonSerializer.Serialize(value, Default);
        }

        public Object? DeserializeObject(String value, Type type)
        {
            return JsonSerializer.Deserialize(value, type, Default);
        }
    }
}