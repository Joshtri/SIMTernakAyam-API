using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class JurnalHarianRepository : BaseRepository<JurnalHarian>, IJurnalHarianRepository
    {
        public JurnalHarianRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<JurnalHarian>> GetByPetugasIdAsync(Guid petugasId, int page = 1, int pageSize = 10)
        {
            return await _context.Set<JurnalHarian>()
                .Include(j => j.Petugas)
                .Include(j => j.Kandang)
                .Where(j => j.PetugasId == petugasId)
                .OrderByDescending(j => j.Tanggal)
                .ThenByDescending(j => j.WaktuMulai)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<JurnalHarian>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? petugasId = null)
        {
            var query = _context.Set<JurnalHarian>()
                .Include(j => j.Petugas)
                .Include(j => j.Kandang)
                .Where(j => j.Tanggal >= startDate && j.Tanggal <= endDate);

            if (petugasId.HasValue)
            {
                query = query.Where(j => j.PetugasId == petugasId.Value);
            }

            return await query
                .OrderByDescending(j => j.Tanggal)
                .ThenByDescending(j => j.WaktuMulai)
                .ToListAsync();
        }

        public async Task<List<JurnalHarian>> GetByKandangIdAsync(Guid kandangId, int page = 1, int pageSize = 10)
        {
            return await _context.Set<JurnalHarian>()
                .Include(j => j.Petugas)
                .Include(j => j.Kandang)
                .Where(j => j.KandangId == kandangId)
                .OrderByDescending(j => j.Tanggal)
                .ThenByDescending(j => j.WaktuMulai)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByPetugasIdAsync(Guid petugasId)
        {
            return await _context.Set<JurnalHarian>()
                .CountAsync(j => j.PetugasId == petugasId);
        }

        public async Task Delete(Guid id)
        {
            var jurnal = await _context.Set<JurnalHarian>().FindAsync(id);
            if (jurnal != null)
            {
                _context.Set<JurnalHarian>().Remove(jurnal);
                await _context.SaveChangesAsync();
            }
        }
    }
}
