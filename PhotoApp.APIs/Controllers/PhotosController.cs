using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;

namespace PhotoApp.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly ILogger<PhotosController> _logger;
        private readonly AppDbContextFactory _dbContextFactory;
        private readonly ILibMonitor _iLibMonitor;
        private const int PageSize = 20;

        public PhotosController(AppDbContextFactory dbContextFactory, ILibMonitor iLibMonitor, ILogger<PhotosController> logger)
        {
            _dbContextFactory = dbContextFactory;
            _iLibMonitor = iLibMonitor;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<string> ScanCompletion()
        {
            var myData = new
            {
                IsBusy = _iLibMonitor.IsBusy,
                PercentageCompletion = _iLibMonitor.ScanCompletionPercentage
            };
            return JsonConvert.SerializeObject(myData);
        }

        [HttpGet]
        [Route("{photoTitle}")]
        [Authorize]
        public async Task<ActionResult<PhotoDto>> Get(string photoTitle)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var photoItem = await dbContext.Photos.
                    FirstOrDefaultAsync(x=>x.Title.Equals(photoTitle));

                if (photoItem == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(photoItem);
                }
            }
        }

        [HttpGet("GetPhotosForAlbumId")]
        [Authorize]
        public ActionResult<string> GetPhotosForAlbumId([FromQuery] int albumId, [FromQuery] int pageNumber)
        {
            var numberOfRecordToskip = 0;

            Thread.Sleep(10000);

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var nbPhotosForAlbumId = dbContext.Photos
                    .Include(x => x.Album)
                    .Count(x => x.Album.Id == albumId);

                if (nbPhotosForAlbumId > PageSize)
                    numberOfRecordToskip = pageNumber * PageSize;

                var photos = dbContext.Photos
                    .Include(x=>x.Album)
                    .ToList()
                    .Where(x=>x.Album.Id == albumId)
                    .OrderBy(x => x.Id)
                    .Skip(numberOfRecordToskip)
                    .Take(PageSize).ToList();

                if (photos.Any())
                {
                    return JsonConvert.SerializeObject(photos);
                }

                return NoContent();
            }
        }
    }
}
