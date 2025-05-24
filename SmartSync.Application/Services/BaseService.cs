using System.Linq.Expressions;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Repositories;
using SmartSync.Infraestructure.Persistence.Interfaces;
using SmartSync.Application.Interfaces;
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Domain.Events;

namespace SmartSync.Application.Services
{
    public class BaseService<TModel>(IBaseRepository<TModel> repository, IRabbitMqPublisher publisher) : IBaseService<TModel> where TModel : BaseModel
    {
        protected readonly IBaseRepository<TModel> _repostitory = repository;
        protected readonly IRabbitMqPublisher _publisher = publisher;   
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
            var result = await _repostitory.Insert(model);
            if (result > 0)
            {
                // Publique um evento para a fila (por exemplo, "entity.created")
                await _publisher.PublishAsync("entity.created", new EntityCreatedEvent<TModel>(model));
            }
            return result;
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