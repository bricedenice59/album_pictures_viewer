using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using PhotoApp.Db.QueryService;
using PhotoApp.Utils;
using TagLib.Riff;
using File = System.IO.File;

namespace PhotoApp.APIs
{

    public class LibMonitor
    {
        private readonly AppDbContextFactory _dbContextFactory;
        private readonly ILogger<LibMonitor> _logger;
        private string dbName = "PhotosLibrary.db";
        private string photosFolder = "/photos";
        private string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        private System.Threading.Timer _timer;
        private static int _lockFlag = 0;


        readonly TimeSpan startTimeSpan = TimeSpan.Zero;
        readonly TimeSpan periodTimeSpan = TimeSpan.FromHours(1);

        public LibMonitor(AppDbContextFactory dbContextFactory, ILogger<LibMonitor> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public void MonitorFolder()
        {
            _timer = new System.Threading.Timer(async(e) =>
            {
                _logger.LogDebug($"Thread {Thread.CurrentThread.ManagedThreadId} tries to get access to syncDB fct");

                if (Interlocked.CompareExchange(ref _lockFlag, 1, 0) == 0)
                {
                    _logger.LogInformation("Monitoring photo folder...");
                    Monitor.Enter(_lockFlag);
                    _logger.LogDebug($"$Access given to Thread {Thread.CurrentThread.ManagedThreadId}");
                    _logger.LogInformation("Scan about to start...");
                    await SyncDb();
                    // free the lock
                    Interlocked.Decrement(ref _lockFlag);
                }
                else
                {
                    _logger.LogDebug($"$Access refused to Thread {Thread.CurrentThread.ManagedThreadId}");
                    _logger.LogInformation("Scan already processing...");
                }

                _timer.Change(periodTimeSpan, Timeout.InfiniteTimeSpan);
            }, null, startTimeSpan, periodTimeSpan);

        }


        private async Task CreateDb(string dbPath)
        {
            var resourcePath = _dbContextFactory.GetType().Assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(dbName));

            using (var resource = _dbContextFactory.GetType().Assembly.GetManifestResourceStream(resourcePath))
            {
                if (resource == null)
                    return;
                using (var file = new FileStream(dbPath, FileMode.Create, FileAccess.Write))
                {
                    await resource.CopyToAsync(file);
                }
            }
        }

        private async Task SyncDb()
        {
            var dbContext = _dbContextFactory.CreateDbContext();

            //check if db is in music folder, if not copy it
            var dbPath = Path.Combine(photosFolder, dbName);
            if (!File.Exists(dbPath))
                await CreateDb(dbPath);

            try
            {
                var lastDbUpdateTime = await dbContext.GetLastUpdateTime();
                _logger.LogInformation((lastDbUpdateTime.HasValue
                    ? $"Last library update was at {lastDbUpdateTime.Value.ToLocalTime()}"
                    : "Library has never been updated"));

                // Set the last update time now
                // Otherwise, files that change between now and update completion
                // might not get flagged for update up in the next sync round
                _logger.LogInformation($"Set last update time to {DateTime.UtcNow.ToUniversalTime()}");
                await dbContext.SetLastUpdateTime(DateTime.UtcNow.ToUniversalTime());

                _logger.LogInformation("Starting update: Scanning filesystem to find new/updated/deleted photos");

                var allPhotoFiles = Directory
                    .EnumerateFiles(photosFolder, "*", SearchOption.AllDirectories)
                    .Where(file=>AllowedExtensions.Any(file.ToLower().EndsWith)).ToList();

                //first time scan
                if (!lastDbUpdateTime.HasValue)
                {
                    await ProcessAndSaveEntity(allPhotoFiles, dbContext);
                }
                else
                {
                    List<string> added = new List<string>();
                    List<string> updated = new List<string>();
                    List<string> deleted = new List<string>();

                    var allPhotosInDb = dbContext.Photos.
                        ToDictionary(x=> Path.Combine(x.AlbumPath,x.Title), y=>y);

                    foreach (KeyValuePair<string, PhotoDto> entry in allPhotosInDb)
                    {
                        if (!File.Exists(entry.Key))
                        {
                            deleted.Add(entry.Key);
                        }
                        else
                        {
                            if (entry.Value.Filesize != new FileInfo(entry.Key).Length)
                                updated.Add(entry.Key);
                        }
                    }

                    var concatDeletedUpdated = deleted.Concat(updated);
                    for (int i = 0; i < allPhotoFiles.Count; i++)
                    {
                        var photoFilePath = allPhotoFiles[i];
                        if (concatDeletedUpdated.Any(x => x == photoFilePath))
                            continue;

                        if (!allPhotosInDb.ContainsKey(photoFilePath))
                            added.Add(photoFilePath);
                    }

                    _logger.LogInformation($"Found {added.Count} new photos, {updated.Count} updated photos, and {deleted.Count} deleted photos");

                    if (deleted.Count > 0)
                    {
                        List<PhotoDto> deletedPhotoDto = new List<PhotoDto>();
                        foreach (var _deleted in deleted)
                        {
                            PhotoDto photoDto = null;
                            allPhotosInDb.TryGetValue(allPhotosInDb.Keys.First(x => x == _deleted), out photoDto);
                            if (photoDto != null)
                                deletedPhotoDto.Add(photoDto);
                        }
                        await DeleteEntity(deletedPhotoDto, dbContext);
                    }
                    if (added.Count > 0)
                    {
                        await ProcessAndSaveEntity(added, dbContext);
                    }
                    if (updated.Count > 0)
                    {
                        await ProcessAndUpdateEntity(updated, _dbContextFactory);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await dbContext.DisposeAsync();
            }
        }

        private async Task ProcessAndSaveEntity(List<string> files, AppDbContext dbContext)
        {
            var allPhotoFilesChunks = MiscUtils.BreakIntoChunks<string>(files, 5);
            for (int i = 0; i < allPhotoFilesChunks.Count; i++)
            {
                var dicPhotos = await CreateDtoAndThumbnailAsync(allPhotoFilesChunks[i]);
                if (dicPhotos.Count > 0)
                {
                    await dbContext.Photos.AddRangeAsync(dicPhotos.Values);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private async Task ProcessAndUpdateEntity(List<string> files, AppDbContextFactory dbContextFactory)
        {
            var nonQueryService = new NonQueryDataService<PhotoDto>(dbContextFactory);
            var allPhotoFilesChunks = MiscUtils.BreakIntoChunks<string>(files, 5);
            for (int i = 0; i < allPhotoFilesChunks.Count; i++)
            {
                var dicPhotos = await CreateDtoAndThumbnailAsync(allPhotoFilesChunks[i]);

                foreach (var photoToUpdate in dicPhotos)
                {
                    await nonQueryService.Update(photoToUpdate.Value.Id, photoToUpdate.Value);
                }
            }
        }

        private async Task DeleteEntity(List<PhotoDto> files, AppDbContext dbContext)
        {
            await dbContext.BulkDeleteAsync(files);
        }

        private async Task<ConcurrentDictionary<string, PhotoDto>> CreateDtoAndThumbnailAsync(List<string> files)
        {
            var dicPhotos = new ConcurrentDictionary<string, PhotoDto>();
            Parallel.ForEach(files, async (x) =>
            {
                TagLib.File tfile = null;
                try
                {
                    tfile = TagLib.File.Create(x);
                }
                catch (Exception e)
                {
                    _logger.LogError($"File {x} ignored as TagLib can't parse it. {e.Message}");
                }

                if (tfile == null)
                {
                    return;
                }

                var title = Path.GetFileName(x);
                var albumPath = Path.GetDirectoryName(x);
                DateTime? snapshot;
                try
                {
                    var tag = tfile.Tag as TagLib.Image.CombinedImageTag;
                    snapshot = tag?.DateTime;
                }
                finally
                {
                    tfile.Dispose();
                }

                PhotoDto photoDto = new PhotoDto()
                {
                    Title = title,
                    AlbumPath = albumPath,
                    Filesize = new FileInfo(x).Length,
                    Date = snapshot.HasValue
                        ? snapshot?.ToString(CultureInfo.InvariantCulture)
                        : new DateTime(2001, 01, 01).ToString(CultureInfo.InvariantCulture)
                };

                try
                {
                    _logger.LogInformation($"Processing file {x}");
                    // Load image.
                    using (Image image = Image.FromFile(x))
                    {
                        // Compute thumbnail size.
                        Size thumbnailSize = PhotoApp.Utils.ImageUtils.GetThumbnailSize(image);

                        if (thumbnailSize.Equals(image.Size) ||
                            (image.Width < thumbnailSize.Width || image.Height < thumbnailSize.Height))
                        {
                            //if no shrinking is occurring, return the original bytes
                            using (var outStream = new MemoryStream())
                            {
                                image.Save(outStream, ImageFormat.Jpeg);
                                photoDto.Thumbnail = outStream.ToArray();
                            }
                        }
                        else
                        {
                            // Get thumbnail.
                            using (var thumbnail = image.GetThumbnailImage(thumbnailSize.Width,
                                thumbnailSize.Height, null, IntPtr.Zero))
                            {
                                using (var outStream = new MemoryStream())
                                {
                                    thumbnail.Save(outStream, ImageFormat.Jpeg);
                                    photoDto.Thumbnail = outStream.ToArray();
                                }
                            }
                        }
                    }

                    dicPhotos.TryAdd(photoDto.AlbumPath + photoDto.Title, photoDto);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return dicPhotos;
        }
    }
}