using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class VaksinRepository : BaseRepository<Vaksin>, IVaksinRepository
    {
        public VaksinRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Vaksin?> GetByNameAsync(string namaVaksin)
        {
            return await _context.Vaksins
                .FirstOrDefaultAsync(v => v.NamaVaksin.ToLower() == namaVaksin.ToLower());
        }

        public async Task<IEnumerable<Vaksin>> GetLowStockAsync(int threshold = 5)
        {
            return await _context.Vaksins
                .Where(v => v.Stok <= threshold)
                .OrderBy(v => v.Stok)
                .ToListAsync();
        }

        public async Task<bool> IsNameExistsAsync(string namaVaksin, Guid? excludeId = null)
        {
            var query = _context.Vaksins
                .Where(v => v.NamaVaksin.ToLower() == namaVaksin.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> UpdateStokAsync(Guid id, int newStok)
        {
            var vaksin = await _context.Vaksins.FindAsync(id);
            if (vaksin == null) return false;

            vaksin.Stok = newStok;
            vaksin.UpdateAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> UpdateStokAsyncDirect(Guid id, int amountChange, DateTime tanggal)
        {
            var vaksin = await _context.Vaksins.FindAsync(id);
            if (vaksin == null)
            {
                return (false, "Vaksin tidak ditemukan.");
            }

            // Ensure the tanggal matches the month/year of the stock record
            if (vaksin.Bulan != tanggal.Month || vaksin.Tahun != tanggal.Year)
            {
                return (false, $"Tanggal tidak sesuai dengan periode stok vaksin ({vaksin.Bulan}/{vaksin.Tahun}).");
            }

            // Calculate new stock
            var newStok = vaksin.Stok + amountChange;

            // Check for negative stock
            if (newStok < 0)
            {
                return (false, $"Stok vaksin tidak mencukupi. Dibutuhkan: {Math.Abs(amountChange)} dosis, Tersedia: {vaksin.Stok} dosis.");
            }

            // Update the stock directly using raw SQL to avoid tracking conflicts
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE \"Vaksins\" SET \"Stok\" = \"Stok\" + {0}, \"UpdateAt\" = {1} WHERE \"Id\" = {2}",
                amountChange, DateTime.UtcNow, id);

            if (rowsAffected == 0)
            {
                return (false, "Gagal mengupdate stok vaksin.");
            }

            return (true, $"Sisa stok: {newStok} dosis.");
        }

        public async Task<(int Stok, int Bulan, int Tahun)?> GetStockInfoAsync(Guid id)
        {
            var result = await _context.Vaksins
                .Where(v => v.Id == id)
                .Select(v => new { v.Stok, v.Bulan, v.Tahun })
                .FirstOrDefaultAsync();

            if (result == null) return null;

            return (result.Stok, result.Bulan, result.Tahun);
        }

        public async Task<IEnumerable<Vaksin>> GetByTypeAsync(VaksinVitaminTypeEnum tipe)
        {
            return await _context.Vaksins
                .Where(v => v.Tipe == tipe)
                .OrderBy(v => v.NamaVaksin)
                .ToListAsync();
        }
    }
}