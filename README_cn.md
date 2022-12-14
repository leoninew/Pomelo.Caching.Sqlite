## 快速开始

1. 添加依赖

```bash
dotnet add package Pomelo.Caching.Sqlite --version 1.2.3
```

2. 基本使用

和 `IMemoryCache` 的常规用法一样

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

默认情况下项目使用 Newtonsoft.Json 进行序列化和反序列化，也支持配置时修改

```c#
    .AddSqliteCache(options =>
    {
        options.Path = "sqlite_cache_newtonsoft.db";
        options.PurgeOnStartup = true;
        options.Serializer = new TextJsonSqliteCacheSerializer();
    })
```

支持缓存 key 前缀

```c#
    cache = cache.WithPrefix("preifx_");
```

从 [src/WebApplication1/Startup.cs](src/WebApplication1/Startup.cs) 获取更多用例

## 你应该知道的

1. SQLite 比内存要慢得多得多
2. **并不是**所有的东西都可以序列化和反序列化，因为我们存储到了 SQLite 
3. IChangeToken 目前还没有支持

## 为什么有这样一个项目

部分场景下使用 `IMemoryCache` 是不够的

1. 内存很昂贵， 性能却不是那么敏感
2. 缓存的可视化很用