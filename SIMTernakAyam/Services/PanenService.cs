using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class PanenService : BaseService<Panen>, IPanenService
    {
        private readonly IPanenRepository _panenRepository;
        private readonly IAyamRepository _ayamRepository;

        public PanenService(IPanenRepository repository, IAyamRepository ayamRepository) : base(repository)
        {
            _panenRepository = repository;
            _ayamRepository = ayamRepository;
        }

        public async Task<IEnumerable<Panen>> GetPanenByAyamAsync(Guid ayamId)
        {
            return await _panenRepository.GetByAyamIdAsync(ayamId);
        }

        public async Task<IEnumerable<Panen>> GetPanenByKandangAsync(Guid kandangId)
        {
            return await _panenRepository.GetByKandangIdAsync(kandangId);
        }

        public async Task<IEnumerable<Panen>> GetPanenByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _panenRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<Panen>> GetAllPanenWithDetailsAsync()
        {
            return await _panenRepository.GetWithDetailsAsync();
        }

        public async Task<Panen?> GetPanenWithDetailsAsync(Guid id)
        {
            return await _panenRepository.GetWithDetailsAsync(id);
        }

        public async Task<int> GetTotalEkorPanenByKandangAsync(Guid kandangId)
        {
            return await _panenRepository.GetTotalEkorPanenByKandangAsync(kandangId);
        }

        public async Task<decimal> GetTotalBeratPanenByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _panenRepository.GetTotalBeratPanenByDateRangeAsync(startDate, endDate);
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Panen entity)
        {
            if (entity.JumlahEkorPanen <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah ekor panen harus lebih dari 0." };
            }

            if (entity.BeratRataRata <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Berat rata-rata harus lebih dari 0." };
            }

            if (entity.TanggalPanen > DateTime.UtcNow)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal panen tidak boleh di masa depan." };
            }

            // Check if ayam exists
            var ayam = await _ayamRepository.GetByIdAsync(entity.AyamId);
            if (ayam == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Data ayam tidak ditemukan." };
            }

            // Validate that harvest date is not before entry date
            if (entity.TanggalPanen < ayam.TanggalMasuk)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal panen tidak boleh sebelum tanggal masuk ayam." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(Panen entity)
        {
            // Ensure TanggalPanen is UTC
            if (entity.TanggalPanen.Kind != DateTimeKind.Utc)
            {
                entity.TanggalPanen = entity.TanggalPanen.ToUniversalTime();
            }
            await Task.CompletedTask;
        }
    }
}