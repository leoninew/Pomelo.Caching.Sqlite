using System;
using Microsoft.Extensions.Caching.Memory;

namespace Pomelo.Caching.Sqlite
{
    public static class IMemoryCacheExtensions
    {
        public static IMemoryCache WithPrefix(this IMemoryCache memoryCache, String prefix)
        {
            if (memoryCache is SqliteCache sqliteCache)
            {
                return new SqliteCache(sqliteCache, prefix);
            }
            return memoryCache;
        }
    }
}