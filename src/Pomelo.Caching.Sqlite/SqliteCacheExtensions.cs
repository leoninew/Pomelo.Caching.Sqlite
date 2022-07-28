using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.Caching.Sqlite
{
    public static class SqliteCacheExtensions
    {
        public static IServiceCollection AddSqliteCache(this IServiceCollection services, Action<SqliteCacheOptions>? setupAction = null)
        {
            var options = new SqliteCacheOptions
            {
                Path = "sqlite_cache.db",
                PrugeOnStartup = true,
            };
            setupAction?.Invoke(options);
            if (options.PrugeOnStartup && File.Exists(options.Path))
            {
                File.Delete(options.Path);
            }

            services
               .AddDbContext<SqliteCacheContext>(
                   builder => builder.UseSqlite("Data Source=" + options.Path),
                   contextLifetime: ServiceLifetime.Transient);

            //services.AddDbContext<SqliteDbContext>((services, builder) =>
            //{
            //    builder.EnableSensitiveDataLogging();
            //    builder.UseLoggerFactory(services.GetRequiredService<ILoggerFactory>());
            //    builder.UseSqlite(options.ConnectionString);
            //}, contextLifetime: ServiceLifetime.Transient);

            services.AddSingleton<IMemoryCache, SqliteCache>();
            services.AddTransient<ISqliteCacheSerializer>(_ => options.Serializer ?? new NewtonsoftSqliteCacheSerializer());
            return services;
        }
    }
}