using System.Collections.Generic;
using System.Threading.Tasks;
using EFSqliteDOTNET50.Utils.DbContext;
using EFSqliteDOTNET50.Utils.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EFSqliteDOTNET50.Utils.QueryService
{
    public class QueryDataService<T> where T : DomainObject
    {
        private readonly SimpleDbContextFactory _contextFactory;

        public QueryDataService(SimpleDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            using (SimpleDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<User> entities = await context.Users
                    .ToListAsync();
                return entities;
            }
        }
    }
}
