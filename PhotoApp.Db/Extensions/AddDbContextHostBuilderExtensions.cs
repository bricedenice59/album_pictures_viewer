using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.QueryService;

namespace PhotoApp.Db.Extensions
{
    public static class AddDbContextHostBuilderExtensions
    {
        public static IHostBuilder AddDbContext(this IHostBuilder host)
        {
            host.ConfigureServices((context, services) =>
            {
                string connectionString = context.Configuration.GetConnectionString("sqlite");
                Action<DbContextOptionsBuilder> configureDbContext =
                    o => o.UseSqlite(connectionString)
                        .UseLazyLoadingProxies();

                services.AddDbContext<AppDbContext>(configureDbContext);
                services.AddSingleton<AppDbContextFactory>(new AppDbContextFactory(configureDbContext));
            });

            return host;
        }
    }
}
