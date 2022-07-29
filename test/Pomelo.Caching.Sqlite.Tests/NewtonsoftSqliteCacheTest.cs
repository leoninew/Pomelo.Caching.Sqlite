using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace Pomelo.Caching.Sqlite.Tests
{
    public class NewtonsoftSqliteCache
    {
        private readonly IMemoryCache cache;

        public NewtonsoftSqliteCache()
        {
            cache = new ServiceCollection()
                .AddSqliteCache(conf =>
                {
                    conf.Path = "sqlite_cache_newtonsoft.db";
                    conf.PrugeOnStartup = false;
                    conf.Serializer = new NewtonsoftSqliteCacheSerializer();
                })
                .BuildServiceProvider()
                .GetRequiredService<IMemoryCache>();
        }

        [Fact]
        public void TryGetValue_ForNotExisted_ReturnFalse()
        {
            var existed = cache.TryGetValue("TryGetValue_ForNotExisted_ReturnFalse", out Object value);

            Assert.False(existed);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_ForExisted_ReturnTrue()
        {
            var key = "TryGetValue_ForExisted_ReturnTrue";
            cache.Set(key, 1024, TimeSpan.FromSeconds(1));

            var existed = cache.TryGetValue(key, out Object value);
            Assert.True(existed);
            Assert.Equal(1024, value);
        }

        [Fact]
        public void CreateEntry_ThenDispose_CachingSpecified()
        {
            var key = "CreateEntry_ThenDispose_CachingSpecified";
            cache.CreateEntry(key)
                .SetValue("Hello")
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(1))
                .Dispose();

            var existed = cache.TryGetValue(key, out Object value);
            Assert.True(existed);
            Assert.Equal("Hello", value);
        }


        [Fact]
        public void CreateEntry_ForgetDispose_CachingNothing()
        {
            var key = "CreateEntry_ForgetDispose_CachingNothing";
            cache.CreateEntry(key)
                .SetValue("Hello")
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
            //.Dispose();

            var existed = cache.TryGetValue(key, out Object value);
            Assert.False(existed);
            Assert.Null(value);
        }

        [Fact]
        public async Task AbsoluteExpiration()
        {
            var key = "AbsoluteExpiration";
            cache.Set(key, Guid.NewGuid(), DateTime.Now.Add(TimeSpan.FromSeconds(1)));

            await Task.Delay(TimeSpan.FromSeconds(1.1));
            var existed = cache.TryGetValue(key, out _);
            Assert.False(existed);
        }


        [Fact]
        public async Task AbsoluteExpirationRelativeToNow()
        {
            var key = "AbsoluteExpirationRelativeToNow";
            cache.Set(key, Guid.NewGuid(), TimeSpan.FromSeconds(1));

            await Task.Delay(TimeSpan.FromSeconds(1.1));
            var existed = cache.TryGetValue(key, out _);
            Assert.False(existed);
        }

        [Fact]
        public async Task SlidingExpiration()
        {
            var key = "SlidingExpiration";
            cache.CreateEntry(key)
                .SetValue("Hello")
                .SetSlidingExpiration(TimeSpan.FromSeconds(1.5))
                .Dispose();

            var existed = cache.TryGetValue(key, out Object value);
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


        [Fact]
        public void SetValue_ThenRemove_ReturnFalse()
        {
            var key = "SetValue_ThenRemove_ReturnFalse";
            cache.Set(key, new Object());
            cache.Remove(key);
            var existed = cache.TryGetValue(key, out _);
            Assert.False(existed);
        }

        [Fact]
        public void SetPrimitive_ThenGet_ReturnPrimitive()
        {
            var key = "SetPrimitive_ThenGet_ReturnPrimitive";
            var value = (Byte)128;
            cache.Set(key, value);
            var value1 = cache.Get(key);
            Assert.Equal(value1, value);
            var value2 = cache.Get<Byte>(key);
            Assert.Equal(value2, value);
        }

        [Fact]
        public void SetComposite_ThenGet_ReturnComposite()
        {
            var key = "SetComposite_ThenGet_ReturnComposite";
            var value = new Student
            {
                Name = "Rattz",
                Birth = DateTime.Now,
                Address = new[]
                {
                    "Raymond",
                    "Minx",
                }
            };
            cache.Set(key, value);
            var value1 = cache.Get(key);
            Assert.Equal(value1, value);
            var value2 = cache.Get<Student>(key);
            Assert.Equal(value2, value);
        }

        [Fact]
        public void SetNumber_GetString_ThrowInvalidCastException()
        {
            var key = "SetNumber_GetString_ThrowInvalidCastException";
            cache.Set(key, 1024);
            Assert.Throws<InvalidCastException>(() => cache.Get<String>(key));
        }
    }

    class Student : IEquatable<Student>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public String Name { get; set; }
        public DateTime Birth { get; set; }
        public String[] Address { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public bool Equals(Student? other)
        {
            if (other == null)
            {
                return false;
            }

            return String.Equals(
                JsonConvert.SerializeObject(this),
                JsonConvert.SerializeObject(other),
                StringComparison.Ordinal);
        }
    }
}