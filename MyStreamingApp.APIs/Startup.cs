using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyStreamingApp.Utils.DbContext;
using MyStreamingApp.Utils.Models;

namespace MyStreamingApp.APIs
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            AppDbContextFactory dbContextFactory, ILogger<Startup> logger)
        {
            var dbName = "MusicLibrary.db";

            //check if db is in music folder, if not copy it
            var dbPath = Path.Combine("/music", dbName);
            if (!File.Exists(dbPath))
            {
                var resourcePath = dbContextFactory.GetType().Assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(dbName));

                using (var resource = dbContextFactory.GetType().Assembly.GetManifestResourceStream(resourcePath))
                {
                    using (var file = new FileStream(dbPath, FileMode.Create, FileAccess.Write))
                    {
                        resource?.CopyTo(file);
                    }
                }
            }

            try
            {
                var files = Directory.GetFiles("/music", "*.flac", SearchOption.AllDirectories);
                files.ToList().ForEach(x => logger.LogInformation(x));
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
