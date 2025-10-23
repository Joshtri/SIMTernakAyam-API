using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class VaksinService : BaseService<Vaksin>, IVaksinService
    {
        private readonly IVaksinRepository _vaksinRepository;

        public VaksinService(IVaksinRepository repository) : base(repository)
        {
            _vaksinRepository = repository;
        }

        public async Task<Vaksin?> GetByNameAsync(string namaVaksin)
        {
            return await _vaksinRepository.GetByNameAsync(namaVaksin);
        }

        public async Task<IEnumerable<Vaksin>> GetLowStockVaksinAsync(int threshold = 5)
        {
            return await _vaksinRepository.GetLowStockAsync(threshold);
        }

        public async Task<(bool Success, string Message)> UpdateStokAsync(Guid id, int newStok)
        {
            if (newStok < 0)
            {
                return (false, "Stok tidak boleh negatif.");
            }

            var success = await _vaksinRepository.UpdateStokAsync(id, newStok);
            if (!success)
            {
                return (false, "Vaksin tidak ditemukan.");
            }

            return (true, "Stok berhasil diupdate.");
        }

        public async Task<(bool Success, string Message)> ValidateUniqueNameAsync(string namaVaksin, Guid? excludeId = null)
        {
            var exists = await _vaksinRepository.IsNameExistsAsync(namaVaksin, excludeId);
            if (exists)
            {
                return (false, $"Vaksin dengan nama '{namaVaksin}' sudah ada.");
            }

            return (true, "Nama vaksin tersedia.");
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Vaksin entity)
        {
            if (string.IsNullOrWhiteSpace(entity.NamaVaksin))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nama vaksin wajib diisi." };
            }

            // Check unique name
            var nameExists = await _vaksinRepository.IsNameExistsAsync(entity.NamaVaksin);
            if (nameExists)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = $"Vaksin dengan nama '{entity.NamaVaksin}' sudah ada." };
            }

            if (entity.Stok < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Stok tidak boleh negatif." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task<ValidationResult> ValidateOnUpdateAsync(Vaksin entity, Vaksin existingEntity)
        {
            if (string.IsNullOrWhiteSpace(entity.NamaVaksin))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nama vaksin wajib diisi." };
            }

            // Check unique name (excluding current entity)
            var nameExists = await _vaksinRepository.IsNameExistsAsync(entity.NamaVaksin, entity.Id);
            if (nameExists)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = $"Vaksin dengan nama '{entity.NamaVaksin}' sudah ada." };
            }

            if (entity.Stok < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Stok tidak boleh negatif." };
            }

            return new ValidationResult { IsValid = true };
        }

        public async Task<IEnumerable<Vaksin>> GetByTypeAsync(VaksinVitaminTypeEnum tipe)
        {
            return await _vaksinRepository.GetByTypeAsync(tipe);
        }
    }
}