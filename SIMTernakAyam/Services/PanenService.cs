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
            _panenRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            _ayamRepository = ayamRepository ?? throw new ArgumentNullException(nameof(ayamRepository));
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

        public async Task<(int TotalMasuk, int SudahDipanen, int SisaTersedia)> GetStokAyamAsync(Guid ayamId)
        {
            var ayam = await _ayamRepository.GetByIdAsync(ayamId);
            if (ayam == null)
            {
                return (0, 0, 0);
            }

            var totalMasuk = ayam.JumlahMasuk;
            var sudahDipanen = await _panenRepository.GetTotalEkorPanenByAyamAsync(ayamId);
            var sisaTersedia = Math.Max(0, totalMasuk - sudahDipanen);

            return (totalMasuk, sudahDipanen, sisaTersedia);
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Panen entity)
        {
            // Basic validation
            if (entity.JumlahEkorPanen <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah ekor panen harus lebih dari 0." };
            }

            // Sync with CreatePanenDto validation: [Range(0.01, 100.00)]
            if (entity.BeratRataRata < 0.01m || entity.BeratRataRata > 100.00m)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Berat rata-rata harus antara 0.01 sampai 100.00 kg." };
            }

            if (entity.TanggalPanen > DateTime.UtcNow)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal panen tidak boleh di masa depan." };
            }

            // Check if ayam exists with proper null checking
            try
            {
                if (_ayamRepository == null)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Ayam repository is not available." };
                }

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

                // NEW VALIDATION: Check if harvest quantity doesn't exceed available stock
                var totalAyamMasuk = ayam.JumlahMasuk;
                var totalSudahDipanen = await _panenRepository.GetTotalEkorPanenByAyamAsync(entity.AyamId);
                var sisaAyam = totalAyamMasuk - totalSudahDipanen;

                if (entity.JumlahEkorPanen > sisaAyam)
                {
                    return new ValidationResult { 
                        IsValid = false, 
                        ErrorMessage = $"Jumlah panen ({entity.JumlahEkorPanen} ekor) melebihi sisa ayam yang tersedia ({sisaAyam} ekor). Total ayam masuk: {totalAyamMasuk}, sudah dipanen: {totalSudahDipanen}." 
                    };
                }

                if (sisaAyam <= 0)
                {
                    return new ValidationResult { 
                        IsValid = false, 
                        ErrorMessage = "Tidak ada ayam yang tersedia untuk dipanen. Semua ayam sudah dipanen." 
                    };
                }
            }
            catch (Exception ex)
            {
                // More detailed error for debugging
                var innerMsg = ex.InnerException?.Message ?? "";
                return new ValidationResult { 
                    IsValid = false, 
                    ErrorMessage = $"Error validating ayam data: {ex.Message} {innerMsg}".Trim()
                };
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

        protected override async Task<ValidationResult> ValidateOnUpdateAsync(Panen entity, Panen existingEntity)
        {
            // Basic validation
            if (entity.JumlahEkorPanen <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah ekor panen harus lebih dari 0." };
            }

            // Sync with UpdatePanenDto validation: [Range(0.01, 100.00)]
            if (entity.BeratRataRata < 0.01m || entity.BeratRataRata > 100.00m)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Berat rata-rata harus antara 0.01 sampai 100.00 kg." };
            }

            if (entity.TanggalPanen > DateTime.UtcNow)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal panen tidak boleh di masa depan." };
            }

            // Check if ayam exists with proper null checking
            try
            {
                if (_ayamRepository == null)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Ayam repository is not available." };
                }

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

                // NEW VALIDATION: Check stock for UPDATE (exclude current panen from calculation)
                var totalAyamMasuk = ayam.JumlahMasuk;
                var totalSudahDipanen = await _panenRepository.GetTotalEkorPanenByAyamExcludingAsync(entity.AyamId, entity.Id);
                var sisaAyam = totalAyamMasuk - totalSudahDipanen;

                if (entity.JumlahEkorPanen > sisaAyam)
                {
                    return new ValidationResult { 
                        IsValid = false, 
                        ErrorMessage = $"Jumlah panen ({entity.JumlahEkorPanen} ekor) melebihi sisa ayam yang tersedia ({sisaAyam} ekor). Total ayam masuk: {totalAyamMasuk}, sudah dipanen (kecuali yang sedang diupdate): {totalSudahDipanen}." 
                    };
                }
            }
            catch (Exception ex)
            {
                // More detailed error for debugging
                var innerMsg = ex.InnerException?.Message ?? "";
                return new ValidationResult { 
                    IsValid = false, 
                    ErrorMessage = $"Error validating ayam data: {ex.Message} {innerMsg}".Trim()
                };
            }

            return new ValidationResult { IsValid = true };
        }
    }
}