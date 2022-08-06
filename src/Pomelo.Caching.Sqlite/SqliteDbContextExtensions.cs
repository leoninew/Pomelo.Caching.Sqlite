using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Pomelo.Caching.Sqlite
{
    public static class SqliteDbContextExtensions
    {
        public static IServiceCollection AddSqliteDbContext<TContext>(this IServiceCollection services, Action<SqliteDbContextOptions>? configureOptions = null)
            where TContext : DbContext
        {
            services.Configure(configureOptions ?? ConfigureOptions);
            services.AddDbContext<TContext>(
                OptionsAction,
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Singleton);
            return services;
        }

        private static void ConfigureOptions(SqliteDbContextOptions options)
        {
            options.DropOnStartup = true;
            options.Path = Path.Combine(AppContext.BaseDirectory, "sqlite.db");
        }

        private static void OptionsAction(IServiceProvider services, DbContextOptionsBuilder builder)
        {
            var options = services.GetRequiredService<IOptions<SqliteDbContextOptions>>();
            options.Value.Path = options.Value.Path ?? Path.Combine(AppContext.BaseDirectory, "sqlite.db");
            if (options.Value.DropOnStartup && File.Exists(options.Value.Path))
            {
                File.Delete(options.Value.Path);
            }

            var path = Path.IsPathRooted(options.Value.Path)
                ? options.Value.Path
                : Path.Combine(AppContext.BaseDirectory, options.Value.Path);
            builder.UseSqlite("Data Source=" + path);
        }

        public static void EnsureSqliteDbCreated<TContext>(this IServiceProvider services) where TContext : DbContext
        {
            var dbContext = services.GetRequiredService<TContext>();
            dbContext.Database.EnsureCreated();
        }
    }
}
