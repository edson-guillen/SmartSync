using Microsoft.EntityFrameworkCore;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Context;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Repositories
{
    public class BaseRepository<TModel>(SmartSyncDbContext db) : IBaseRepository<TModel> where TModel : BaseModel
    {
        protected readonly SmartSyncDbContext _db = db;

        public IQueryable<TModel> Get(Expression<Func<TModel, bool>>? predicate = null)
        {
            var query = _db.Set<TModel>().AsQueryable();
            if (predicate != null) query = query.Where(predicate);
            return query;
        }

        public IQueryable<TModel> Get(Guid id)
        {
            return Get(p => p.Id.Equals(id))
                .AsQueryable();
        }

        public virtual async Task<int> Insert(TModel model)
        {
            try
            {
                await _db.AddAsync(model);
                return await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<int> Update(TModel model)
        {
            try
            {
                _db.Update(model);
                return await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<int> Delete(Guid id)
        {
            try
            {
                TModel model = Get(id).FirstOrDefault() ?? throw new Exception("Entity not found!");
                _db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                return await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
