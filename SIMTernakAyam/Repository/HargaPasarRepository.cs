using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Data;

namespace SIMTernakAyam.Repository
{
    /// <summary>
    /// Repository untuk operasi data harga pasar ayam
    /// </summary>
    public class HargaPasarRepository : BaseRepository<HargaPasar>, IHargaPasarRepository
    {
        public HargaPasarRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<HargaPasar?> GetHargaAktifByTanggalAsync(DateTime tanggal)
        {
            return await _context.HargaPasar
                .Where(h => h.IsAktif && 
                           h.TanggalMulai <= tanggal && 
                           (h.TanggalBerakhir == null || h.TanggalBerakhir >= tanggal))
                .OrderByDescending(h => h.TanggalMulai)
                .FirstOrDefaultAsync();
        }

        public async Task<HargaPasar?> GetHargaTerbaruAsync()
        {
            return await _context.HargaPasar
                .Where(h => h.IsAktif)
                .OrderByDescending(h => h.TanggalMulai)
                .ThenByDescending(h => h.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<HargaPasar>> GetRiwayatHargaAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.HargaPasar
                .Where(h => h.TanggalMulai >= startDate && h.TanggalMulai <= endDate)
                .OrderByDescending(h => h.TanggalMulai)
                .ToListAsync();
        }

        public async Task<bool> DeactivateAllAsync()
        {
            try
            {
                var activeHarga = await _context.HargaPasar
                    .Where(h => h.IsAktif)
                    .ToListAsync();

                foreach (var harga in activeHarga)
                {
                    harga.IsAktif = false;
                    harga.TanggalBerakhir = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStatusAsync(Guid id, bool isAktif)
        {
            try
            {
                var harga = await _context.HargaPasar.FindAsync(id);
                if (harga == null) return false;

                // ? VALIDASI REPOSITORY LEVEL: Jika mengaktifkan, pastikan tidak ada harga lain yang aktif
                if (isAktif)
                {
                    var existingActiveCount = await _context.HargaPasar
                        .CountAsync(h => h.IsAktif && h.Id != id);
                    
                    if (existingActiveCount > 0)
                    {
                        // Jangan update jika sudah ada harga lain yang aktif
                        return false;
                    }
                }

                harga.IsAktif = isAktif;
                if (!isAktif && harga.TanggalBerakhir == null)
                {
                    harga.TanggalBerakhir = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<HargaPasar?> GetHargaByWilayahAsync(string wilayah, DateTime tanggal)
        {
            return await _context.HargaPasar
                .Where(h => h.IsAktif && 
                           h.Wilayah == wilayah &&
                           h.TanggalMulai <= tanggal && 
                           (h.TanggalBerakhir == null || h.TanggalBerakhir >= tanggal))
                .OrderByDescending(h => h.TanggalMulai)
                .FirstOrDefaultAsync();
        }
    }
}