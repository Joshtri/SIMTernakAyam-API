using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseModel
    {

        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _database;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _database = _context.Set<T>();
        }
        public virtual async Task AddAsync(T entity)
        {
            await _database.AddAsync(entity);
        }

        /// <summary>
        /// ⭐ HARD DELETE: Benar-benar hapus entity dari database
        /// </summary>
        public virtual void Delete(T entity)
        {
            // ⭐ HARD DELETE: Remove entity from database
            _database.Remove(entity);
        }

        /// <summary>
        /// ⭐ SOFT DELETE: Mark entity as deleted instead of removing it
        /// (DEPRECATED - Gunakan Delete() untuk hard delete)
        /// </summary>
        public virtual void SoftDelete(T entity)
        {
            // Mark as deleted
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            // Update the entity state
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _database.Attach(entity);
            }
            entry.State = EntityState.Modified;
        }

        public IEnumerable<T> FindWhere(Func<T, bool> filter)
        {
            return _database.Where(filter);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            // ⭐ Filter out soft-deleted records
            return await _database.Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            // ⭐ Filter out soft-deleted records
            return await _database.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public virtual async Task<T?> GetByIdNoTrackingAsync(Guid id)
        {
            // ⭐ Filter out soft-deleted records
            return await _database.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void UpdateAsync(T entity)
        {
            // ✅ ENHANCED: Better entity state management
            var entry = _context.Entry(entity);
            
            if (entry.State == EntityState.Detached)
            {
                // Entity is not tracked, use Update to start tracking
                _database.Update(entity);
            }
            else
            {
                // Entity is already tracked, just mark as modified
                entry.State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Detach entity from context to prevent tracking conflicts
        /// </summary>
        public virtual void DetachEntity(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Detached;
            }
        }

        /// <summary>
        /// Clear all tracked entities for this type
        /// </summary>
        public virtual void ClearTrackedEntities()
        {
            var entries = _context.ChangeTracker.Entries<T>()
                .Where(e => e.State != EntityState.Detached)
                .ToList();
            
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }


        /// <summary>
        /// ⭐ RESTORE: Restore soft-deleted entity
        /// </summary>
        public virtual void Restore(T entity)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _database.Attach(entity);
            }
            entry.State = EntityState.Modified;
        }
    }
}
