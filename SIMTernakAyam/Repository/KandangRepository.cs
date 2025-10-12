using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class KandangRepository : BaseRepository<Kandang>, IKandangRepository
    {
        public KandangRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> IsKandangAvailableAsync(Guid kandangId, int jumlahAyamBaru)
        {
            var kandang = await _database.FindAsync(kandangId);
            if (kandang == null)
            {
                return false;
            }

            var currentAyamCount = await GetCurrentAyamCountAsync(kandangId);
            var availableCapacity = kandang.Kapasitas - currentAyamCount;

            return availableCapacity >= jumlahAyamBaru;
        }

        public async Task<int> GetCurrentAyamCountAsync(Guid kandangId)
        {
            var totalAyam = await _context.Ayams
                .Where(a => a.KandangId == kandangId)
                .SumAsync(a => a.JumlahMasuk);

            var totalMortalitas = await _context.Mortalitas
                .Where(m => m.Ayam.KandangId == kandangId)
                .SumAsync(m => m.JumlahKematian);

            var totalPanen = await _context.Panens
                .Where(p => p.Ayam.KandangId == kandangId)
                .SumAsync(p => p.JumlahEkorPanen);

            return totalAyam - totalMortalitas - totalPanen;
        }
    }
}
