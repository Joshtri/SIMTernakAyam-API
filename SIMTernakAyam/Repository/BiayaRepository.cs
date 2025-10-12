using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class BiayaRepository : BaseRepository<Biaya>, IBiayaRepository
    {
        public BiayaRepository(ApplicationDbContext context) : base(context)
        {
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
                .OrderByDescending(b => b.Tanggal)
                .ToListAsync();
        }
    }
}