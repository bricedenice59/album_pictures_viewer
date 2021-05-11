using EntityFrameworkDotNet50.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkDotNet50
{
    public class SimpleDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public SimpleDbContext(DbContextOptions options) : base(options) { }
    }
}

