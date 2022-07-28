using System;
using Microsoft.Extensions.Options;


namespace Pomelo.Caching.Sqlite
{
    public class SqliteCacheOptions : IOptions<SqliteCacheOptions>
    {
        public String? Path { get; set; }
        public Boolean PrugeOnStartup { get; set; }
        public ISqliteCacheSerializer? Serializer { get; set; }

        public SqliteCacheOptions Value => this;
    }
}