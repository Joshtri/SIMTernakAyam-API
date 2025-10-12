using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class PanenRepository : BaseRepository<Panen>, IPanenRepository
    {
        public PanenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Panen>> GetByAyamIdAsync(Guid ayamId)
        {
            return await _context.Panens
                .Include(p => p.Ayam)
                .ThenInclude(a => a.Kandang)
                .Where(p => p.AyamId == ayamId)
                .OrderByDescending(p => p.TanggalPanen)
                .ToListAsync();
        }

        public async Task<IEnumerable<Panen>> GetByKandangIdAsync(Guid kandangId)
        {
            return await _context.Panens
                .Include(p => p.Ayam)
                .ThenInclude(a => a.Kandang)
                .Where(p => p.Ayam.KandangId == kandangId)
                .OrderByDescending(p => p.TanggalPanen)
                .ToListAsync();
        }

        public async Task<IEnumerable<Panen>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Panens
                .Include(p => p.Ayam)
                .ThenInclude(a => a.Kandang)
                .Where(p => p.TanggalPanen >= startDate && p.TanggalPanen <= endDate)
                .OrderByDescending(p => p.TanggalPanen)
                .ToListAsync();
        }

        public async Task<IEnumerable<Panen>> GetWithDetailsAsync()
        {
            return await _context.Panens
                .Include(p => p.Ayam)
                .ThenInclude(a => a.Kandang)
                .ThenInclude(k => k.User)
                .OrderByDescending(p => p.TanggalPanen)
                .ToListAsync();
        }

        public async Task<Panen?> GetWithDetailsAsync(Guid id)
        {
            return await _context.Panens
                .Include(p => p.Ayam)
                .ThenInclude(a => a.Kandang)
                .ThenInclude(k => k.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> GetTotalEkorPanenByKandangAsync(Guid kandangId)
        {
            return await _context.Panens
                .Include(p => p.Ayam)
                .Where(p => p.Ayam.KandangId == kandangId)
                .SumAsync(p => p.JumlahEkorPanen);
        }

        public async Task<decimal> GetTotalBeratPanenByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var panens = await _context.Panens
                .Where(p => p.TanggalPanen >= startDate && p.TanggalPanen <= endDate)
                .ToListAsync();

            return panens.Sum(p => p.JumlahEkorPanen * p.BeratRataRata);
        }
    }
}