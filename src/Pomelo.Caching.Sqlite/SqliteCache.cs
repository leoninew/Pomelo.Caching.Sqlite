using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Pomelo.Caching.Sqlite
{
    class SqliteCache : IMemoryCache
    {
        private readonly SqliteCacheContext _dbContext;
        private readonly ISqliteCacheSerializer _cacheSerializer;
        private readonly String? _prefix;

        public SqliteCache(SqliteCacheContext dbContext, ISqliteCacheSerializer cacheSerializer)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
            _cacheSerializer = cacheSerializer;
        }

        public SqliteCache(SqliteCache sqliteCache, String? prefix = null)
        {
            _dbContext = sqliteCache._dbContext;
            _cacheSerializer = sqliteCache._cacheSerializer;
            _prefix = prefix;
        }

        public ICacheEntry CreateEntry(Object key)
        {
            var keyStr = key as String;
            if (keyStr == null)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            return new SqliteCacheEntry(_prefix + keyStr, _dbContext, _cacheSerializer);
        }

        public void Dispose()
        {
            _dbContext.Database.ExecuteSqlRaw("DELETE FROM CacheItem");
        }

        public void Remove(Object key)
        {
            var cacheItem = _dbContext.CacheItems.Find(_prefix + key);
            if (cacheItem != null)
            {
                _dbContext.Remove(cacheItem);
                _dbContext.SaveChanges();
            }
        }

        public bool TryGetValue(Object key, out Object? value)
        {
            var cacheItem = _dbContext.CacheItems.Find(_prefix + key);
            if (cacheItem == null || cacheItem.Value == null || cacheItem.Type == null || cacheItem.HasExpired())
            {
                value = null;
                return false;
            }
            if (cacheItem.SlidingExpiration.HasValue)
            {
                cacheItem.UpdateAt = DateTime.Now;
                _dbContext.SaveChanges();
            }

            var type = Type.GetType(cacheItem.Type);
            if (type == null)
            {
                value = null;
                return false;
            }

            value = _cacheSerializer.DeserializeObject(cacheItem.Value, type);
            return true;
        }
    }
}