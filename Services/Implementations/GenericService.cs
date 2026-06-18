#nullable enable
using Warehouse.Repositories.Interfaces;
using Warehouse.Services.Interfaces;

namespace Warehouse.Services.Implementations
{
    public class GenericService<TDto, TEntity> : IGenericService<TDto, TEntity>
        where TEntity : class
        where TDto : class
    {
        protected readonly IGenericRepository<TEntity> _repository;

        public GenericService(IGenericRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public virtual async Task<TDto> CreateAsync(TDto dto)
        {
            // map dto to entity in concrete implementations
            throw new NotImplementedException();
        }

        public virtual Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEnumerable<TDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task<TDto?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TDto?> UpdateAsync(int id, TDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
