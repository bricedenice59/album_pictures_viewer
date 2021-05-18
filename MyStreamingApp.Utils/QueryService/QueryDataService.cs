using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyStreamingApp.Utils.DbContext;
using MyStreamingApp.Utils.Models;

namespace MyStreamingApp.Utils.QueryService
{
    public class QueryDataService<T> where T : DomainObject
    {
        private readonly AppDbContextFactory _contextFac;

        public QueryDataService(AppDbContextFactory contextFac)
        {
            _contextFac = contextFac;
        }

        public async Task<IEnumerable<UserDto>> GetAll()
        {
            using (AppDbContext context = _contextFac.CreateDbContext())
            {
                IEnumerable<UserDto> entities = await context.Users
                    .ToListAsync();
                return entities;
            }
        }
    }
}
