using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
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
}
