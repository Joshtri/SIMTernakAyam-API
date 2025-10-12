using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class JenisKegiatanService : BaseService<JenisKegiatan>, IJenisKegiatanService
    {
        private readonly IJenisKegiatanRepository _jenisKegiatanRepository;

        public JenisKegiatanService(IJenisKegiatanRepository repository) : base(repository)
        {
            _jenisKegiatanRepository = repository;
        }

        public async Task<JenisKegiatan?> GetByNameAsync(string namaKegiatan)
        {
            return await _jenisKegiatanRepository.GetByNameAsync(namaKegiatan);
        }

        public async Task<IEnumerable<JenisKegiatan>> GetBySatuanAsync(string satuan)
        {
            return await _jenisKegiatanRepository.GetBySatuanAsync(satuan);
        }

        public async Task<(bool Success, string Message)> ValidateUniqueNameAsync(string namaKegiatan, Guid? excludeId = null)
        {
            var exists = await _jenisKegiatanRepository.IsNameExistsAsync(namaKegiatan, excludeId);
            if (exists)
            {
                return (false, $"Jenis kegiatan dengan nama '{namaKegiatan}' sudah ada.");
            }

            return (true, "Nama jenis kegiatan tersedia.");
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(JenisKegiatan entity)
        {
            if (string.IsNullOrWhiteSpace(entity.NamaKegiatan))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nama kegiatan wajib diisi." };
            }

            // Check unique name
            var nameExists = await _jenisKegiatanRepository.IsNameExistsAsync(entity.NamaKegiatan);
            if (nameExists)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = $"Jenis kegiatan dengan nama '{entity.NamaKegiatan}' sudah ada." };
            }

            if (entity.BiayaDefault.HasValue && entity.BiayaDefault < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Biaya default tidak boleh negatif." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task<ValidationResult> ValidateOnUpdateAsync(JenisKegiatan entity, JenisKegiatan existingEntity)
        {
            if (string.IsNullOrWhiteSpace(entity.NamaKegiatan))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nama kegiatan wajib diisi." };
            }

            // Check unique name (excluding current entity)
            var nameExists = await _jenisKegiatanRepository.IsNameExistsAsync(entity.NamaKegiatan, entity.Id);
            if (nameExists)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = $"Jenis kegiatan dengan nama '{entity.NamaKegiatan}' sudah ada." };
            }

            if (entity.BiayaDefault.HasValue && entity.BiayaDefault < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Biaya default tidak boleh negatif." };
            }

            return new ValidationResult { IsValid = true };
        }
    }
}