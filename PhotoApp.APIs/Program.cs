using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PhotoApp.Db.Extensions;

namespace PhotoApp.APIs
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
#else
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
#endif
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddConfiguration()
                .AddDbContext()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options => options.ConfigureEndpoints());
                    webBuilder.UseStartup<Startup>();
                });
    }
}
