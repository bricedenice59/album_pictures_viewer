using Microsoft.EntityFrameworkCore;
using MyStreamingApp.Utils.Models;

namespace MyStreamingApp.Utils.DbContext
{
    public class SimpleDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<User> Users { get; set; }

        public SimpleDbContext(DbContextOptions options) : base(options) { }
    }
}

