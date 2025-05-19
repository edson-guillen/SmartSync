using SmartSync.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SmartSync.Application.Interfaces
{
    public interface IBaseService<TModel> where TModel : BaseModel
    {
        IQueryable<TModel> Get(Expression<Func<TModel, bool>>? predicate = null);

        IQueryable<TModel> Get(Guid id);

        Task<int> Insert(TModel model);

        Task<int> Update(TModel model);

        Task<int> Delete(Guid id);
    }
}