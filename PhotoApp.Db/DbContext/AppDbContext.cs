using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Db.Models;

namespace PhotoApp.Db.DbContext
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<UserDto> Users { get; set; }

        public DbSet<PhotoDto> Photos { get; set; }

        public DbSet<LastUpdateDto> LastUpdates { get; set; }

        public AppDbContext(DbContextOptions options) : base(options) { }

        /// <summary>
        /// Call this at the beginning of every disk-to-database sync.
        /// Calls SaveChanges
        /// </summary>
        /// <param name="newTime">Time at which this disk-to-database sync began.</param>
        public async Task SetLastUpdateTime(DateTime newTime)
        {
            var oldTime = LastUpdates.SingleOrDefault();
            if (oldTime != null)
            {
                LastUpdates.Remove(oldTime);
            }
            LastUpdates.Add(new LastUpdateDto() { UpdateTime = newTime });
            await SaveChangesAsync();
        }

        /// <returns>The last time a disk-to-database sync began.</returns>
        public async Task<DateTime?> GetLastUpdateTime()
        {
            var lastUpdate = await LastUpdates.SingleOrDefaultAsync();
            return lastUpdate?.UpdateTime;
        }
    }
}

