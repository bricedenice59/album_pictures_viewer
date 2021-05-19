using System;
using Microsoft.EntityFrameworkCore;

namespace PhotoApp.Db.DbContext
{
    public class AppDbContextFactory
    {
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public AppDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext)
        {
            _configureDbContext = configureDbContext;
        }

        public AppDbContext CreateDbContext(string connectionString=null)
        {
            DbContextOptionsBuilder<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>();

            _configureDbContext(options);

            return new AppDbContext(options.Options);
        }
    }
}
