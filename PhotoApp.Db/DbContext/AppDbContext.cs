using Microsoft.EntityFrameworkCore;
using PhotoApp.Db.Models;

namespace PhotoApp.Db.DbContext
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<UserDto> Users { get; set; }

        public DbSet<PhotoDto> Photos { get; set; }
        
        public AppDbContext(DbContextOptions options) : base(options) { }
    }
}

