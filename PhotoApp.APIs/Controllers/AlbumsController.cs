using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AlbumsController : ControllerBase
    {
        private readonly ILogger<AlbumsController> _logger;
        private readonly AppDbContextFactory _dbContextFactory;

        public AlbumsController(AppDbContextFactory dbContextFactory, ILogger<AlbumsController> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<string> GetAll()
        {
            List<AlbumDto> albumsLst = null;
            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    albumsLst = dbContext.Albums.AsNoTracking().ToList();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            if (albumsLst == null)
            {
                return NoContent();
            }
            else
            {
                return Ok(JsonConvert.SerializeObject(albumsLst));
            }
        }
    }
}
