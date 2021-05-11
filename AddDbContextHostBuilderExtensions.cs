using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EntityFrameworkDotNet50
{
    public static class AddDbContextHostBuilderExtensions
    {
        public static IHostBuilder AddDbContext(this IHostBuilder host)
        {
            host.ConfigureServices((context, services) =>
            {
                string connectionString = context.Configuration.GetConnectionString("sqlite");
                Action<DbContextOptionsBuilder> configureDbContext = o => o.UseSqlite(connectionString);

                services.AddDbContext<SimpleDbContext>(configureDbContext);
                services.AddSingleton<SimpleDbContextFactory>(new SimpleDbContextFactory(configureDbContext));
            });

            return host;
        }
    }
}
