using System.Linq.Expressions;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Repositories;
using SmartSync.Infraestructure.Persistence.Interfaces;
using SmartSync.Application.Interfaces;

namespace SmartSync.Application.Services
{
    public class BaseService<TModel>(IBaseRepository<TModel> repository) : IBaseService<TModel> where TModel : BaseModel
    {
        protected readonly IBaseRepository<TModel> _repostitory = repository;

        public IQueryable<TModel> Get(Expression<Func<TModel, bool>>? predicate = null)
        {
            return _repostitory.Get(predicate);
        }

        public IQueryable<TModel> Get(Guid id)
        {
            return _repostitory.Get(id);
        }

        public virtual async Task<int> Insert(TModel model)
        {
            return await _repostitory.Insert(model);
        }

        public virtual async Task<int> Update(TModel model)
        {
            return await _repostitory.Update(model);
        }

        public virtual async Task<int> Delete(Guid id)
        {
            return await _repostitory.Delete(id);
        }
    }
}