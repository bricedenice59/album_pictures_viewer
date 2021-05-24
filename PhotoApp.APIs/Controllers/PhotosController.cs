using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        public PhotosController(AppDbContextFactory dbContextFactory, ILibMonitor iLibMonitor, ILogger<PhotosController> logger)
        {
            _dbContextFactory = dbContextFactory;
            _iLibMonitor = iLibMonitor;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<string> ScanCompletion()
        {
            var myData = new
            {
                IsBusy = _iLibMonitor.IsBusy,
                PercentageCompletion = _iLibMonitor.ScanCompletionPercentage
            };
            return JsonConvert.SerializeObject(myData);
        }
    }
}
