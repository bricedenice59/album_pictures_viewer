using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PhotoApp.Db.Models;
using PhotoApp.Utils;

namespace PhotoApp.APIs
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
            services.AddAuthentication("OAuth")
            .AddJwtBearer("OAuth", options =>
            {
                var secretBytes = Encoding.UTF8.GetBytes(Configuration["Auth0:Secret"]);
                var key = new SymmetricSecurityKey(secretBytes);

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Auth0:Issuer"],
                    ValidAudience = Configuration["Auth0:Audience"],
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };

            });

            services.AddControllers();
            services.AddSingleton<ILibMonitor, LibMonitor>();
            services.AddSingleton<PhotoApp.APIs.AuthenticationServices.IAuthenticationService, PhotoApp.APIs.AuthenticationServices.AuthenticationService>();
            services.AddSingleton<IMeasureTimePerformance, MeasureTimePerformance>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Task.Run(() =>
            {
                var libMonitor = app.ApplicationServices.GetService<ILibMonitor>();
                libMonitor.MonitorFolder();
            });
        }
    }
}
