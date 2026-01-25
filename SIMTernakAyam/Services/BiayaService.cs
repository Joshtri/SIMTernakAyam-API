using SIMTernakAyam.DTOs.Biaya;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Services
{
    public class BiayaService : BaseService<Biaya>, IBiayaService
    {
        private readonly IBiayaRepository _biayaRepository;
        private readonly IUserRepository _userRepository;

        public BiayaService(IBiayaRepository repository, IUserRepository userRepository) : base(repository)
        {
            _biayaRepository = repository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByPetugasAsync(Guid petugasId)
        {
            return await _biayaRepository.GetByPetugasIdAsync(petugasId);
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByOperasionalAsync(Guid operasionalId)
        {
            return await _biayaRepository.GetByOperasionalIdAsync(operasionalId);
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _biayaRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByJenisAsync(string jenisBiaya)
        {
            return await _biayaRepository.GetByJenisBiayaAsync(jenisBiaya);
        }

        public async Task<decimal> GetTotalBiayaPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _biayaRepository.GetTotalBiayaByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<Biaya>> GetAllBiayaWithDetailsAsync()
        {
            return await _biayaRepository.GetWithDetailsAsync();
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Biaya entity)
        {
            if (string.IsNullOrWhiteSpace(entity.JenisBiaya))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jenis biaya wajib diisi." };
            }

            if (entity.Jumlah <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah biaya harus lebih dari 0." };
            }

            // Compare dates only to avoid timezone issues
            if (entity.Tanggal.Date > DateTime.UtcNow.Date)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal biaya tidak boleh di masa depan." };
            }

            // Check if petugas exists
            var petugas = await _userRepository.GetByIdAsync(entity.PetugasId);
            if (petugas == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Petugas tidak ditemukan." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(Biaya entity)
        {
            // Ensure Tanggal is UTC
            if (entity.Tanggal.Kind != DateTimeKind.Utc)
            {
                entity.Tanggal = entity.Tanggal.ToUniversalTime();
            }

            // Auto set Bulan/Tahun jika belum diisi, prioritaskan Tanggal lalu CreatedAt
            if (!entity.Bulan.HasValue || !entity.Tahun.HasValue)
            {
                var sourceDate = entity.Tanggal != default ? entity.Tanggal : entity.CreatedAt;
                if (sourceDate != default)
                {
                    entity.Bulan = sourceDate.Month;
                    entity.Tahun = sourceDate.Year;
                }
            }
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Biaya entity, Biaya existingEntity)
        {
            // Ensure Tanggal is UTC
            if (entity.Tanggal.Kind != DateTimeKind.Utc)
            {
                entity.Tanggal = entity.Tanggal.ToUniversalTime();
            }

            // Auto set Bulan/Tahun jika belum diisi, prioritaskan Tanggal lalu CreatedAt
            if (!entity.Bulan.HasValue || !entity.Tahun.HasValue)
            {
                var sourceDate = entity.Tanggal != default ? entity.Tanggal : entity.CreatedAt;
                if (sourceDate != default)
                {
                    entity.Bulan = sourceDate.Month;
                    entity.Tahun = sourceDate.Year;
                }
            }
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByBulanTahunAsync(int bulan, int tahun)
        {
            return await _biayaRepository.GetByBulanTahunAsync(bulan, tahun);
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByKandangAsync(Guid kandangId)
        {
            return await _biayaRepository.GetByKandangIdAsync(kandangId);
        }

        public async Task<RekapBiayaBulananDto> GetRekapBiayaBulananAsync(int bulan, int tahun)
        {
            var allBiaya = await _biayaRepository.GetByBulanTahunAsync(bulan, tahun);

            var rekap = new RekapBiayaBulananDto
            {
                Bulan = bulan,
                Tahun = tahun
            };

            // Group by KandangId
            var groupedByKandang = allBiaya.GroupBy(b => b.KandangId);

            foreach (var group in groupedByKandang)
            {
                var biayaList = group.ToList();
                var firstBiaya = biayaList.First();

                var biayaBulanan = new BiayaBulananResponseDto
                {
                    Bulan = bulan,
                    Tahun = tahun,
                    KandangId = group.Key,
                    KandangNama = firstBiaya.Kandang?.NamaKandang ?? "Tanpa Kandang",
                    TotalBiayaListrik = biayaList.Where(b => b.JenisBiaya.ToLower() == "listrik").Sum(b => b.Jumlah),
                    TotalBiayaAir = biayaList.Where(b => b.JenisBiaya.ToLower() == "air").Sum(b => b.Jumlah),
                    TotalBiayaLainnya = biayaList.Where(b => b.JenisBiaya.ToLower() != "listrik" && b.JenisBiaya.ToLower() != "air").Sum(b => b.Jumlah),
                    DetailBiaya = BiayaListResponseDto.FromEntities(biayaList)
                };

                biayaBulanan.TotalBiaya = biayaBulanan.TotalBiayaListrik + biayaBulanan.TotalBiayaAir + biayaBulanan.TotalBiayaLainnya;

                rekap.PerKandang.Add(biayaBulanan);
            }

            // Calculate grand totals
            rekap.GrandTotalBiayaListrik = rekap.PerKandang.Sum(k => k.TotalBiayaListrik);
            rekap.GrandTotalBiayaAir = rekap.PerKandang.Sum(k => k.TotalBiayaAir);
            rekap.GrandTotalBiayaLainnya = rekap.PerKandang.Sum(k => k.TotalBiayaLainnya);
            rekap.GrandTotal = rekap.GrandTotalBiayaListrik + rekap.GrandTotalBiayaAir + rekap.GrandTotalBiayaLainnya;

            return rekap;
        }

        public async Task<Biaya?> GetSingleByOperasionalIdAsync(Guid operasionalId)
        {
            return await _biayaRepository.GetSingleByOperasionalIdAsync(operasionalId);
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByKategoriAsync(KategoriBiayaEnum kategori)
        {
            return await _biayaRepository.GetByKategoriBiayaAsync(kategori);
        }

        public async Task<IEnumerable<Biaya>> GetBiayaByKategoriAndPeriodAsync(KategoriBiayaEnum kategori, DateTime startDate, DateTime endDate)
        {
            return await _biayaRepository.GetByKategoriBiayaAndDateRangeAsync(kategori, startDate, endDate);
        }

        public async Task<decimal> GetTotalBiayaByKategoriAndPeriodAsync(KategoriBiayaEnum kategori, DateTime startDate, DateTime endDate)
        {
            return await _biayaRepository.GetTotalBiayaByKategoriBiayaAndDateRangeAsync(kategori, startDate, endDate);
        }
    }
}
