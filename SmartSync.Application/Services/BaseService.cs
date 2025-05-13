using SmartSync.Infraestructure.Persistence.Repositories;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class BaseService<T> : IBaseService<T> where T : BaseModel
    {
        protected readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<T> CreateAsync(T entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            return await _repository.CreateAsync(entity);
        }

        public async Task<T> UpdateAsync(Guid id, T entity)
        {
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
                return null;

            entity.Id = id;
            //entity.UpdatedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
                return false;

            return await _repository.DeleteAsync(id);
        }
    }
}
