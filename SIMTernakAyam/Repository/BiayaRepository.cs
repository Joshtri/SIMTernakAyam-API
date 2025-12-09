using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Repository
{
    public class BiayaRepository : BaseRepository<Biaya>, IBiayaRepository
    {
        public BiayaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Biaya>> GetAllAsync()
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Kandang)
                .Include(b => b.Operasional)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public override async Task<Biaya?> GetByIdAsync(Guid id)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Kandang)
                .Include(b => b.Operasional)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Biaya>> GetByPetugasIdAsync(Guid petugasId)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Where(b => b.PetugasId == petugasId)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biaya>> GetByOperasionalIdAsync(Guid operasionalId)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Where(b => b.OperasionalId == operasionalId)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biaya>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Where(b => b.Tanggal >= startDate && b.Tanggal <= endDate)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biaya>> GetByJenisBiayaAsync(string jenisBiaya)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Where(b => b.JenisBiaya.ToLower() == jenisBiaya.ToLower())
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalBiayaByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Biayas
                .Where(b => b.Tanggal >= startDate && b.Tanggal <= endDate)
                .SumAsync(b => b.Jumlah);
        }

        public async Task<IEnumerable<Biaya>> GetWithDetailsAsync()
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Include(b => b.Kandang)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biaya>> GetByBulanTahunAsync(int bulan, int tahun)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Include(b => b.Kandang)
                .Where(b => b.Bulan == bulan && b.Tahun == tahun)
                .OrderBy(b => b.KandangId)
                .ThenBy(b => b.JenisBiaya)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biaya>> GetByKandangBulanTahunAsync(Guid kandangId, int bulan, int tahun)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Include(b => b.Kandang)
                .Where(b => b.KandangId == kandangId && b.Bulan == bulan && b.Tahun == tahun)
                .OrderBy(b => b.JenisBiaya)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biaya>> GetByKandangIdAsync(Guid kandangId)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Include(b => b.Kandang)
                .Where(b => b.KandangId == kandangId)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<Biaya?> GetSingleByOperasionalIdAsync(Guid operasionalId)
        {
            return await _context.Biayas
                .AsNoTracking()
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Include(b => b.Kandang)
                .FirstOrDefaultAsync(b => b.OperasionalId == operasionalId);
        }

        public async Task<IEnumerable<Biaya>> GetByKategoriBiayaAsync(KategoriBiayaEnum kategori)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Include(b => b.Kandang)
                .Where(b => b.KategoriBiaya == kategori)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biaya>> GetByKategoriBiayaAndDateRangeAsync(KategoriBiayaEnum kategori, DateTime startDate, DateTime endDate)
        {
            return await _context.Biayas
                .Include(b => b.Petugas)
                .Include(b => b.Operasional)
                .Include(b => b.Kandang)
                .Where(b => b.KategoriBiaya == kategori && b.Tanggal >= startDate && b.Tanggal <= endDate)
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalBiayaByKategoriBiayaAndDateRangeAsync(KategoriBiayaEnum kategori, DateTime startDate, DateTime endDate)
        {
            return await _context.Biayas
                .Where(b => b.KategoriBiaya == kategori && b.Tanggal >= startDate && b.Tanggal <= endDate)
                .SumAsync(b => b.Jumlah);
        }
    }
}
