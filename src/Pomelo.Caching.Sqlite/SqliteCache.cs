﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Pomelo.Caching.Sqlite
{
    class SqliteCache : IMemoryCache
    {
        private readonly SqliteCacheContext _dbContext;
        private readonly ISqliteCacheSerializer _cacheSerializer;

        public SqliteCache(SqliteCacheContext dbContext, ISqliteCacheSerializer cacheSerializer)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
            _cacheSerializer = cacheSerializer;
        }

        public ICacheEntry CreateEntry(Object key)
        {
            var keyStr = key as String;
            if (keyStr == null)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            return new SqliteCacheEntry(keyStr, _dbContext, _cacheSerializer);
        }

        public void Dispose()
        {
            _dbContext.Database.ExecuteSqlRaw("DELETE FROM CacheItem");
        }

        public void Remove(Object key)
        {
            var cacheItem = _dbContext.CacheItems.Find(key);
            if (cacheItem != null)
            {
                _dbContext.Remove(cacheItem);
            }
        }

        public bool TryGetValue(Object key, out Object? value)
        {
            var cacheItem = _dbContext.CacheItems.Find(key);
            if (cacheItem == null || cacheItem.HasExpired())
            {
                value = null;
                return false;
            }
            if (cacheItem.SlidingExpiration.HasValue)
            {
                cacheItem.UpdateAt = DateTime.Now;
                _dbContext.SaveChanges();
            }

            value = _cacheSerializer.DeserializeObject(cacheItem.Value);
            return true;
        }
    }
}