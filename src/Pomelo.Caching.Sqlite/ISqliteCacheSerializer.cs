using System;

namespace Pomelo.Caching.Sqlite
{
    public interface ISqliteCacheSerializer
    {
        String SerializeObject(Object value);
        Object? DeserializeObject(String value);
    }
}