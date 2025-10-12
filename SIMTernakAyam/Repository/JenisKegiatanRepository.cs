using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class JenisKegiatanRepository : BaseRepository<JenisKegiatan>, IJenisKegiatanRepository
    {
        public JenisKegiatanRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<JenisKegiatan?> GetByNameAsync(string namaKegiatan)
        {
            return await _context.JenisKegiatans
                .FirstOrDefaultAsync(jk => jk.NamaKegiatan.ToLower() == namaKegiatan.ToLower());
        }

        public async Task<IEnumerable<JenisKegiatan>> GetBySatuanAsync(string satuan)
        {
            return await _context.JenisKegiatans
                .Where(jk => jk.Satuan != null && jk.Satuan.ToLower() == satuan.ToLower())
                .OrderBy(jk => jk.NamaKegiatan)
                .ToListAsync();
        }

        public async Task<bool> IsNameExistsAsync(string namaKegiatan, Guid? excludeId = null)
        {
            var query = _context.JenisKegiatans
                .Where(jk => jk.NamaKegiatan.ToLower() == namaKegiatan.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(jk => jk.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}