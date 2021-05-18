using Microsoft.EntityFrameworkCore;
using MyStreamingApp.Utils.Models;

namespace MyStreamingApp.Utils.DbContext
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<UserDto> Users { get; set; }

        public DbSet<Track> Tracks { get; set; }

        public DbSet<AlbumDto> Albums { get; set; }

        public AppDbContext(DbContextOptions options) : base(options) { }
    }
}

