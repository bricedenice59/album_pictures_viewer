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
using PhotoApp.Utils.Models;

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
        public async Task<ActionResult<PhotoApp.Db.Models.PhotoDto>> Get(string photoTitle)
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
        public ActionResult<string> GetPhotosForAlbumId(
            [FromQuery] int albumId, 
            [FromQuery] int pageNumber,
            [FromQuery] int nbPhotosForAlbumId)
        {
            var numberOfRecordToskip = 0;

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                if (nbPhotosForAlbumId > PageSize)
                    numberOfRecordToskip = pageNumber * PageSize;

                var photos = dbContext.Photos
                    .AsNoTracking()
                    .Include(x=>x.Album)
                    .ToList()
                    .Where(x=>x.Album.Id == albumId)
                    .OrderBy(x => x.Id)
                    .Skip(numberOfRecordToskip)
                    .Take(PageSize)
                    .Select(s => new PhotoApp.Utils.Models.PhotoDto
                    {
                        Title = s.Title, 
                        Date = s.Date,
                        Filesize = s.Filesize,
                        Thumbnail = s.Thumbnail
                    })
                    .ToList();

                if (photos.Any())
                {
                    return JsonConvert.SerializeObject(new PhotosModelDto(photos));
                }

                return NoContent();
            }
        }
    }
}
