using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using File = System.IO.File;

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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            AppDbContextFactory dbContextFactory, ILogger<Startup> logger)
        {
            var dbName = "PhotosLibrary.db";
            var photosFolder = "/photos";

            AppDbContext context = dbContextFactory.CreateDbContext();
            //check if db is in music folder, if not copy it
            var dbPath = Path.Combine(photosFolder, dbName);
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

                var files = Directory.GetFiles(photosFolder, "*.jpg", SearchOption.AllDirectories);
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
                        throw;
                    }


                    var title = Path.GetFileName(x);
                    var albumPath = Path.GetDirectoryName(x);
                    var tag = tfile.Tag as TagLib.Image.CombinedImageTag;
                    DateTime? snapshot = tag?.DateTime;

                    PhotoDto photoDto = new PhotoDto()
                    {
                        Title = title,
                        AlbumPath = albumPath,
                        Date = snapshot.HasValue
                            ? snapshot?.ToString(CultureInfo.InvariantCulture)
                            : new DateTime(2001, 01, 01).ToString(CultureInfo.InvariantCulture)
                    };

                    bool thumbnailCreated = false;
                    try
                    {
                        Console.WriteLine(x);
                        // Load image.
                        Image image = Image.FromFile(x);

                        // Compute thumbnail size.
                        Size thumbnailSize = PhotoApp.Utils.ImageUtils.GetThumbnailSize(image);

                        // Get thumbnail.
                        Image thumbnail = image.GetThumbnailImage(thumbnailSize.Width,
                            thumbnailSize.Height, null, IntPtr.Zero);

                        using (MemoryStream outStream = new MemoryStream())
                        {
                            thumbnail.Save(outStream,ImageFormat.Jpeg);
                            photoDto.Thumbnail = outStream.ToArray();
                        }

                        thumbnailCreated = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if(thumbnailCreated)
                        context.Photos.Add(photoDto);
                });

                try
                {
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }


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
