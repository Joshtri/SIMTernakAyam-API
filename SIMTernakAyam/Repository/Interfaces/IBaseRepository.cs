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
        
        /// <summary>
        /// ⭐ HARD DELETE: Permanently remove entity from database
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// ⭐ SOFT DELETE: Mark entity as deleted (IsDeleted = true) - DEPRECATED
        /// </summary>
        void SoftDelete(T entity);

        /// <summary>
        /// ⭐ RESTORE: Restore soft-deleted entity (IsDeleted = false)
        /// </summary>
        void Restore(T entity);
        
        Task SaveChangesAsync();
        
        // ✅ Add entity state management methods to prevent tracking conflicts
        void DetachEntity(T entity);
        void ClearTrackedEntities();
    }
}
