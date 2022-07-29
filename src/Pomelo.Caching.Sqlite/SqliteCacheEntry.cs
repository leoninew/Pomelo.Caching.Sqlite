using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;


namespace Pomelo.Caching.Sqlite
{
    class SqliteCacheEntry : ICacheEntry
    {
        private readonly SqliteCacheItem _cacheItem;
        private readonly SqliteCacheContext _dbContext;
        private readonly ISqliteCacheSerializer _cacheSerializer;
        private EntityState? _entityState;

        public SqliteCacheEntry(String cacheKey, SqliteCacheContext dbContext, ISqliteCacheSerializer cacheSerializer)
        {
            _dbContext = dbContext;
            var cacheItem = _dbContext.CacheItems.Find(cacheKey);
            if (cacheItem == null)
            {
                _cacheItem = new SqliteCacheItem
                {
                    Key = cacheKey,
                    CreateAt = DateTime.Now
                };
                _entityState = EntityState.Added;
            }
            else
            {
                _cacheItem = cacheItem;
                _cacheItem.CreateAt = DateTime.Now;
                _cacheItem.UpdateAt = null;
            }
            _cacheSerializer = cacheSerializer;
        }

        public Object Key => _cacheItem.Key;

        public Object Value
        {
            get { return _cacheItem.Value; }
            set
            {
                var json = _cacheSerializer.SerializeObject(value);
                if (json != _cacheItem.Value)
                {
                    _cacheItem.Value = json;
                }
            }
        }

        public DateTimeOffset? AbsoluteExpiration
        {
            get { return _cacheItem.AbsoluteExpiration; }
            set
            {
                if (value != _cacheItem.AbsoluteExpiration)
                {
                    _cacheItem.AbsoluteExpiration = value;
                }
            }
        }

        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get { return _cacheItem.AbsoluteExpirationRelativeToNow; }
            set
            {
                if (value != _cacheItem.AbsoluteExpirationRelativeToNow)
                {
                    _cacheItem.AbsoluteExpirationRelativeToNow = value;
                }
            }
        }

        public TimeSpan? SlidingExpiration
        {
            get { return _cacheItem.SlidingExpiration; }
            set
            {
                if (value != _cacheItem.SlidingExpiration)
                {
                    _cacheItem.SlidingExpiration = value;
                }
            }
        }

        public IList<IChangeToken> ExpirationTokens => new List<IChangeToken>();

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => new List<PostEvictionCallbackRegistration>();

        public CacheItemPriority Priority
        {
            get { return _cacheItem.Priority; }
            set
            {
                if (value != _cacheItem.Priority)
                {
                    _cacheItem.Priority = value;
                }
            }
        }

        public long? Size
        {
            get { return _cacheItem.Size; }
            set
            {
                if (value != _cacheItem.Size)
                {
                    _cacheItem.Size = value;
                }
            }
        }

        public void Dispose()
        {
            if (_entityState.HasValue && _entityState == EntityState.Added)
            {
                _dbContext.CacheItems.Add(_cacheItem);
            }
            _dbContext.SaveChanges();
        }
    }
}