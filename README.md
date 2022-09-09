## Quick Start

1. add dependency

```bash
dotnet add package Pomelo.Caching.Sqlite --version 1.2.3
```

2. usage

Use it as well as `IMemoryCache`.

```c#
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.Caching.Sqlite;

var cache = new ServiceCollection()
    .AddSqliteCache(conf => conf.Path = "sqlite_cache.db")
    .BuildServiceProvider()
    .EnsureSqliteCacheInitialized()
    .GetRequiredService<IMemoryCache>();

cache.Set("time", DateTime.Now, TimeSpan.FromSeconds(10));

var time = cache.Get<DateTime>("time");
```

Newtonsoft.Json should be used for serialize and deserialize which could be changed via configure

```c#
    .AddSqliteCache(options =>
    {
        options.Path = "sqlite_cache_newtonsoft.db";
        options.PurgeOnStartup = true;
        options.Serializer = new TextJsonSqliteCacheSerializer();
    })
```

Cache key prefix is supported

```c#
    cache = cache.WithPrefix("preifx_");
```

Get more usage from [src/WebApplication1/Startup.cs](src/WebApplication1/Startup.cs)

## What you should know

1. SQLite is much more slower than memory
2. NOT everything can be serialized and deserialized since we use sqlite to store
3. IChangeToken is absent here

## Why This Project?

Using `IMemoryCache` is not enough in some particular scene.

1. Memory is expensive, performance is not that sensitive.
2. Visualization of the cache is useful