using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class VaksinRepository : BaseRepository<Vaksin>, IVaksinRepository
    {
        public VaksinRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Vaksin?> GetByNameAsync(string namaVaksin)
        {
            return await _context.Vaksins
                .FirstOrDefaultAsync(v => v.NamaVaksin.ToLower() == namaVaksin.ToLower());
        }

        public async Task<IEnumerable<Vaksin>> GetLowStockAsync(int threshold = 5)
        {
            return await _context.Vaksins
                .Where(v => v.Stok <= threshold)
                .OrderBy(v => v.Stok)
                .ToListAsync();
        }

        public async Task<bool> IsNameExistsAsync(string namaVaksin, Guid? excludeId = null)
        {
            var query = _context.Vaksins
                .Where(v => v.NamaVaksin.ToLower() == namaVaksin.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> UpdateStokAsync(Guid id, int newStok)
        {
            var vaksin = await _context.Vaksins.FindAsync(id);
            if (vaksin == null) return false;

            vaksin.Stok = newStok;
            vaksin.UpdateAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}