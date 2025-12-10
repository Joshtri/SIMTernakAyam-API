using SIMTernakAyam.DTOs.Vaksin;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class VaksinService : BaseService<Vaksin>, IVaksinService
    {
        private readonly IVaksinRepository _vaksinRepository;
        private readonly IOperasionalRepository _operasionalRepository;

        public VaksinService(IVaksinRepository repository, IOperasionalRepository operasionalRepository) : base(repository)
        {
            _vaksinRepository = repository;
            _operasionalRepository = operasionalRepository;
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

        public async Task<List<VaksinResponseDto>> GetAllVaksinWithUsageDetailAsync()
        {
            var vaksins = await _vaksinRepository.GetAllAsync();
            var result = new List<VaksinResponseDto>();

            foreach (var vaksin in vaksins)
            {
                var stokTerpakai = await GetTotalStokTerpakai(vaksin.Id, vaksin.Bulan, vaksin.Tahun);
                result.Add(VaksinResponseDto.FromEntityWithUsage(vaksin, stokTerpakai));
            }

            return result;
        }

        public async Task<VaksinResponseDto?> GetVaksinWithUsageDetailAsync(Guid id)
        {
            var vaksin = await _vaksinRepository.GetByIdAsync(id);
            if (vaksin == null) return null;

            var stokTerpakai = await GetTotalStokTerpakai(vaksin.Id, vaksin.Bulan, vaksin.Tahun);
            return VaksinResponseDto.FromEntityWithUsage(vaksin, stokTerpakai);
        }

        public async Task<object?> CheckStockAvailabilityAsync(Guid id, int jumlahDibutuhkan)
        {
            var vaksin = await _vaksinRepository.GetByIdAsync(id);
            if (vaksin == null) return null;

            var stokTersedia = vaksin.Stok;
            var isAvailable = stokTersedia >= jumlahDibutuhkan;
            var stokTerpakai = await GetTotalStokTerpakai(vaksin.Id, vaksin.Bulan, vaksin.Tahun);

            return new
            {
                VaksinId = id,
                NamaVaksin = vaksin.NamaVaksin,
                StokTersedia = stokTersedia,
                JumlahDibutuhkan = jumlahDibutuhkan,
                IsAvailable = isAvailable,
                StokKurang = isAvailable ? 0 : jumlahDibutuhkan - stokTersedia,
                StokTerpakai = stokTerpakai,
                StatusStok = GetStatusStok(stokTersedia),
                Rekomendasi = GetRekomendasi(stokTersedia, jumlahDibutuhkan)
            };
        }

        private async Task<int> GetTotalStokTerpakai(Guid vaksinId, int bulan, int tahun)
        {
            try
            {
                var startDate = new DateTime(tahun, bulan, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Ambil semua operasional yang menggunakan vaksin ini pada periode tersebut
                var operasionals = await _operasionalRepository.GetByVaksinIdAndPeriodAsync(vaksinId, startDate, endDate);
                
                return operasionals.Sum(o => o.Jumlah);
            }
            catch
            {
                return 0; // Jika error, return 0
            }
        }

        private static string GetStatusStok(int stokTersisa)
        {
            return stokTersisa switch
            {
                0 => "Habis",
                <= 2 => "Kritis",
                <= 5 => "Menipis", 
                _ => "Aman"
            };
        }

        private static string GetRekomendasi(int stokTersedia, int jumlahDibutuhkan)
        {
            if (stokTersedia >= jumlahDibutuhkan)
            {
                var sisaSetelahPakai = stokTersedia - jumlahDibutuhkan;
                return sisaSetelahPakai switch
                {
                    0 => "Stok akan habis setelah penggunaan ini. Perlu penambahan stok segera.",
                    <= 2 => "Stok akan menjadi kritis setelah penggunaan. Disarankan untuk menambah stok.",
                    <= 5 => "Stok akan menipis setelah penggunaan. Pertimbangkan untuk menambah stok.",
                    _ => "Stok masih aman untuk penggunaan ini."
                };
            }
            else
            {
                var kekurangan = jumlahDibutuhkan - stokTersedia;
                return $"Stok tidak mencukupi. Kekurangan {kekurangan} dosis. Perlu penambahan stok minimal {kekurangan} dosis.";
            }
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