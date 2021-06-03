﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using EFCore.BulkExtensions;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using PhotoApp.Db.QueryService;
using PhotoApp.Utils;
using File = System.IO.File;
using ImageUtils = PhotoApp.APIs.Utils.ImageUtils;

namespace PhotoApp.APIs
{

    public class LibMonitor : ILibMonitor
    {
        private readonly IMeasureTimePerformance _measureTimePerformance;
        private readonly AppDbContextFactory _dbContextFactory;
        private readonly ILogger<LibMonitor> _logger;
        private string dbName = "PhotosLibrary.db";
        private string photosFolder = "/photos";
        private string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        private System.Threading.Timer _timer;
        private static int _lockFlag = 0;
        private int chunkSize = 5;
        readonly TimeSpan startTimeSpan = TimeSpan.Zero;
        readonly TimeSpan periodTimeSpan = TimeSpan.FromHours(1);

        //this dictionary allows me to avoid querying db for retrieving foreign key in albums table everytime we add a new picture
        Dictionary<string, AlbumDto> Albums;

        public bool IsBusy { get; set; }
        public int ScanCompletionPercentage { get; set; }

        public LibMonitor(AppDbContextFactory dbContextFactory, 
            IMeasureTimePerformance measureTimePerformance,
            ILogger<LibMonitor> logger)
        {
            _dbContextFactory = dbContextFactory;
            _measureTimePerformance = measureTimePerformance;
            _logger = logger;
        }

        public void MonitorFolder()
        {
            _timer = new System.Threading.Timer(async(e) =>
            {
                _logger.LogDebug($"Thread {Thread.CurrentThread.ManagedThreadId} tries to get access to syncDB fct");

                if (Interlocked.CompareExchange(ref _lockFlag, 1, 0) == 0)
                {
                    IsBusy = true;
                    _logger.LogInformation("Monitoring photo folder...");
                    Monitor.Enter(_lockFlag);
                    _logger.LogDebug($"$Access given to Thread {Thread.CurrentThread.ManagedThreadId}");
                    _logger.LogInformation("Scan about to start...");

                    _measureTimePerformance.Init();
                    _measureTimePerformance.Start();

                    await SyncDb();
                    _measureTimePerformance.Stop();
                    _logger.LogInformation($"It took {_measureTimePerformance.GetElapsedTime()} to sync photos with database");
                    
                    // free the lock
                    Interlocked.Decrement(ref _lockFlag);
                    IsBusy = false;
                    ScanCompletionPercentage = 0;
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
            ScanCompletionPercentage = 0;
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

                var uniqueAlbums = allPhotoFiles
                    .Select(x => Path.GetDirectoryName(x))
                    .Distinct()
                    .Select(p => new AlbumDto() { Path = p })
                    .ToList();

                //first time scan
                if (!lastDbUpdateTime.HasValue)
                {
                    await dbContext.Albums.AddRangeAsync(uniqueAlbums);
                    await dbContext.SaveChangesAsync();

                    Albums = dbContext.Albums
                        .AsNoTracking()
                        .ToList()
                        .ToDictionary(x => x.Path, y => new AlbumDto() {Id = y.Id, Path = y.Path});

                    await ProcessAndSaveEntity(allPhotoFiles);
                }
                else
                {
                    List<string> added = new List<string>();
                    List<string> updated = new List<string>();
                    List<string> deleted = new List<string>();

                    var allPhotosInDb = dbContext.Photos.
                        ToDictionary(x=> Path.Combine(x.Album.Path,x.Title), y=>y);

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
                        //process new albums creation in db first
                        List<AlbumDto> newAlbums = new List<AlbumDto>();

                        //get existing list of albums in db
                        var existingAlbumsInDb = dbContext.Albums
                            .AsNoTracking()
                            .ToList()
                            .ToDictionary(x => x.Path, y => new AlbumDto() { Id = y.Id, Path = y.Path });

                        //get list of new albums to be added in db
                        var uniqueNewAlbums = added
                            .Select(x => Path.GetDirectoryName(x))
                            .Distinct()
                            .Select(p => new AlbumDto() { Path = p })
                            .ToList();

                        foreach (var uniqueNewAlbum in uniqueNewAlbums)
                        {
                            if(!existingAlbumsInDb.ContainsKey(uniqueNewAlbum.Path))
                                newAlbums.Add(uniqueNewAlbum);
                        }

                        await dbContext.Albums.AddRangeAsync(newAlbums);
                        await dbContext.SaveChangesAsync();

                        //refresh and get in sync with what has been just added in db
                        Albums = dbContext.Albums
                            .AsNoTracking()
                            .ToList()
                            .ToDictionary(x => x.Path, y => new AlbumDto() { Id = y.Id, Path = y.Path });

                        await ProcessAndSaveEntity(added);
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

        private async Task ProcessAndSaveEntity(List<string> files)
        {
            int intNbFilesToProcess = files.Count;
            var allPhotoFilesChunks = MiscUtils.BreakIntoChunks<string>(files, chunkSize);

            https://stackoverflow.com/questions/28809036/insert-a-new-entity-without-creating-child-entities-if-they-exist
            using (var newDbContext = _dbContextFactory.CreateDbContext())
            {
                for (int i = 0; i < allPhotoFilesChunks.Count; i++)
                {
                    var dicPhotos = await CreateDtoAndThumbnailAsync(allPhotoFilesChunks[i]);
                    if (dicPhotos.Count > 0)
                    {
                        foreach (var photo in dicPhotos.Values)
                        {
                            newDbContext.Entry(photo.Album).State = EntityState.Modified;
                            newDbContext.Entry(photo).State = EntityState.Added;
                            newDbContext.Set<PhotoDto>().Add(photo);
                        }

                        await newDbContext.SaveChangesAsync();
                    }
                    ScanCompletionPercentage = Convert.ToInt32(((double)(i * chunkSize) / (double)intNbFilesToProcess) * 100);
                }
            }
        }

        private async Task ProcessAndUpdateEntity(List<string> files, AppDbContextFactory dbContextFactory)
        {
            int intNbFilesToProcess = files.Count;
            var nonQueryService = new NonQueryDataService<PhotoDto>(dbContextFactory);
            var allPhotoFilesChunks = MiscUtils.BreakIntoChunks<string>(files, chunkSize);
            for (int i = 0; i < allPhotoFilesChunks.Count; i++)
            {
                var dicPhotos = await CreateDtoAndThumbnailAsync(allPhotoFilesChunks[i]);

                foreach (var photoToUpdate in dicPhotos)
                {
                    await nonQueryService.Update(photoToUpdate.Value.Id, photoToUpdate.Value);
                }
                ScanCompletionPercentage = Convert.ToInt32(((double)(i * chunkSize) / (double)intNbFilesToProcess) * 100);
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
                string file = new string(x);
                var fileInfo = new FileInfo(file);
                var filepath = Path.GetDirectoryName(file);
                PhotoDto photoDto = new PhotoDto()
                {
                    Title = Path.GetFileName(file),
                    Filesize = fileInfo.Length,
                    Date = fileInfo.CreationTimeUtc.ToString(CultureInfo.InvariantCulture),
                    PhotoExif = null
                };
                if(Albums.TryGetValue(filepath, out var album))
                {
                    photoDto.Album = album;
                }

                //try
                //{
                //    var tag = tfile?.Tag as TagLib.Image.CombinedImageTag;
                //    snapshot = tag?.DateTime ?? new FileInfo(file).CreationTimeUtc;
                //    if (tag?.Exif != null)
                //    {
                //        if (!(string.IsNullOrEmpty(root.ExifPackage.Make) &&
                //            string.IsNullOrEmpty(root.ExifPackage.Model) &&
                //            !tag.Exif.ExposureTime.HasValue &&
                //            !tag.Exif.FNumber.HasValue &&
                //            !tag.Exif.ISOSpeedRatings.HasValue))
                //            photoExif = new PhotoExif()
                //            {
                //                Manufacturer = root.ExifPackage.Make,
                //                Model = root.ExifPackage.Model,
                //                ExposureTime = tag.Exif.ExposureTime,
                //                FNumber = tag.Exif.FNumber,
                //                Iso = tag.Exif.ISOSpeedRatings
                //            };
                //    }
                //}
                //finally
                //{
                //    tfile?.Dispose();
                //}

                MagickImage image = null;
                try
                {
                    _logger.LogInformation($"Processing file {x}");

                    // Load image.
                    image = new MagickImage(file);

                    // Compute thumbnail size.
                    MagickGeometry thumbnailSize = ImageUtils.GetThumbnailSize(image);

                    if (image.Width < thumbnailSize.Width || image.Height < thumbnailSize.Height)
                    {
                        //if no shrinking is occurring, return the original bytes
                        using (var outStream = new MemoryStream())
                        {
                            await image.WriteAsync(outStream, MagickFormat.Jpg);
                            photoDto.Thumbnail = outStream.ToArray();
                        }
                    }
                    else
                    {
                        // Get thumbnail.
                        image.Thumbnail(new MagickGeometry(thumbnailSize.Width, thumbnailSize.Height));
                        using (var outStream = new MemoryStream())
                        {
                            await image.WriteAsync(outStream, MagickFormat.Jpg);
                            photoDto.Thumbnail = outStream.ToArray();
                        }
                    }

                    dicPhotos.TryAdd(photoDto.Album.Path + photoDto.Title, photoDto);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    image?.Dispose();
                    file = null;
                }
            });
            return dicPhotos;
        }
    }

    public interface ILibMonitor
    {
        public bool IsBusy { get; set; }

        public int ScanCompletionPercentage { get; set; }

        void MonitorFolder();
    }
}