using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public PhotosController(AppDbContextFactory dbContextFactory, ILogger<PhotosController> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<PhotoDto> All()
        {
            using (AppDbContext context = _dbContextFactory.CreateDbContext())
            {
                return context.Photos.ToList();
            }
        }
    }
}
