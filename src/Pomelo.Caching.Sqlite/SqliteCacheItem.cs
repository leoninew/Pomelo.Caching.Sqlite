using System;
using Microsoft.Extensions.Caching.Memory;


namespace Pomelo.Caching.Sqlite
{
    class SqliteCacheItem
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public String Key { get; set; }
        public String Value { get; set; }
        public String? Type { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public bool HasExpired()
        {
            if (AbsoluteExpiration.HasValue)
            {
                return DateTimeOffset.Now > AbsoluteExpiration.Value;
            }

            var recordAt = UpdateAt.GetValueOrDefault(CreateAt);
            if (AbsoluteExpirationRelativeToNow.HasValue)
            {
                return DateTime.Now > recordAt.Add(AbsoluteExpirationRelativeToNow.Value);
            }

            if (SlidingExpiration.HasValue)
            {
                return DateTime.Now > recordAt.Add(SlidingExpiration.Value);
            }

            return false;
        }
    }
}