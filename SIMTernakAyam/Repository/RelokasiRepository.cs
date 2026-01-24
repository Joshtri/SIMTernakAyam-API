using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class RelokasiRepository : BaseRepository<RelokasiAyam>, IRelokasiRepository
    {
        public RelokasiRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Override GetAllAsync to include related entities
        /// </summary>
        public override async Task<IEnumerable<RelokasiAyam>> GetAllAsync()
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .OrderByDescending(r => r.TanggalRelokasi)
                .ToListAsync();
        }

        /// <summary>
        /// Override GetByIdAsync to include related entities
        /// </summary>
        public override async Task<RelokasiAyam?> GetByIdAsync(Guid id)
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        /// <summary>
        /// Get all relokasi with full details
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> GetAllWithDetailsAsync()
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                    .ThenInclude(a => a.Kandang)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .OrderByDescending(r => r.TanggalRelokasi)
                .ToListAsync();
        }

        /// <summary>
        /// Get relokasi by ID with full details
        /// </summary>
        public async Task<RelokasiAyam?> GetWithDetailsAsync(Guid id)
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                    .ThenInclude(a => a.Kandang)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        /// <summary>
        /// Get all relokasi for a specific kandang (as source or destination)
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> GetByKandangAsync(Guid kandangId)
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .Where(r => r.KandangAsalId == kandangId || r.KandangTujuanId == kandangId)
                .OrderByDescending(r => r.TanggalRelokasi)
                .ToListAsync();
        }

        /// <summary>
        /// Get all relokasi from a specific kandang (as source)
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> GetByKandangAsalAsync(Guid kandangAsalId)
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .Where(r => r.KandangAsalId == kandangAsalId)
                .OrderByDescending(r => r.TanggalRelokasi)
                .ToListAsync();
        }

        /// <summary>
        /// Get all relokasi to a specific kandang (as destination)
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> GetByKandangTujuanAsync(Guid kandangTujuanId)
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .Where(r => r.KandangTujuanId == kandangTujuanId)
                .OrderByDescending(r => r.TanggalRelokasi)
                .ToListAsync();
        }

        /// <summary>
        /// Get all relokasi for a specific batch ayam (as source)
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> GetByAyamAsalAsync(Guid ayamAsalId)
        {
            return await _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .Where(r => r.AyamAsalId == ayamAsalId)
                .OrderByDescending(r => r.TanggalRelokasi)
                .ToListAsync();
        }

        /// <summary>
        /// Get total ekor yang telah direlokasi keluar dari batch ayam tertentu
        /// Hanya hitung yang statusnya Selesai
        /// </summary>
        public async Task<int> GetTotalRelokasiKeluarByAyamAsync(Guid ayamId)
        {
            return await _database
                .Where(r => r.AyamAsalId == ayamId && r.StatusRelokasi == StatusRelokasiEnum.Selesai)
                .SumAsync(r => r.JumlahEkor);
        }

        /// <summary>
        /// Get total relokasi keluar untuk multiple ayam IDs (bulk query)
        /// </summary>
        public async Task<Dictionary<Guid, int>> GetTotalRelokasiKeluarByAyamIdsAsync(IEnumerable<Guid> ayamIds)
        {
            var result = await _database
                .Where(r => ayamIds.Contains(r.AyamAsalId) && r.StatusRelokasi == StatusRelokasiEnum.Selesai)
                .GroupBy(r => r.AyamAsalId)
                .Select(g => new { AyamId = g.Key, Total = g.Sum(r => r.JumlahEkor) })
                .ToListAsync();

            return result.ToDictionary(x => x.AyamId, x => x.Total);
        }

        /// <summary>
        /// Search relokasi with filters
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> SearchRelokasiAsync(string? search = null, Guid? kandangId = null)
        {
            var query = _database
                .Include(r => r.KandangAsal)
                .Include(r => r.KandangTujuan)
                .Include(r => r.AyamAsal)
                .Include(r => r.AyamTujuan)
                .Include(r => r.Petugas)
                .AsQueryable();

            // Filter by kandang (source or destination)
            if (kandangId.HasValue)
            {
                query = query.Where(r => r.KandangAsalId == kandangId || r.KandangTujuanId == kandangId);
            }

            // Free-text search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(r =>
                    (r.KandangAsal != null && r.KandangAsal.NamaKandang.Contains(s)) ||
                    (r.KandangTujuan != null && r.KandangTujuan.NamaKandang.Contains(s)) ||
                    (r.Petugas != null && (r.Petugas.FullName.Contains(s) || r.Petugas.Username.Contains(s))) ||
                    (r.Catatan != null && r.Catatan.Contains(s)) ||
                    r.JumlahEkor.ToString().Contains(s)
                );
            }

            return await query.OrderByDescending(r => r.TanggalRelokasi).ToListAsync();
        }
    }
}
