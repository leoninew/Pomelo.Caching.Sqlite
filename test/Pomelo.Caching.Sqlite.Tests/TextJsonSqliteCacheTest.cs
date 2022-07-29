using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Pomelo.Caching.Sqlite.Tests
{
    public class TextJsonSqliteCache
    {
        private readonly ServiceProvider _services;

        public TextJsonSqliteCache()
        {
            _services = new ServiceCollection()
                .AddSqliteCache(conf =>
                {
                    conf.Path = "sqlite_cache_system.db";
                    conf.PrugeOnStartup = false;
                    conf.Serializer = new TextJsonSqliteCacheSerializer();
                })
                .BuildServiceProvider();
        }

        [Fact]
        public void TryGetValue_ForNotExisted_ReturnFalse()
        {
            var cache = _services.GetRequiredService<IMemoryCache>();
            var existed = cache.TryGetValue("TryGetValue_ForNotExisted_ReturnFalse", out Object value);

            Assert.False(existed);
            Assert.Null(value);
        }


        [Fact]
        public void TryGetValue_ForExisted_ReturnTrue()
        {
            var cache = _services.GetRequiredService<IMemoryCache>();
            cache.Set("TryGetValue_ForExisted_ReturnTrue", 1024, TimeSpan.FromSeconds(1));

            var existed = cache.TryGetValue("TryGetValue_ForExisted_ReturnTrue", out Object value);
            Assert.True(existed);
            Assert.Equal(1024, value);
        }

        [Fact]
        public void CreateEntry_ThenDispose_CachingSpecified()
        {
            var cache = _services.GetRequiredService<IMemoryCache>();
            cache.CreateEntry("CreateEntry_ThenDispose_CachingSpecified")
                .SetValue("Hello")
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(1))
                .Dispose();

            var existed = cache.TryGetValue("CreateEntry_ThenDispose_CachingSpecified", out Object value);
            Assert.True(existed);
            Assert.Equal("Hello", value);
        }


        [Fact]
        public void CreateEntry_ForgetDispose_CachingNothing()
        {
            var cache = _services.GetRequiredService<IMemoryCache>();
            cache.CreateEntry("CreateEntry_ForgetDispose_CachingNothing")
                .SetValue("Hello")
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
            //.Dispose();

            var existed = cache.TryGetValue("CreateEntry_ForgetDispose_CachingNothing", out Object value);
            Assert.False(existed);
            Assert.Null(value);
        }

        [Fact]
        public async Task AbsoluteExpiration()
        {
            var cache = _services.GetRequiredService<IMemoryCache>();
            cache.Set("AbsoluteExpiration", Guid.NewGuid(), DateTime.Now.Add(TimeSpan.FromSeconds(1)));

            await Task.Delay(TimeSpan.FromSeconds(1.1));
            var existed = cache.TryGetValue("AbsoluteExpiration", out _);
            Assert.False(existed);
        }


        [Fact]
        public async Task AbsoluteExpirationRelativeToNow()
        {
            var cache = _services.GetRequiredService<IMemoryCache>();
            cache.Set("AbsoluteExpirationRelativeToNow", Guid.NewGuid(), TimeSpan.FromSeconds(1));

            await Task.Delay(TimeSpan.FromSeconds(1.1));
            var existed = cache.TryGetValue("AbsoluteExpirationRelativeToNow", out _);
            Assert.False(existed);
        }

        [Fact]
        public async Task SlidingExpiration()
        {
            var cache = _services.GetRequiredService<IMemoryCache>();
            cache.CreateEntry("SlidingExpiration")
                .SetValue("Hello")
                .SetSlidingExpiration(TimeSpan.FromSeconds(1.5))
                .Dispose();

            var existed = cache.TryGetValue("SlidingExpiration", out Object value);
            Assert.True(existed);
            Assert.Equal("Hello", value);

            // delay 1s
            await Task.Delay(TimeSpan.FromSeconds(1));
            existed = cache.TryGetValue("SlidingExpiration", out _);
            Assert.True(existed);

            // delay 1s
            await Task.Delay(TimeSpan.FromSeconds(1));
            existed = cache.TryGetValue("SlidingExpiration", out _);
            Assert.True(existed);

            // delay 2s
            await Task.Delay(TimeSpan.FromSeconds(2));
            existed = cache.TryGetValue("SlidingExpiration", out _);
            Assert.False(existed);
        }
    }
}