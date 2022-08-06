using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication1.Middlewares;
using Pomelo.Caching.Sqlite;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // MUST before services.AddMvc(...)
            services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);
            services.AddControllers();
            services.AddSwaggerGen();
            services.AddSqliteDbContext<SqliteDbContext>(options =>
            {
                options.Path = "sqlite.db";
                options.DropOnStartup = true;
            });
            services.AddSqliteCache(options =>
            {
                options.Path = "sqlite.db";
                options.DropOnStartup = false; // drop again would lost some table defined on SqliteDbContext
                options.PurgeOnStartup = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLoggingPro();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.ApplicationServices.EnsureSqliteDbCreated<SqliteDbContext>();
            app.ApplicationServices.EnsureSqliteCacheInitialized();
        }
    }
}
