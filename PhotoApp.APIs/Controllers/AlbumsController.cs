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
using PhotoApp.Utils.Models;

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
            List<AlbumModelDto> albumsModelDto = new List<AlbumModelDto>();
            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = "SELECT albums.Id as id, albums.Path as path, count(*) as nbPhotos" +
                                              " FROM Photos photos" +
                                              " INNER JOIN Albums albums on photos.AlbumId = albums.Id" +
                                              " GROUP BY albums.Path";
                        dbContext.Database.OpenConnection();
                        using (var result = command.ExecuteReader())
                        {
                            while (result.Read())
                            {
                                try
                                {
                                    var id = Convert.ToInt32(result["id"].ToString());
                                    var path = result["path"].ToString();
                                    var nbPhotos = Convert.ToInt32(result["nbPhotos"].ToString());

                                    albumsModelDto.Add(new AlbumModelDto(id, path, nbPhotos));
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, e.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            if (albumsModelDto.Count == 0)
            {
                return NoContent();
            }

            var albumModelDtos = albumsModelDto.OrderBy(x => x.Path).ToList();
            return Ok(JsonConvert.SerializeObject(albumModelDtos));

        }
    }
}
