using EFSqliteDOTNET50.Utils.Models;
using Microsoft.EntityFrameworkCore;

namespace EFSqliteDOTNET50.Utils.DbContext
{
    public class SimpleDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<User> Users { get; set; }

        public SimpleDbContext(DbContextOptions options) : base(options) { }
    }
}

