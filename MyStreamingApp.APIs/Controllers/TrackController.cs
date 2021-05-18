using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyStreamingApp.Utils.DbContext;
using MyStreamingApp.Utils.Models;


namespace MyStreamingApp.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackController : ControllerBase
    {
        private readonly ILogger<TrackController> _logger;
        private readonly AppDbContextFactory _dbContextFactory;

        public TrackController(AppDbContextFactory dbContextFactory, ILogger<TrackController> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<TrackDto> All()
        {
            using (AppDbContext context = _dbContextFactory.CreateDbContext())
            {
                return context.Tracks.ToList();
            }
        }
    }
}
