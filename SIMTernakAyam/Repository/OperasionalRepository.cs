using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class OperasionalRepository : BaseRepository<Operasional>, IOperasionalRepository
    {
        public OperasionalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Operasional>> GetByKandangIdAsync(Guid kandangId)
        {
            return await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Kandang)
                .Include(o => o.Pakan)
                .Include(o => o.Vaksin)
                .Where(o => o.KandangId == kandangId)
                .OrderByDescending(o => o.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Operasional>> GetByPetugasIdAsync(Guid petugasId)
        {
            return await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Kandang)
                .Include(o => o.Pakan)
                .Include(o => o.Vaksin)
                .Where(o => o.PetugasId == petugasId)
                .OrderByDescending(o => o.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Operasional>> GetByJenisKegiatanIdAsync(Guid jenisKegiatanId)
        {
            return await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Kandang)
                .Include(o => o.Pakan)
                .Include(o => o.Vaksin)
                .Where(o => o.JenisKegiatanId == jenisKegiatanId)
                .OrderByDescending(o => o.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Operasional>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Kandang)
                .Include(o => o.Pakan)
                .Include(o => o.Vaksin)
                .Where(o => o.Tanggal >= startDate && o.Tanggal <= endDate)
                .OrderByDescending(o => o.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Operasional>> GetWithDetailsAsync()
        {
            return await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Kandang)
                .Include(o => o.Pakan)
                .Include(o => o.Vaksin)
                .OrderByDescending(o => o.Tanggal)
                .ToListAsync();
        }

        public async Task<Operasional?> GetWithDetailsAsync(Guid id)
        {
            return await _context.Operasionals
                .Include(o => o.JenisKegiatan)
                .Include(o => o.Petugas)
                .Include(o => o.Kandang)
                .Include(o => o.Pakan)
                .Include(o => o.Vaksin)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Operasional>> GetByVaksinIdAndPeriodAsync(Guid vaksinId, DateTime startDate, DateTime endDate)
        {
            return await _context.Operasionals
                .Where(o => o.VaksinId == vaksinId && 
                           o.Tanggal >= startDate && 
                           o.Tanggal <= endDate)
                .OrderByDescending(o => o.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Operasional>> GetByPakanIdAndPeriodAsync(Guid pakanId, DateTime startDate, DateTime endDate)
        {
            return await _context.Operasionals
                .Where(o => o.PakanId == pakanId && 
                           o.Tanggal >= startDate && 
                           o.Tanggal <= endDate)
                .OrderByDescending(o => o.Tanggal)
                .ToListAsync();
        }
    }
}