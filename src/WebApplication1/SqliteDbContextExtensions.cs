using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApplication1
{
    public static class SqliteDbContextExtensions
    {
        public static IServiceCollection AddSqliteDbContext(this IServiceCollection services, Action<SqliteDbContextOptions> configureOptions = null)
        {
            services.Configure(configureOptions ?? ConfigureOptions);
            services.AddDbContext<SqliteDbContext>(
                OptionsAction,
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Singleton);
            // services.AddTransient<ICommandExecuteReportRepository, CommandExecuteReportRepository>();
            return services;
        }

        private static void ConfigureOptions(SqliteDbContextOptions options)
        {
            options.PrugeOnStartup = true;
            options.Path = "sqlite.db";
        }

        private static void OptionsAction(IServiceProvider services, DbContextOptionsBuilder builder)
        {
            var options = services.GetRequiredService<IOptions<SqliteDbContextOptions>>();
            options.Value.Path = options.Value.Path ?? "sqlite.db";
            if (options.Value.PrugeOnStartup && File.Exists(options.Value.Path))
            {
                File.Delete(options.Value.Path);
            }

            builder.UseSqlite("Data Source=" + options.Value.Path);
        }

        public static void EnsureSqliteDbCreated(this IApplicationBuilder app)
        {
            var dbContext = app.ApplicationServices.GetRequiredService<SqliteDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }

    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(build =>
            {
                build.HasKey(x => x.Id);
                build.Property(x => x.Id).ValueGeneratedOnAdd();
            });
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Blog> Blogs { get; set; }
    }

    public class SqliteDbContextOptions
    {
        public Boolean PrugeOnStartup { get; set; }
        public String Path { get; set; }
    }
}
