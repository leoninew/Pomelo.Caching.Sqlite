using System;

namespace Pomelo.Caching.Sqlite
{
    public class SqliteDbContextOptions
    {
        public Boolean DropOnStartup { get; set; }
        public String? Path { get; set; }
    }
}