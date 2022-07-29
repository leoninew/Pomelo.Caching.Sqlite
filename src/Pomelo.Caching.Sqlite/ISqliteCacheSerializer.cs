using System;
using System.IO;
using System.Text;

namespace Pomelo.Caching.Sqlite
{
    public interface ISqliteCacheSerializer
    {
        String SerializeObject(Object value);
        Object? DeserializeObject(String value, Type type);
    }
}