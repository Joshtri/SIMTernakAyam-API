using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

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

            if (entity.Tanggal > DateTime.UtcNow)
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
            await Task.CompletedTask;
        }
    }
}