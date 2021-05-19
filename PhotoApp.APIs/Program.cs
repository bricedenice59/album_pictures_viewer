using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PhotoApp.Db.Extensions;

namespace PhotoApp.APIs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddConfiguration()
                .AddDbContext()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
