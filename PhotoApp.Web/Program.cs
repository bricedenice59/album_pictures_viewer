using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoApp.Web
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
