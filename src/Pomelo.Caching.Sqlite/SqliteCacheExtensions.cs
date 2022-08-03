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
        public static IServiceCollection AddSqliteCache(this IServiceCollection services, Action<SqliteCacheOptions>? configureOptions = null)
        {
            services.Configure(configureOptions ?? ConfigureOptions);
            services.AddDbContext<SqliteCacheContext>(
                   OptionsAction,
                   contextLifetime: ServiceLifetime.Transient,
                   optionsLifetime: ServiceLifetime.Transient);

           
            //    builder.EnableSensitiveDataLogging();
            //    builder.UseLoggerFactory(services.GetRequiredService<ILoggerFactory>());
            

            services.AddSingleton<IMemoryCache, SqliteCache>();
            services.AddTransient(services => services.GetRequiredService<IOptions<SqliteCacheOptions>>().Value.Serializer ?? new NewtonsoftSqliteCacheSerializer());
            return services;
        }

        private static void ConfigureOptions(SqliteCacheOptions options)
        {
            options.PrugeOnStartup = true;
            options.Path = "cache.db";
        }

        private static void OptionsAction(IServiceProvider services, DbContextOptionsBuilder builder)
        {
            var options = services.GetRequiredService<IOptions<SqliteCacheOptions>>();
            options.Value.Path = options.Value.Path ?? "cache.db";
            if (options.Value.PrugeOnStartup && File.Exists(options.Value.Path))
            {
                File.Delete(options.Value.Path);
            }

            builder.UseSqlite("Data Source=" + options.Value.Path);
        }
    }
}