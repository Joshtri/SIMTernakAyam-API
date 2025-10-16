using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class PakanRepository : BaseRepository<Pakan>, IPakanRepository
    {
        public PakanRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Pakan?> GetByNameAsync(string namaPakan)
        {
            return await _context.Pakans
                .FirstOrDefaultAsync(p => p.NamaPakan.ToLower() == namaPakan.ToLower());
        }

        public async Task<IEnumerable<Pakan>> GetLowStockAsync(int threshold = 10)
        {
            return await _context.Pakans
                .Where(p => p.StokKg <= threshold)
                .OrderBy(p => p.StokKg)
                .ToListAsync();
        }

        public async Task<bool> IsNameExistsAsync(string namaPakan, Guid? excludeId = null)
        {
            var query = _context.Pakans
                .Where(p => p.NamaPakan.ToLower() == namaPakan.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> UpdateStokAsync(Guid id, int newStok)
        {
            var pakan = await _context.Pakans.FindAsync(id);
            if (pakan == null) return false;

            pakan.StokKg = newStok;
            pakan.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}