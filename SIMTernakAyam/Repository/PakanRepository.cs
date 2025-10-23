using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class PakanRepository : BaseRepository<Pakan>, IPakanRepository
    {
        public PakanRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Pakan?> GetByNameAsync(string namaPakan)
        {
            return await _context.Pakans
                .FirstOrDefaultAsync(p => p.NamaPakan.ToLower() == namaPakan.ToLower());
        }

        public async Task<IEnumerable<Pakan>> GetLowStockAsync(int threshold = 10)
        {
            return await _context.Pakans
                .Where(p => p.StokKg <= threshold)
                .OrderBy(p => p.StokKg)
                .ToListAsync();
        }

        public async Task<bool> IsNameExistsAsync(string namaPakan, Guid? excludeId = null)
        {
            var query = _context.Pakans
                .Where(p => p.NamaPakan.ToLower() == namaPakan.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> UpdateStokAsync(Guid id, int newStok)
        {
            var pakan = await _context.Pakans.FindAsync(id);
            if (pakan == null) return false;

            pakan.StokKg = newStok;
            pakan.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStokKgAsync(Guid id, decimal newStok)
        {
            var pakan = await _context.Pakans.FindAsync(id);
            if (pakan == null) return false;

            pakan.StokKg = newStok;
            pakan.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> UpdateStokKgAsyncDirect(Guid id, decimal amountChange, DateTime tanggal)
        {
            var pakan = await _context.Pakans.FindAsync(id);
            if (pakan == null)
            {
                return (false, "Pakan tidak ditemukan.");
            }

            // Ensure the tanggal matches the month/year of the stock record
            if (pakan.Bulan != tanggal.Month || pakan.Tahun != tanggal.Year)
            {
                return (false, $"Tanggal tidak sesuai dengan periode stok pakan ({pakan.Bulan}/{pakan.Tahun}).");
            }

            // Calculate new stock
            var newStok = pakan.StokKg + amountChange;

            // Check for negative stock
            if (newStok < 0)
            {
                return (false, $"Stok pakan tidak mencukupi. Dibutuhkan: {Math.Abs(amountChange)} kg, Tersedia: {pakan.StokKg} kg.");
            }

            // Update the stock directly using raw SQL to avoid tracking conflicts
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE \"Pakans\" SET \"StokKg\" = \"StokKg\" + {0}, \"UpdateAt\" = {1} WHERE \"Id\" = {2}",
                amountChange, DateTime.UtcNow, id);

            if (rowsAffected == 0)
            {
                return (false, "Gagal mengupdate stok pakan.");
            }

            return (true, $"Sisa stok: {newStok} kg.");
        }

        public async Task<(decimal StokKg, int Bulan, int Tahun)?> GetStockInfoAsync(Guid id)
        {
            var result = await _context.Pakans
                .Where(p => p.Id == id)
                .Select(p => new { p.StokKg, p.Bulan, p.Tahun })
                .FirstOrDefaultAsync();

            if (result == null) return null;

            return (result.StokKg, result.Bulan, result.Tahun);
        }
    }
}