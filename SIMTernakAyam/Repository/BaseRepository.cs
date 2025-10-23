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

        public virtual void Delete(T entity)
        {
            _database.Remove(entity);
        }

        public IEnumerable<T> FindWhere(Func<T, bool> filter)
        {
            return _database.Where(filter);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _database.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _database.FirstOrDefaultAsync(s => s.Id == id);
        }

        public virtual async Task<T?> GetByIdNoTrackingAsync(Guid id)
        {
            return await _database.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void UpdateAsync(T entity)
        {
            _database.Update(entity);
        }
    }
}
