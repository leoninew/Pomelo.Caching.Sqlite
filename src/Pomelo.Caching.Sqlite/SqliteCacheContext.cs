using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Pomelo.Caching.Sqlite
{
    class SqliteCacheContext : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SqliteCacheContext(DbContextOptions options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SqliteCacheItem>(options =>
            {
                options.ToTable("SqliteCacheItem")
                    .HasKey(x => x.Key);
            });

            base.OnModelCreating(builder);
        }

        public DbSet<SqliteCacheItem> CacheItems { get; set; }
    }
}