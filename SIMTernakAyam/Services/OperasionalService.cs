using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class OperasionalService : BaseService<Operasional>, IOperasionalService
    {
        private readonly IOperasionalRepository _operasionalRepository;
        private readonly IUserRepository _userRepository;
        private readonly IKandangRepository _kandangRepository;
        private readonly IJenisKegiatanRepository _jenisKegiatanRepository;

        public OperasionalService(
            IOperasionalRepository repository, 
            IUserRepository userRepository,
            IKandangRepository kandangRepository,
            IJenisKegiatanRepository jenisKegiatanRepository) : base(repository)
        {
            _operasionalRepository = repository;
            _userRepository = userRepository;
            _kandangRepository = kandangRepository;
            _jenisKegiatanRepository = jenisKegiatanRepository;
        }

        public async Task<IEnumerable<Operasional>> GetOperasionalByKandangAsync(Guid kandangId)
        {
            return await _operasionalRepository.GetByKandangIdAsync(kandangId);
        }

        public async Task<IEnumerable<Operasional>> GetOperasionalByPetugasAsync(Guid petugasId)
        {
            return await _operasionalRepository.GetByPetugasIdAsync(petugasId);
        }

        public async Task<IEnumerable<Operasional>> GetOperasionalByJenisKegiatanAsync(Guid jenisKegiatanId)
        {
            return await _operasionalRepository.GetByJenisKegiatanIdAsync(jenisKegiatanId);
        }

        public async Task<IEnumerable<Operasional>> GetOperasionalByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _operasionalRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<Operasional>> GetAllOperasionalWithDetailsAsync()
        {
            return await _operasionalRepository.GetWithDetailsAsync();
        }

        public async Task<Operasional?> GetOperasionalWithDetailsAsync(Guid id)
        {
            return await _operasionalRepository.GetWithDetailsAsync(id);
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Operasional entity)
        {
            if (entity.Jumlah <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah operasional harus lebih dari 0." };
            }

            if (entity.Tanggal > DateTime.UtcNow)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal operasional tidak boleh di masa depan." };
            }

            // Check if petugas exists
            var petugas = await _userRepository.GetByIdAsync(entity.PetugasId);
            if (petugas == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Petugas tidak ditemukan." };
            }

            // Check if kandang exists
            var kandang = await _kandangRepository.GetByIdAsync(entity.KandangId);
            if (kandang == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Kandang tidak ditemukan." };
            }

            // Check if jenis kegiatan exists
            var jenisKegiatan = await _jenisKegiatanRepository.GetByIdAsync(entity.JenisKegiatanId);
            if (jenisKegiatan == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jenis kegiatan tidak ditemukan." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(Operasional entity)
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