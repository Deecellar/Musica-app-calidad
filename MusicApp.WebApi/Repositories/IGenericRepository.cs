using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicApp.WebApi.Repositories
{
    public interface IGenericRepositoryAsync<T,TId> where T : class
    {
        
        Task<T> GetByIdAsync(TId id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetPagedResponsesAsync(int pageNumber, int pageSize);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}