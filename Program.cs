using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EntityFrameworkDotNet50
{
    class Program
    {
        private static IHost _host;

        public static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .AddConfiguration()
                .AddDbContext();

        }

        static void Main(string[] args)
        {
            _host = CreateHostBuilder().Build();
            _host.Start();

            SimpleDbContextFactory contextFactory =
                _host.Services.GetRequiredService<SimpleDbContextFactory>();

            using (SimpleDbContext context = contextFactory.CreateDbContext())
            {
                var lstUsers = context.Users.ToList();
            }
        }
    }
}
