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
        private readonly IPakanRepository _pakanRepository;
        private readonly IVaksinRepository _vaksinRepository;
        private readonly IBiayaService _biayaService;
        private readonly IStokService _stokService;

        public OperasionalService(
            IOperasionalRepository repository, 
            IUserRepository userRepository,
            IKandangRepository kandangRepository,
            IJenisKegiatanRepository jenisKegiatanRepository,
            IPakanRepository pakanRepository,
            IVaksinRepository vaksinRepository,
            IBiayaService biayaService,
            IStokService stokService) : base(repository)
        {
            _operasionalRepository = repository;
            _userRepository = userRepository;
            _kandangRepository = kandangRepository;
            _jenisKegiatanRepository = jenisKegiatanRepository;
            _pakanRepository = pakanRepository;
            _vaksinRepository = vaksinRepository;
            _biayaService = biayaService;
            _stokService = stokService;
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

            // Check if Pakan is being used and validate stock
            if (entity.PakanId.HasValue)
            {
                var pakanStockInfo = await _pakanRepository.GetStockInfoAsync(entity.PakanId.Value);
                if (pakanStockInfo == null)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Pakan tidak ditemukan." };
                }

                // Check if stock is sufficient
                if (pakanStockInfo.Value.StokKg < entity.Jumlah)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Stok pakan tidak mencukupi. Tersedia: {pakanStockInfo.Value.StokKg} kg, Dibutuhkan: {entity.Jumlah} kg." };
                }

                // Check if the date matches the stock period
                if (pakanStockInfo.Value.Bulan != entity.Tanggal.Month || pakanStockInfo.Value.Tahun != entity.Tanggal.Year)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Tanggal operasional tidak sesuai dengan periode stok pakan ({pakanStockInfo.Value.Bulan}/{pakanStockInfo.Value.Tahun})." };
                }
            }

            // Check if Vaksin is being used and validate stock
            if (entity.VaksinId.HasValue)
            {
                var vaksinStockInfo = await _vaksinRepository.GetStockInfoAsync(entity.VaksinId.Value);
                if (vaksinStockInfo == null)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Vaksin tidak ditemukan." };
                }

                // Check if stock is sufficient
                if (vaksinStockInfo.Value.Stok < entity.Jumlah)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Stok vaksin tidak mencukupi. Tersedia: {vaksinStockInfo.Value.Stok} dosis, Dibutuhkan: {entity.Jumlah} dosis." };
                }

                // Check if the date matches the stock period
                if (vaksinStockInfo.Value.Bulan != entity.Tanggal.Month || vaksinStockInfo.Value.Tahun != entity.Tanggal.Year)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Tanggal operasional tidak sesuai dengan periode stok vaksin ({vaksinStockInfo.Value.Bulan}/{vaksinStockInfo.Value.Tahun})." };
                }
            }

            // If both PakanId and VaksinId are null but Jumlah > 0, provide warning
            if (!entity.PakanId.HasValue && !entity.VaksinId.HasValue)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Harus memilih pakan atau vaksin untuk operasional." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task<ValidationResult> ValidateOnUpdateAsync(Operasional entity, Operasional existingEntity)
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

            // Check if Pakan is being used and validate stock
            if (entity.PakanId.HasValue)
            {
                var pakanStockInfo = await _pakanRepository.GetStockInfoAsync(entity.PakanId.Value);
                if (pakanStockInfo == null)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Pakan tidak ditemukan." };
                }

                // Calculate the difference between new and old stock usage
                decimal currentPakanUsage = existingEntity.PakanId == entity.PakanId ? existingEntity.Jumlah : 0;
                decimal additionalPakanNeeded = entity.Jumlah - currentPakanUsage;
                
                if (additionalPakanNeeded > 0 && pakanStockInfo.Value.StokKg < additionalPakanNeeded)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Stok pakan tidak mencukupi. Tersedia: {pakanStockInfo.Value.StokKg} kg, Dibutuhkan tambahan: {additionalPakanNeeded} kg." };
                }

                // Check if the date matches the stock period
                if (pakanStockInfo.Value.Bulan != entity.Tanggal.Month || pakanStockInfo.Value.Tahun != entity.Tanggal.Year)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Tanggal operasional tidak sesuai dengan periode stok pakan ({pakanStockInfo.Value.Bulan}/{pakanStockInfo.Value.Tahun})." };
                }
            }

            // Check if Vaksin is being used and validate stock
            if (entity.VaksinId.HasValue)
            {
                var vaksinStockInfo = await _vaksinRepository.GetStockInfoAsync(entity.VaksinId.Value);
                if (vaksinStockInfo == null)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Vaksin tidak ditemukan." };
                }

                // Calculate the difference between new and old stock usage
                int currentVaksinUsage = existingEntity.VaksinId == entity.VaksinId ? existingEntity.Jumlah : 0;
                int additionalVaksinNeeded = entity.Jumlah - currentVaksinUsage;
                
                if (additionalVaksinNeeded > 0 && vaksinStockInfo.Value.Stok < additionalVaksinNeeded)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Stok vaksin tidak mencukupi. Tersedia: {vaksinStockInfo.Value.Stok} dosis, Dibutuhkan tambahan: {additionalVaksinNeeded} dosis." };
                }

                // Check if the date matches the stock period
                if (vaksinStockInfo.Value.Bulan != entity.Tanggal.Month || vaksinStockInfo.Value.Tahun != entity.Tanggal.Year)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Tanggal operasional tidak sesuai dengan periode stok vaksin ({vaksinStockInfo.Value.Bulan}/{vaksinStockInfo.Value.Tahun})." };
                }
            }

            // If both PakanId and VaksinId are null but Jumlah > 0, provide warning
            if (!entity.PakanId.HasValue && !entity.VaksinId.HasValue)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Harus memilih pakan atau vaksin untuk operasional." };
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

        protected override async Task AfterCreateAsync(Operasional entity)
        {
            // Reduce stock if pakan or vaksin was used
            if (entity.PakanId.HasValue)
            {
                await _stokService.KurangiStokPakan(entity.PakanId.Value, entity.Tanggal, entity.Jumlah);
            }

            if (entity.VaksinId.HasValue)
            {
                await _stokService.KurangiStokVaksin(entity.VaksinId.Value, entity.Tanggal, entity.Jumlah);
            }
            
            await Task.CompletedTask;
        }

        protected override async Task AfterUpdateAsync(Operasional entity)
        {
            // Get the original entity to calculate stock difference
            var originalEntity = await _operasionalRepository.GetByIdAsync(entity.Id);
            if (originalEntity == null) return;

            // Handle Pakan stock adjustment
            if (originalEntity.PakanId.HasValue)
            {
                // Restore original pakan stock
                await _stokService.TambahStokPakan(originalEntity.PakanId.Value, originalEntity.Tanggal, originalEntity.Jumlah);
            }

            if (entity.PakanId.HasValue && entity.PakanId != originalEntity.PakanId)
            {
                // If different pakan is now being used, reduce its stock
                await _stokService.KurangiStokPakan(entity.PakanId.Value, entity.Tanggal, entity.Jumlah);
            }
            else if (entity.PakanId.HasValue && entity.PakanId == originalEntity.PakanId)
            {
                // If same pakan but different quantity, adjust accordingly
                var quantityDifference = entity.Jumlah - originalEntity.Jumlah;
                if (quantityDifference > 0)
                {
                    // Need to reduce more stock
                    await _stokService.KurangiStokPakan(entity.PakanId.Value, entity.Tanggal, quantityDifference);
                }
                else if (quantityDifference < 0)
                {
                    // Need to restore some stock (negative difference)
                    await _stokService.TambahStokPakan(entity.PakanId.Value, entity.Tanggal, Math.Abs(quantityDifference));
                }
            }

            // Handle Vaksin stock adjustment
            if (originalEntity.VaksinId.HasValue)
            {
                // Restore original vaksin stock
                await _stokService.TambahStokVaksin(originalEntity.VaksinId.Value, originalEntity.Tanggal, originalEntity.Jumlah);
            }

            if (entity.VaksinId.HasValue && entity.VaksinId != originalEntity.VaksinId)
            {
                // If different vaksin is now being used, reduce its stock
                await _stokService.KurangiStokVaksin(entity.VaksinId.Value, entity.Tanggal, entity.Jumlah);
            }
            else if (entity.VaksinId.HasValue && entity.VaksinId == originalEntity.VaksinId)
            {
                // If same vaksin but different quantity, adjust accordingly
                var quantityDifference = entity.Jumlah - originalEntity.Jumlah;
                if (quantityDifference > 0)
                {
                    // Need to reduce more stock
                    await _stokService.KurangiStokVaksin(entity.VaksinId.Value, entity.Tanggal, quantityDifference);
                }
                else if (quantityDifference < 0)
                {
                    // Need to restore some stock (negative difference)
                    await _stokService.TambahStokVaksin(entity.VaksinId.Value, entity.Tanggal, Math.Abs(quantityDifference));
                }
            }
            
            await Task.CompletedTask;
        }

        protected override async Task AfterDeleteAsync(Operasional entity)
        {
            // Restore stock when operational record is deleted
            if (entity.PakanId.HasValue)
            {
                await _stokService.TambahStokPakan(entity.PakanId.Value, entity.Tanggal, entity.Jumlah);
            }

            if (entity.VaksinId.HasValue)
            {
                await _stokService.TambahStokVaksin(entity.VaksinId.Value, entity.Tanggal, entity.Jumlah);
            }
            
            await Task.CompletedTask;
        }
    }
}