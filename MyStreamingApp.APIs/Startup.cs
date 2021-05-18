using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyStreamingApp.Utils.DbContext;
using MyStreamingApp.Utils.Models;
using File = System.IO.File;

namespace MyStreamingApp.APIs
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            AppDbContextFactory dbContextFactory, ILogger<Startup> logger)
        {
            var dbName = "MusicLibrary.db";

            //check if db is in music folder, if not copy it
            var dbPath = Path.Combine("/music", dbName);
            if (!File.Exists(dbPath))
            {
                var resourcePath = dbContextFactory.GetType().Assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(dbName));

                using (var resource = dbContextFactory.GetType().Assembly.GetManifestResourceStream(resourcePath))
                {
                    using (var file = new FileStream(dbPath, FileMode.Create, FileAccess.Write))
                    {
                        resource?.CopyToAsync(file);
                    }
                }
            }

            Task.Run(async () =>
            {
                Dictionary<string, AlbumDto> lstAlbums = new Dictionary<string, AlbumDto>();
                Dictionary<string, TrackDto> lstTracks = new Dictionary<string, TrackDto>();
                var files = Directory.GetFiles("/music", "*.flac", SearchOption.AllDirectories);
                files.ToList().ForEach(x =>
                {
                    TagLib.File tfile = null;
                    try
                    {
                        tfile = TagLib.File.Create(x);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (tfile != null)
                    {
                        var albumName = tfile.Tag.Album;
                        var albumPath = Path.GetDirectoryName(x);
                        var trackTitle = tfile.Tag.Title;
                        var releasedYear = (int)tfile.Tag.Year;

                        if (!lstAlbums.ContainsKey(albumName + albumPath))
                            lstAlbums.Add(albumName + albumPath, new AlbumDto()
                            {
                                Name = !string.IsNullOrEmpty(albumName) ? albumName : albumPath,
                                AlbumPath = albumPath,
                                ReleasedYear = releasedYear
                            });
                        if (!lstTracks.ContainsKey(trackTitle + albumPath))
                            lstTracks.Add(trackTitle + albumPath, new TrackDto()
                            {
                                Title = !string.IsNullOrEmpty(trackTitle) ? trackTitle : "no title",
                                FilePath = x,
                                Year = releasedYear,
                                Album = lstAlbums[albumName + albumPath]
                            });
                    }
                });

                using (AppDbContext context = dbContextFactory.CreateDbContext())
                {
                    try
                    {
                        foreach (var uniqueAlbum in lstAlbums.Values)
                            await context.Albums.AddAsync(uniqueAlbum);

                        foreach (var uniqueTrack in lstTracks.Values)
                            await context.Tracks.AddAsync(uniqueTrack);

                        await context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
