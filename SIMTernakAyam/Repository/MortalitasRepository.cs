using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Repository
{
    public class MortalitasRepository : BaseRepository<Mortalitas>, IMortalitasRepository
    {
        public MortalitasRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Override GetAllAsync to include related entities
        public override async Task<IEnumerable<Mortalitas>> GetAllAsync()
        {
            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        // Override GetByIdAsync to include related entities
        public override async Task<Mortalitas?> GetByIdAsync(Guid id)
        {
            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        // Get mortalitas with detailed calculations
        public async Task<IEnumerable<Mortalitas>> GetMortalitasWithCalculationsAsync()
        {
            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        // Get mortalitas by kandang with calculations
        public async Task<IEnumerable<Mortalitas>> GetByKandangIdAsync(Guid kandangId)
        {
            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .Where(m => m.Ayam.KandangId == kandangId)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        // Calculate total ayam before mortality incident (PER BATCH, not per kandang)
        public async Task<int> GetTotalAyamBeforeMortalityAsync(Guid ayamId, DateTime tanggalKematian)
        {
            var ayam = await _context.Ayams.FindAsync(ayamId);
            if (ayam == null) return 0;

            // Get total ayam masuk for THIS SPECIFIC BATCH only
            var totalMasuk = ayam.JumlahMasuk;

            // ⭐ Subtract previous mortalities for THIS BATCH only (EXCLUDE SOFT DELETED)
            var totalMortalities = await _context.Mortalitas
                .Where(m => m.AyamId == ayamId && m.TanggalKematian < tanggalKematian && !m.IsDeleted)
                .SumAsync(m => m.JumlahKematian);

            // ⭐ Subtract previous harvests for THIS BATCH only (EXCLUDE SOFT DELETED)
            var totalPanen = await _context.Panens
                .Where(p => p.AyamId == ayamId && p.TanggalPanen < tanggalKematian && !p.IsDeleted)
                .SumAsync(p => p.JumlahEkorPanen);

            return Math.Max(0, totalMasuk - totalMortalities - totalPanen);
        }

        /// <summary>
        /// Calculate total ayam AFTER a specific mortality event (for display purposes)
        /// This includes mortalities on the same date UP TO the specific mortality ID
        /// </summary>
        public async Task<int> GetTotalAyamAfterMortalityAsync(Guid ayamId, Guid mortalitasId, DateTime tanggalKematian)
        {
            var ayam = await _context.Ayams.FindAsync(ayamId);
            if (ayam == null) return 0;

            // Get the mortality record to get its JumlahKematian and CreatedAt
            var currentMortalitas = await _context.Mortalitas.FindAsync(mortalitasId);
            if (currentMortalitas == null) return 0;

            // Get total ayam masuk for THIS SPECIFIC BATCH only
            var totalMasuk = ayam.JumlahMasuk;

            // ⭐ Subtract ALL mortalities UP TO AND INCLUDING this mortality event
            // Use CreatedAt to determine order for mortalities on the same date
            var totalMortalities = await _context.Mortalitas
                .Where(m => m.AyamId == ayamId && !m.IsDeleted &&
                           (m.TanggalKematian < tanggalKematian ||
                            (m.TanggalKematian == tanggalKematian && m.CreatedAt <= currentMortalitas.CreatedAt)))
                .SumAsync(m => m.JumlahKematian);

            // ⭐ Subtract ALL harvests UP TO this date
            var totalPanen = await _context.Panens
                .Where(p => p.AyamId == ayamId && !p.IsDeleted && p.TanggalPanen <= tanggalKematian)
                .SumAsync(p => p.JumlahEkorPanen);

            return Math.Max(0, totalMasuk - totalMortalities - totalPanen);
        }

        // Get kandang capacity
        public async Task<int> GetKandangCapacityAsync(Guid kandangId)
        {
            var kandang = await _context.Kandangs.FindAsync(kandangId);
            return kandang?.Kapasitas ?? 0;
        }

        // Search mortalitas with filters
        public async Task<IEnumerable<Mortalitas>> SearchMortalitasAsync(string? search = null, Guid? kandangId = null)
        {
            var query = _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .AsQueryable();

            if (kandangId.HasValue)
            {
                query = query.Where(m => m.Ayam.KandangId == kandangId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(m =>
                    m.PenyebabKematian.Contains(s) ||
                    (m.Ayam.Kandang != null && m.Ayam.Kandang.NamaKandang.Contains(s)) ||
                    (m.Ayam.Kandang != null && m.Ayam.Kandang.User != null &&
                     (m.Ayam.Kandang.User.FullName.Contains(s) || m.Ayam.Kandang.User.Username.Contains(s))) ||
                    m.JumlahKematian.ToString().Contains(s)
                );
            }

            return await query.OrderByDescending(m => m.TanggalKematian).ToListAsync();
        }

        // Get mortalitas by ayam ID
        public async Task<IEnumerable<Mortalitas>> GetByAyamIdAsync(Guid ayamId)
        {
            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .Where(m => m.AyamId == ayamId)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        // Get mortalitas by date range
        public async Task<IEnumerable<Mortalitas>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure dates are in UTC
            var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .Where(m => m.TanggalKematian >= startUtc && m.TanggalKematian <= endUtc)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        // Get all mortalitas with details (overload without parameters)
        public async Task<IEnumerable<Mortalitas>> GetWithDetailsAsync()
        {
            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .OrderByDescending(m => m.TanggalKematian)
                .ToListAsync();
        }

        // Get single mortalitas with details (overload with Guid parameter)
        public async Task<Mortalitas?> GetWithDetailsAsync(Guid id)
        {
            return await _database
                .Include(m => m.Ayam)
                    .ThenInclude(a => a.Kandang)
                    .ThenInclude(k => k.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Get total mortalitas count by kandang
        public async Task<int> GetTotalMortalitasByKandangAsync(Guid kandangId)
        {
            return await _database
                .Where(m => m.Ayam.KandangId == kandangId)
                .SumAsync(m => m.JumlahKematian);
        }

        // Get total mortalitas count by date range
        public async Task<int> GetTotalMortalitasByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure dates are in UTC
            var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            return await _database
                .Where(m => m.TanggalKematian >= startUtc && m.TanggalKematian <= endUtc)
                .SumAsync(m => m.JumlahKematian);
        }

        // Get total mortality count for specific ayam
        public async Task<int> GetTotalMortalitasByAyamAsync(Guid ayamId)
        {
            return await _database
                .Where(m => m.AyamId == ayamId)
                .SumAsync(m => m.JumlahKematian);
        }

        // Get total mortality count for multiple ayam IDs at once (for efficient bulk queries)
        public async Task<Dictionary<Guid, int>> GetTotalMortalitasByAyamIdsAsync(IEnumerable<Guid> ayamIds)
        {
            var result = await _database
                .Where(m => ayamIds.Contains(m.AyamId))
                .GroupBy(m => m.AyamId)
                .Select(g => new { AyamId = g.Key, Total = g.Sum(m => m.JumlahKematian) })
                .ToListAsync();

            return result.ToDictionary(x => x.AyamId, x => x.Total);
        }
    }
}