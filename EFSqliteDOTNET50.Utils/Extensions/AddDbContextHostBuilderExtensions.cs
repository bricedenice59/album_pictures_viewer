using System;
using EFSqliteDOTNET50.Utils.DbContext;
using EFSqliteDOTNET50.Utils.QueryService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFSqliteDOTNET50.Utils.Extensions
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
                        .AddInterceptors(new QueryCommandInterceptor());

                services.AddDbContext<SimpleDbContext>(configureDbContext);
                services.AddSingleton<SimpleDbContextFactory>(new SimpleDbContextFactory(configureDbContext));
            });

            return host;
        }
    }
}
