using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PhotoApp.APIs.Utils;
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
            string authHeaderBearerStr = "Bearer" + " ";
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
                    RequireExpirationTime = false,
                    ValidateIssuerSigningKey = true,

                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Headers.ContainsKey("Authorization"))
                        {
                            //extract header
                            Microsoft.Extensions.Primitives.StringValues accessTokens = context.Request.Headers["Authorization"];
                            var header = accessTokens.FirstOrDefault();
                            if (header != null)
                            {
                                if(header.Contains(authHeaderBearerStr))
                                    header = header.Substring(authHeaderBearerStr.Length);

                                //finally decrypt token
                                context.Token = AesUtils.DecryptString(header);
                            }
                             
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddControllers();
            services.AddSingleton<ILibMonitor, LibMonitor>();
            services.AddSingleton<PhotoApp.APIs.AuthenticationServices.IAuthenticationService, PhotoApp.APIs.AuthenticationServices.AuthenticationService>();
            services.AddSingleton<IMeasureTimePerformance, MeasureTimePerformance>();

            services.AddHostedService<ApplicationPartsLogger>();
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
                //https://github.com/kobake/AspNetCore.RouteAnalyzer/issues/28
                endpoints.MapGet("/routes", request =>
                {
                    request.Response.Headers.Add("content-type", "application/json");

                    var ep = endpoints.DataSources.First().Endpoints.Select(e => e as RouteEndpoint);
                    return request.Response.WriteAsync(
                        JsonSerializer.Serialize(
                            ep.Select(e => new
                            {
                                Method = ((HttpMethodMetadata)e.Metadata.First(m => m.GetType() == typeof(HttpMethodMetadata))).HttpMethods.First(),
                                Route = e.RoutePattern.RawText
                            })
                        )
                    );
                });
            });

            Task.Run(() =>
            {
                var libMonitor = app.ApplicationServices.GetService<ILibMonitor>();
                libMonitor.MonitorFolder();
            });
        }
    }
}
