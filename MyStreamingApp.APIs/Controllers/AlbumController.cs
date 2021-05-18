using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyStreamingApp.Utils.Models;


namespace MyStreamingApp.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumController : ControllerBase
    {
        private readonly ILogger<AlbumController> _logger;

        public AlbumController(ILogger<AlbumController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<AlbumDto> All()
        {
            return new List<AlbumDto>().ToArray();
        }
    }
}
