using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Pomelo.Caching.Sqlite
{
    public static class SqliteCacheExtensions
    {
        private const String DropTableSql = "DROP TABLE IF EXISTS [SqliteCacheItem]";
        private const String CreateTableSql =
@"CREATE TABLE IF NOT EXISTS [SqliteCacheItem] (
	[Key] text NOT NULL PRIMARY KEY, 
	[Value] text, 
	[Type] text, 
	[AbsoluteExpiration] text, 
	[AbsoluteExpirationRelativeToNow] text, 
	[SlidingExpiration] text, 
	[Priority] integer NOT NULL, 
	[Size] integer, 
	[CreateAt] text NOT NULL, 
	[UpdateAt] text
)";

        public static IServiceCollection AddSqliteCache(this IServiceCollection services, Action<SqliteCacheOptions>? configureOptions = null)
        {
            services.Configure(configureOptions ?? ConfigureOptions);
            services.AddDbContext<SqliteCacheContext>(
                   OptionsAction,
                   contextLifetime: ServiceLifetime.Transient,
                   optionsLifetime: ServiceLifetime.Singleton);

            services.AddSingleton<IMemoryCache, SqliteCache>();
            services.AddTransient(services => services.GetRequiredService<IOptions<SqliteCacheOptions>>().Value.Serializer ?? new NewtonsoftSqliteCacheSerializer());
            return services;
        }

        private static void ConfigureOptions(SqliteCacheOptions options)
        {
            options.DropOnStartup = false;
            options.PurgeOnStartup = true;
            options.Path = Path.Combine(AppContext.BaseDirectory, "cache.db");
        }

        private static void OptionsAction(IServiceProvider services, DbContextOptionsBuilder builder)
        {
            // builder.EnableSensitiveDataLogging();
            // builder.UseLoggerFactory(services.GetRequiredService<ILoggerFactory>());

            var options = services.GetRequiredService<IOptions<SqliteCacheOptions>>();
            options.Value.Path = options.Value.Path ?? Path.Combine(AppContext.BaseDirectory, "cache.db");
            if (options.Value.DropOnStartup && File.Exists(options.Value.Path))
            {
                File.Delete(options.Value.Path);
            }

            var path = Path.IsPathRooted(options.Value.Path)
                ? options.Value.Path
                : Path.Combine(AppContext.BaseDirectory, options.Value.Path);
            builder.UseSqlite("Data Source=" + path);
        }

        public static IServiceProvider EnsureSqliteCacheInitialized(this IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<SqliteCacheContext>();
            var options = services.GetRequiredService<IOptions<SqliteCacheOptions>>();

            dbContext.Database.EnsureCreated();
            if (options.Value.PurgeOnStartup)
            {
                dbContext.Database.ExecuteSqlRaw(DropTableSql);
            }

            dbContext.Database.ExecuteSqlRaw(CreateTableSql);
            return services;
        }
    }
}