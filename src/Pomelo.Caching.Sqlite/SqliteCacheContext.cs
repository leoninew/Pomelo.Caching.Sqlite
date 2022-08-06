using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Pomelo.Caching.Sqlite
{
    class SqliteCacheContext : DbContext
    {
        public const String DropTableSql = "DROP TABLE IF EXISTS [SqliteCacheItems]";
        public const String CreateTableSql =
@"CREATE TABLE IF NOT EXISTS [SqliteCacheItems] (
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SqliteCacheContext(DbContextOptions<SqliteCacheContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SqliteCacheItem>(options =>
            {
                options.ToTable("SqliteCacheItems")
                    .HasKey(x => x.Key);
            });

            base.OnModelCreating(builder);
        }

        public DbSet<SqliteCacheItem> CacheItems { get; set; }
    }
}