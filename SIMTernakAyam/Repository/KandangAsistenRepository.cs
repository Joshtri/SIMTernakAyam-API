using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class KandangAsistenRepository : BaseRepository<KandangAsisten>, IKandangAsistenRepository
    {
        public KandangAsistenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<KandangAsisten>> GetAsistensByKandangIdAsync(Guid kandangId)
        {
            return await _database
                .Include(ka => ka.Asisten)
                .Include(ka => ka.Kandang)
                .Where(ka => ka.KandangId == kandangId)
                .OrderByDescending(ka => ka.IsAktif)
                .ThenBy(ka => ka.Asisten.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<KandangAsisten>> GetKandangsByAsistenIdAsync(Guid asistenId)
        {
            return await _database
                .Include(ka => ka.Asisten)
                .Include(ka => ka.Kandang)
                .Where(ka => ka.AsistenId == asistenId)
                .OrderByDescending(ka => ka.IsAktif)
                .ThenBy(ka => ka.Kandang.NamaKandang)
                .ToListAsync();
        }

        public async Task<bool> IsAsistenExistsInKandangAsync(Guid kandangId, Guid asistenId, Guid? excludeId = null)
        {
            var query = _database.Where(ka => ka.KandangId == kandangId && ka.AsistenId == asistenId);

            if (excludeId.HasValue)
            {
                query = query.Where(ka => ka.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<KandangAsisten>> GetActiveAsistensByKandangIdAsync(Guid kandangId)
        {
            return await _database
                .Include(ka => ka.Asisten)
                .Include(ka => ka.Kandang)
                .Where(ka => ka.KandangId == kandangId && ka.IsAktif)
                .OrderBy(ka => ka.Asisten.FullName)
                .ToListAsync();
        }

        public async Task<KandangAsisten?> GetWithDetailsAsync(Guid id)
        {
            return await _database
                .Include(ka => ka.Kandang)
                .Include(ka => ka.Asisten)
                .FirstOrDefaultAsync(ka => ka.Id == id);
        }

        public async Task<IEnumerable<KandangAsisten>> GetAllWithDetailsAsync()
        {
            return await _database
                .Include(ka => ka.Kandang)
                .Include(ka => ka.Asisten)
                .OrderBy(ka => ka.Kandang.NamaKandang)
                .ThenBy(ka => ka.Asisten.FullName)
                .ToListAsync();
        }
    }
}
