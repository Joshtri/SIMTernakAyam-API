using SIMTernakAyam.Models;
namespace SIMTernakAyam.Repositories.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByIdNoTrackingAsync(Guid id);

        IEnumerable<T> FindWhere(Func<T,bool> filter);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void UpdateAsync(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
        
        // ✅ Add entity state management methods to prevent tracking conflicts
        void DetachEntity(T entity);
        void ClearTrackedEntities();
    }
}
