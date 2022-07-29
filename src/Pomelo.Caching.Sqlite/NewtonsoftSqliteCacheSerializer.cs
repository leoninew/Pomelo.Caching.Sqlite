using System;
using Newtonsoft.Json;

namespace Pomelo.Caching.Sqlite
{
    public class NewtonsoftSqliteCacheSerializer : ISqliteCacheSerializer
    {
        static readonly JsonSerializerSettings Default = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public String SerializeObject(Object value)
        {
            return JsonConvert.SerializeObject(value, Default);
        }

        public Object? DeserializeObject(String value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type, Default);
        }
    }
}