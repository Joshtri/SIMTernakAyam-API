using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class PakanService : BaseService<Pakan>, IPakanService
    {
        private readonly IPakanRepository _pakanRepository;

        public PakanService(IPakanRepository repository) : base(repository)
        {
            _pakanRepository = repository;
        }

        public async Task<Pakan?> GetByNameAsync(string namaPakan)
        {
            return await _pakanRepository.GetByNameAsync(namaPakan);
        }

        public async Task<IEnumerable<Pakan>> GetLowStockPakanAsync(int threshold = 10)
        {
            return await _pakanRepository.GetLowStockAsync(threshold);
        }

        public async Task<(bool Success, string Message)> UpdateStokAsync(Guid id, int newStok)
        {
            if (newStok < 0)
            {
                return (false, "Stok tidak boleh negatif.");
            }

            var success = await _pakanRepository.UpdateStokAsync(id, newStok);
            if (!success)
            {
                return (false, "Pakan tidak ditemukan.");
            }

            return (true, "Stok berhasil diupdate.");
        }

        public async Task<(bool Success, string Message)> ValidateUniqueNameAsync(string namaPakan, Guid? excludeId = null)
        {
            var exists = await _pakanRepository.IsNameExistsAsync(namaPakan, excludeId);
            if (exists)
            {
                return (false, $"Pakan dengan nama '{namaPakan}' sudah ada.");
            }

            return (true, "Nama pakan tersedia.");
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Pakan entity)
        {
            if (string.IsNullOrWhiteSpace(entity.NamaPakan))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nama pakan wajib diisi." };
            }

            // Check unique name
            var nameExists = await _pakanRepository.IsNameExistsAsync(entity.NamaPakan);
            if (nameExists)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = $"Pakan dengan nama '{entity.NamaPakan}' sudah ada." };
            }

            if (entity.StokKg < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Stok tidak boleh negatif." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task<ValidationResult> ValidateOnUpdateAsync(Pakan entity, Pakan existingEntity)
        {
            if (string.IsNullOrWhiteSpace(entity.NamaPakan))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Nama pakan wajib diisi." };
            }

            // Check unique name (excluding current entity)
            var nameExists = await _pakanRepository.IsNameExistsAsync(entity.NamaPakan, entity.Id);
            if (nameExists)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = $"Pakan dengan nama '{entity.NamaPakan}' sudah ada." };
            }

            if (entity.StokKg < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Stok tidak boleh negatif." };
            }

            return new ValidationResult { IsValid = true };
        }
    }
}