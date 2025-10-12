using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class AyamService : BaseService<Ayam>, IAyamService
    {
        private readonly IAyamRepository _ayamRepository;
        private readonly IKandangRepository _kandangRepository;

        public AyamService(IAyamRepository repository, IKandangRepository kandangRepository) : base(repository)
        {
            _ayamRepository = repository;
            _kandangRepository = kandangRepository;
        }

        public async Task<IEnumerable<Ayam>> GetAyamByKandangAsync(Guid kandangId)
        {
            return await _ayamRepository.GetByKandangIdAsync(kandangId);
        }

        public async Task<(bool Success, string Message, Ayam? Data)> AddAyamToKandangAsync(Ayam ayam)
        {
            // Check kandang capacity before adding
            var isAvailable = await _kandangRepository.IsKandangAvailableAsync(ayam.KandangId, ayam.JumlahMasuk);
            if (!isAvailable)
            {
                return (false, "Kapasitas kandang tidak mencukupi untuk menambah ayam sebanyak ini.", null);
            }

            return await CreateAsync(ayam);
        }

        public async Task<int> GetTotalAyamCountInKandangAsync(Guid kandangId)
        {
            return await _ayamRepository.GetTotalAyamInKandangAsync(kandangId);
        }

        public async Task<IEnumerable<Ayam>> GetAllAyamWithDetailsAsync()
        {
            return await _ayamRepository.GetAyamWithKandangAsync();
        }

        public async Task<Ayam?> GetAyamWithDetailsAsync(Guid id)
        {
            return await _ayamRepository.GetWithDetailsAsync(id);
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Ayam entity)
        {
            if (entity.JumlahMasuk <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah ayam masuk harus lebih dari 0." };
            }

            if (entity.TanggalMasuk > DateTime.UtcNow)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal masuk tidak boleh di masa depan." };
            }

            // Check if kandang exists
            var kandang = await _kandangRepository.GetByIdAsync(entity.KandangId);
            if (kandang == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Kandang tidak ditemukan." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(Ayam entity)
        {
            // Ensure TanggalMasuk is UTC
            if (entity.TanggalMasuk.Kind != DateTimeKind.Utc)
            {
                entity.TanggalMasuk = entity.TanggalMasuk.ToUniversalTime();
            }
            await Task.CompletedTask;
        }
    }
}