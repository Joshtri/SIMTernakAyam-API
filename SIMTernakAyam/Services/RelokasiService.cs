using SIMTernakAyam.Data;
using SIMTernakAyam.DTOs.Relokasi;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class RelokasiService : BaseService<RelokasiAyam>, IRelokasiService
    {
        private readonly IRelokasiRepository _relokasiRepository;
        private readonly IKandangRepository _kandangRepository;
        private readonly IAyamRepository _ayamRepository;
        private readonly IPanenRepository _panenRepository;
        private readonly IMortalitasRepository _mortalitasRepository;
        private readonly ApplicationDbContext _context;

        public RelokasiService(
            IRelokasiRepository repository,
            IKandangRepository kandangRepository,
            IAyamRepository ayamRepository,
            IPanenRepository panenRepository,
            IMortalitasRepository mortalitasRepository,
            ApplicationDbContext context) : base(repository)
        {
            _relokasiRepository = repository;
            _kandangRepository = kandangRepository;
            _ayamRepository = ayamRepository;
            _panenRepository = panenRepository;
            _mortalitasRepository = mortalitasRepository;
            _context = context;
        }

        /// <summary>
        /// Get all relokasi with full details
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> GetAllRelokasiWithDetailsAsync()
        {
            return await _relokasiRepository.GetAllWithDetailsAsync();
        }

        /// <summary>
        /// Get relokasi by ID with full details
        /// </summary>
        public async Task<RelokasiAyam?> GetRelokasiWithDetailsAsync(Guid id)
        {
            return await _relokasiRepository.GetWithDetailsAsync(id);
        }

        /// <summary>
        /// Get all relokasi for a specific kandang
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> GetRelokasiByKandangAsync(Guid kandangId)
        {
            return await _relokasiRepository.GetByKandangAsync(kandangId);
        }

        /// <summary>
        /// Create relokasi with full business logic
        /// </summary>
        public async Task<(bool Success, string Message, RelokasiAyam? Data)> CreateRelokasiAsync(
            CreateRelokasiDto dto,
            Guid petugasId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validate kandang asal exists
                var kandangAsal = await _kandangRepository.GetByIdAsync(dto.KandangAsalId);
                if (kandangAsal == null)
                {
                    return (false, "Kandang asal tidak ditemukan.", null);
                }

                // 2. Validate kandang tujuan exists
                var kandangTujuan = await _kandangRepository.GetByIdAsync(dto.KandangTujuanId);
                if (kandangTujuan == null)
                {
                    return (false, "Kandang tujuan tidak ditemukan.", null);
                }

                // 3. Validate kandang asal dan tujuan berbeda
                if (dto.KandangAsalId == dto.KandangTujuanId)
                {
                    return (false, "Kandang asal dan tujuan tidak boleh sama.", null);
                }

                // 4. Validate ayam asal exists and belongs to kandang asal
                var ayamAsal = await _ayamRepository.GetByIdAsync(dto.AyamAsalId);
                if (ayamAsal == null)
                {
                    return (false, "Batch ayam asal tidak ditemukan.", null);
                }

                if (ayamAsal.KandangId != dto.KandangAsalId)
                {
                    return (false, "Batch ayam tidak berada di kandang asal yang dipilih.", null);
                }

                // 5. Calculate available stock for ayam asal
                var ayamAsalId = ayamAsal.Id;
                var jumlahDipanen = await _panenRepository.GetTotalEkorPanenByAyamAsync(ayamAsalId);
                var jumlahMortalitas = await _mortalitasRepository.GetTotalMortalitasByAyamAsync(ayamAsalId);
                var jumlahDirelokasi = await _relokasiRepository.GetTotalRelokasiKeluarByAyamAsync(ayamAsalId);

                var sisaAyamHidup = ayamAsal.JumlahMasuk - jumlahDipanen - jumlahMortalitas - jumlahDirelokasi;

                if (sisaAyamHidup < dto.JumlahEkor)
                {
                    return (false, $"Stok ayam tidak mencukupi. Tersedia: {sisaAyamHidup} ekor, diminta: {dto.JumlahEkor} ekor.", null);
                }

                // 6. Calculate available capacity at kandang tujuan
                var totalAyamTujuan = await _ayamRepository.GetTotalAyamInKandangAsync(dto.KandangTujuanId);
                var kapasitasTersedia = kandangTujuan.Kapasitas - totalAyamTujuan;

                if (kapasitasTersedia < dto.JumlahEkor)
                {
                    return (false, $"Kapasitas kandang tujuan tidak mencukupi. Tersedia: {kapasitasTersedia} ekor, diminta: {dto.JumlahEkor} ekor.", null);
                }

                // 7. Create new batch at kandang tujuan
                var tanggalRelokasi = dto.TanggalRelokasi.Kind == DateTimeKind.Utc
                    ? dto.TanggalRelokasi
                    : dto.TanggalRelokasi.ToUniversalTime();

                var ayamTujuan = new Ayam
                {
                    Id = Guid.NewGuid(),
                    KandangId = dto.KandangTujuanId,
                    TanggalMasuk = tanggalRelokasi,
                    JumlahMasuk = dto.JumlahEkor,
                    IsAyamSisa = false, // This is a relocated batch, not leftover
                    CreatedAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                await _ayamRepository.AddAsync(ayamTujuan);
                await _ayamRepository.SaveChangesAsync();

                // 8. Create relokasi record
                var relokasi = new RelokasiAyam
                {
                    Id = Guid.NewGuid(),
                    KandangAsalId = dto.KandangAsalId,
                    KandangTujuanId = dto.KandangTujuanId,
                    AyamAsalId = dto.AyamAsalId,
                    AyamTujuanId = ayamTujuan.Id,
                    JumlahEkor = dto.JumlahEkor,
                    TanggalRelokasi = tanggalRelokasi,
                    AlasanRelokasi = dto.AlasanRelokasi,
                    StatusRelokasi = StatusRelokasiEnum.Selesai,
                    Catatan = dto.Catatan,
                    PetugasId = petugasId,
                    CreatedAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                await _relokasiRepository.AddAsync(relokasi);
                await _relokasiRepository.SaveChangesAsync();

                await transaction.CommitAsync();

                // Fetch full relokasi with details for response
                var result = await _relokasiRepository.GetWithDetailsAsync(relokasi.Id);
                return (true, "Relokasi berhasil dibuat.", result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMessage = ex.InnerException != null
                    ? $"Terjadi kesalahan: {ex.Message}. Detail: {ex.InnerException.Message}"
                    : $"Terjadi kesalahan: {ex.Message}";
                return (false, errorMessage, null);
            }
        }

        /// <summary>
        /// Cancel relokasi (soft cancel - change status to Dibatalkan)
        /// </summary>
        public async Task<(bool Success, string Message)> BatalkanRelokasiAsync(Guid id)
        {
            try
            {
                var relokasi = await _relokasiRepository.GetByIdAsync(id);
                if (relokasi == null)
                {
                    return (false, "Relokasi tidak ditemukan.");
                }

                if (relokasi.StatusRelokasi == StatusRelokasiEnum.Dibatalkan)
                {
                    return (false, "Relokasi sudah dibatalkan sebelumnya.");
                }

                relokasi.StatusRelokasi = StatusRelokasiEnum.Dibatalkan;
                relokasi.UpdateAt = DateTime.UtcNow;

                _relokasiRepository.UpdateAsync(relokasi);
                await _relokasiRepository.SaveChangesAsync();

                return (true, "Relokasi berhasil dibatalkan. Catatan: Stok tidak otomatis dikembalikan, silakan lakukan penyesuaian manual jika diperlukan.");
            }
            catch (Exception ex)
            {
                return (false, $"Terjadi kesalahan: {ex.Message}");
            }
        }

        /// <summary>
        /// Get total ekor yang telah direlokasi keluar dari batch ayam tertentu
        /// </summary>
        public async Task<int> GetTotalRelokasiKeluarByAyamAsync(Guid ayamId)
        {
            return await _relokasiRepository.GetTotalRelokasiKeluarByAyamAsync(ayamId);
        }

        /// <summary>
        /// Get total relokasi keluar untuk multiple ayam IDs (bulk query)
        /// </summary>
        public async Task<Dictionary<Guid, int>> GetTotalRelokasiKeluarByAyamIdsAsync(IEnumerable<Guid> ayamIds)
        {
            return await _relokasiRepository.GetTotalRelokasiKeluarByAyamIdsAsync(ayamIds);
        }

        /// <summary>
        /// Search relokasi with filters
        /// </summary>
        public async Task<IEnumerable<RelokasiAyam>> SearchRelokasiAsync(string? search = null, Guid? kandangId = null)
        {
            return await _relokasiRepository.SearchRelokasiAsync(search, kandangId);
        }

        #region Override Base Validations

        protected override async Task<ValidationResult> ValidateOnCreateAsync(RelokasiAyam entity)
        {
            // Basic validations (more complex ones are in CreateRelokasiAsync)
            if (entity.JumlahEkor <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah ekor harus lebih dari 0." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(RelokasiAyam entity)
        {
            // Ensure TanggalRelokasi is UTC
            if (entity.TanggalRelokasi.Kind != DateTimeKind.Utc)
            {
                entity.TanggalRelokasi = entity.TanggalRelokasi.ToUniversalTime();
            }
            await Task.CompletedTask;
        }

        #endregion
    }
}
