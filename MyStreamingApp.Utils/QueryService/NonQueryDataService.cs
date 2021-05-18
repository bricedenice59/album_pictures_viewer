using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MyStreamingApp.Utils.DbContext;
using MyStreamingApp.Utils.Models;

namespace MyStreamingApp.Utils.QueryService
{
    public class NonQueryDataService<T> where T : DomainObject
    {
        private readonly AppDbContextFactory _contextFac;

        public NonQueryDataService(AppDbContextFactory contextFac)
        {
            _contextFac = contextFac;
        }

        public async Task<T> Create(T entity)
        {
            using (AppDbContext context = _contextFac.CreateDbContext())
            {
                EntityEntry<T> createdResult = await context.Set<T>().AddAsync(entity);
                await context.SaveChangesAsync();

                return createdResult.Entity;
            }
        }

        public async Task<T> Update(int id, T entity)
        {
            using (AppDbContext context = _contextFac.CreateDbContext())
            {
                entity.Id = id;
                context.Set<T>().Update(entity);
                await context.SaveChangesAsync();

                return entity;
            }
        }

        public async Task<bool> Delete(int id)
        {
            using (AppDbContext context = _contextFac.CreateDbContext())
            {
                T entity = await context.Set<T>().FirstOrDefaultAsync((e) => e.Id == id);
                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync();

                return true;
            }
        }
    }
}
