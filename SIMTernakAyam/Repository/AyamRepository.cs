using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class AyamRepository : BaseRepository<Ayam>, IAyamRepository
    {
        public AyamRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ayam>> GetByKandangIdAsync(Guid kandangId)
        {
            return await _context.Ayams
                 .Include(a => a.Kandang)
                     .ThenInclude(k => k.User) // ? Include User
                 .Where(a => a.KandangId == kandangId)
                 .OrderByDescending(a => a.TanggalMasuk)
                 .ToListAsync();
        }

        public async Task<int> GetTotalAyamInKandangAsync(Guid kandangId)
        {
            return await _context.Ayams
                .Where(a => a.KandangId == kandangId)
                .SumAsync(a => a.JumlahMasuk);
        }

        public async Task<IEnumerable<Ayam>> GetAyamWithKandangAsync()
        {
            return await _context.Ayams
                .Include(a => a.Kandang)
                .ThenInclude(k => k.User)
                .OrderByDescending(a => a.TanggalMasuk)
                .ToListAsync();
        }

        public async Task<Ayam?> GetWithDetailsAsync(Guid id)
        {

            return await _context.Ayams
                .Include(a => a.Kandang)
                .ThenInclude(k => k.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}