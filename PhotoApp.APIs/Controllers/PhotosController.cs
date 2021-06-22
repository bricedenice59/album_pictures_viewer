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
using PhotoDto = PhotoApp.Db.Models.PhotoDto;

namespace PhotoApp.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly ILogger<PhotosController> _logger;
        private readonly AppDbContextFactory _dbContextFactory;
        private readonly ILibMonitor _iLibMonitor;
        private const int PageSize = 15;

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

        [HttpGet("GetThumbnailBigVersion")]
        [Authorize]
        public async Task<ActionResult<byte[]>> GetThumbnailBigVersion([FromQuery] string photoPath, [FromQuery] string albumId)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var photoItem = await dbContext.Photos
                    .Include(y=>y.Album)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x=>x.Title.Equals(photoPath) && x.Album.Id == Convert.ToInt32(albumId));

                if (photoItem == null)
                {
                    return NotFound();
                }
                else
                {
                    var img = $"data:image/png;base64,{Convert.ToBase64String(photoItem.Thumbnail500px)}";
                    return Ok(img);
                }
            }
        }

        [HttpGet("GetPhotosForAlbumId")]
        [Authorize]
        public async Task<ActionResult<string>> GetPhotosForAlbumId(
            [FromQuery] int albumId, 
            [FromQuery] int pageNumber,
            [FromQuery] int nbPhotosForAlbumId)
        {
            var numberOfRecordToskip = 0;
            List<PhotoApp.Utils.Models.PhotoDto> lstPhotos = new List<PhotoApp.Utils.Models.PhotoDto>();

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                if (nbPhotosForAlbumId > PageSize)
                    numberOfRecordToskip = pageNumber * PageSize;

                var photos = dbContext.Photos
                    .AsNoTracking()
                    .Include(x=>x.Album)
                    .Where(x=>x.Album.Id == albumId)
                    .OrderBy(x => x.Id)
                    .Skip(numberOfRecordToskip)
                    .Take(PageSize)
                    .Select(s => new PhotoApp.Utils.Models.PhotoDto
                    {
                        Title = s.Title, 
                        Date = s.Date,
                        Filesize = s.Filesize,
                        Thumbnail = s.Thumbnail200px
                    })
                    .AsAsyncEnumerable();

                var e = photos.GetAsyncEnumerator();

                try
                {
                    while (await e.MoveNextAsync())
                    {
                        e.Current.ImgDataURL = $"data:image/png;base64,{Convert.ToBase64String(e.Current.Thumbnail)}";
                        lstPhotos.Add(e.Current);
                    }
                }
                finally { await e.DisposeAsync(); }

                return JsonConvert.SerializeObject(new PhotosModelDto(lstPhotos));
            }
        }
    }
}
