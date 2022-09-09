using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;


namespace Pomelo.Caching.Sqlite
{
    public interface ISqliteCacheEntry : ICacheEntry
    {
        public DateTime CreateAt { get; }
        public DateTime? UpdateAt { get; }
    }

    class SqliteCacheEntry : ICacheEntry, ISqliteCacheEntry
    {
        private readonly SqliteCacheItem _cacheItem;
        private readonly SqliteCacheContext _dbContext;
        private readonly ISqliteCacheSerializer _cacheSerializer;
        private EntityState? _entityState;
        private Boolean _changed;

        public DateTime CreateAt => _cacheItem.CreateAt;
        public DateTime? UpdateAt => _cacheItem.UpdateAt;

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
            }
            _cacheSerializer = cacheSerializer;
        }

        public Object Key => _cacheItem.Key;

        public Object Value
        {
            get { return _cacheItem.Value!; }
            set
            {
                if (value == null)
                {
                    _cacheItem.Type = null;
                    _cacheItem.Value = "null";
                    _cacheItem.Size = 4;
                }
                else
                {
                    var type = value.GetType();
                    if (type.IsNestedPrivate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), "nested private value not supported");
                    }

                    var typeName = type.FullName;
                    if (type.IsPrimitive == false)
                    {
                        typeName += ", " + type.Assembly.GetName().Name;
                    }

                    _cacheItem.Type = typeName;
                    var json = _cacheSerializer.SerializeObject(value);
                    //if (json != _cacheItem.Value)
                    //{
                        _cacheItem.Value = json;
                        _cacheItem.Size = json.Length;

                        if (_entityState != EntityState.Added)
                        {
                            _changed = true;
                        }
                    //}
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

                    if (_entityState != EntityState.Added)
                    {
                        _changed = true;
                    }
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

                    if (_entityState != EntityState.Added)
                    {
                        _changed = true;
                    }
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

                    if (_entityState != EntityState.Added)
                    {
                        _changed = true;
                    }
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

                    if (_entityState != EntityState.Added)
                    {
                        _changed = true;
                    }
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

                    if (_entityState != EntityState.Added)
                    {
                        _changed = true;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_entityState.HasValue && _entityState == EntityState.Added)
            {
                _dbContext.CacheItems.Add(_cacheItem);
            }
            else
            {
                if (_changed)
                {
                    _cacheItem.UpdateAt = DateTime.Now;
                }
            }
            _dbContext.SaveChanges();
        }
    }
}