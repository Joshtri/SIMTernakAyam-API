using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class MortalitasRepository : BaseRepository<Mortalitas>, IMortalitasRepository
    {
        public MortalitasRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Mortalitas>> GetByAyamIdAsync(Guid ayamId)
        {
            return await _context.Mortalitas
                .Include(m => m.Ayam)
                .ThenInclude(a => a.Kandang)
                .Where(m => m.AyamId == ayamId)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        public async Task<IEnumerable<Mortalitas>> GetByKandangIdAsync(Guid kandangId)
        {
            return await _context.Mortalitas
                .Include(m => m.Ayam)
                .ThenInclude(a => a.Kandang)
                .Where(m => m.Ayam.KandangId == kandangId)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        public async Task<IEnumerable<Mortalitas>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Mortalitas
                .Include(m => m.Ayam)
                .ThenInclude(a => a.Kandang)
                .Where(m => m.TanggalKematian >= startDate && m.TanggalKematian <= endDate)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        public async Task<IEnumerable<Mortalitas>> GetWithDetailsAsync()
        {
            return await _context.Mortalitas
                .Include(m => m.Ayam)
                .ThenInclude(a => a.Kandang)
                .ThenInclude(k => k.User)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        public async Task<Mortalitas?> GetWithDetailsAsync(Guid id)
        {
            return await _context.Mortalitas
                .Include(m => m.Ayam)
                .ThenInclude(a => a.Kandang)
                .ThenInclude(k => k.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int> GetTotalMortalitasByKandangAsync(Guid kandangId)
        {
            return await _context.Mortalitas
                .Include(m => m.Ayam)
                .Where(m => m.Ayam.KandangId == kandangId)
                .SumAsync(m => m.JumlahKematian);
        }

        public async Task<int> GetTotalMortalitasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Mortalitas
                .Where(m => m.TanggalKematian >= startDate && m.TanggalKematian <= endDate)
                .SumAsync(m => m.JumlahKematian);
        }
    }
}