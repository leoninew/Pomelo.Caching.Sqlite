using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly ILogger _logger;
        private IMemoryCache _memoryCache;
        private readonly SqliteDbContext _dbContext;

        public BlogController(ILogger<BlogController> logger, IMemoryCache memoryCache, SqliteDbContext dbContext)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<Blog> Get()
        {
            _logger.LogInformation("get blog list");
            var key = "blog";
            if (_memoryCache.TryGetValue("blog", out Blog[] list) == false)
            {
                _logger.LogInformation("cache {Key} missing", key);
                list = _dbContext.Blogs.ToArray();

                _logger.LogInformation("cache {Key} set", key);
                _memoryCache.Set(key, list);
            }
            else
            {
                _logger.LogInformation("cache {Key} hint", key);
            }
            return list;
        }


        [HttpPost]
        public Int32 Post([FromBody] Blog blog)
        {
            _logger.LogInformation("create blog");
            var key = "blog";
            _dbContext.Blogs.Add(blog);
            var row = _dbContext.SaveChanges();

            _logger.LogInformation("cache {Key} expire", key);
            _memoryCache.Remove(key);
            return row;
        }
    }
}