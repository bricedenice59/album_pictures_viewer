using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PhotoApp.Db.Extensions
{
    public static class AddConfigurationHostBuilderExtensions
    {
        public static IHostBuilder AddConfiguration(this IHostBuilder host)
        {
            host.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json");
                config.AddEnvironmentVariables();
            });

            return host;
        }
    }
}
