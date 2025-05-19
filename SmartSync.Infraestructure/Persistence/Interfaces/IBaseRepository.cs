using Microsoft.EntityFrameworkCore;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Interfaces
{
    public interface IBaseRepository<TModel> where TModel : BaseModel
    {
        IQueryable<TModel> Get(Expression<Func<TModel, bool>>? predicate = null);

        IQueryable<TModel> Get(Guid id);

        Task<int> Insert(TModel model);

        Task<int> Update(TModel model);

        Task<int> Delete(Guid id);
    }
}
