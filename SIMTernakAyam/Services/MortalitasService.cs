using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class MortalitasService : BaseService<Mortalitas>, IMortalitasService
    {
        private readonly IMortalitasRepository _mortalitasRepository;
        private readonly IAyamRepository _ayamRepository;

        public MortalitasService(IMortalitasRepository repository, IAyamRepository ayamRepository) : base(repository)
        {
            _mortalitasRepository = repository;
            _ayamRepository = ayamRepository;
        }

        public async Task<IEnumerable<Mortalitas>> GetMortalitasByAyamAsync(Guid ayamId)
        {
            return await _mortalitasRepository.GetByAyamIdAsync(ayamId);
        }

        public async Task<IEnumerable<Mortalitas>> GetMortalitasByKandangAsync(Guid kandangId)
        {
            return await _mortalitasRepository.GetByKandangIdAsync(kandangId);
        }

        public async Task<IEnumerable<Mortalitas>> GetMortalitasByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _mortalitasRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<Mortalitas>> GetAllMortalitasWithDetailsAsync()
        {
            return await _mortalitasRepository.GetWithDetailsAsync();
        }

        public async Task<Mortalitas?> GetMortalitasWithDetailsAsync(Guid id)
        {
            return await _mortalitasRepository.GetWithDetailsAsync(id);
        }

        public async Task<int> GetTotalMortalitasByKandangAsync(Guid kandangId)
        {
            return await _mortalitasRepository.GetTotalMortalitasByKandangAsync(kandangId);
        }

        public async Task<int> GetTotalMortalitasByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _mortalitasRepository.GetTotalMortalitasByDateRangeAsync(startDate, endDate);
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Mortalitas entity)
        {
            if (entity.JumlahKematian <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah kematian harus lebih dari 0." };
            }

            if (entity.TanggalKematian > DateTime.UtcNow)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal kematian tidak boleh di masa depan." };
            }

            if (string.IsNullOrWhiteSpace(entity.PenyebabKematian))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Penyebab kematian wajib diisi." };
            }

            // Check if ayam exists
            var ayam = await _ayamRepository.GetByIdAsync(entity.AyamId);
            if (ayam == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Data ayam tidak ditemukan." };
            }

            // Validate that mortality date is not before entry date
            if (entity.TanggalKematian < ayam.TanggalMasuk)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal kematian tidak boleh sebelum tanggal masuk ayam." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(Mortalitas entity)
        {
            // Ensure TanggalKematian is UTC
            if (entity.TanggalKematian.Kind != DateTimeKind.Utc)
            {
                entity.TanggalKematian = entity.TanggalKematian.ToUniversalTime();
            }
            await Task.CompletedTask;
        }
    }
}