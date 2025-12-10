using SIMTernakAyam.DTOs.Pakan;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class PakanService : BaseService<Pakan>, IPakanService
    {
        private readonly IPakanRepository _pakanRepository;
        private readonly IOperasionalRepository _operasionalRepository;

        public PakanService(IPakanRepository repository, IOperasionalRepository operasionalRepository) : base(repository)
        {
            _pakanRepository = repository;
            _operasionalRepository = operasionalRepository;
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

        public async Task<object> GetStokDiagnosticAsync(Guid pakanId)
        {
            var pakan = await _pakanRepository.GetByIdAsync(pakanId);
            if (pakan == null) return null!;

            // Get all operasionals that use this pakan (without period filter)
            var allOperasionalsWithThisPakan = await _operasionalRepository
                .GetAllAsync();
            var operasionalsUsingPakan = allOperasionalsWithThisPakan
                .Where(o => o.PakanId == pakanId)
                .ToList();

            // Get operasionals in the specific period
            var startDate = new DateTime(pakan.Tahun, pakan.Bulan, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var operasionalsInPeriod = await _operasionalRepository
                .GetByPakanIdAndPeriodAsync(pakanId, startDate, endDate);

            // Get all operasionals to see what PakanIds are actually stored
            var allOperasionals = await _operasionalRepository.GetAllAsync();
            var operasionalsWithPakanId = allOperasionals
                .Where(o => o.PakanId.HasValue)
                .Select(o => new { 
                    o.Id, 
                    o.PakanId, 
                    o.Tanggal, 
                    o.Jumlah,
                    Bulan = o.Tanggal.Month,
                    Tahun = o.Tanggal.Year
                })
                .ToList();

            return new
            {
                Pakan = new {
                    pakan.Id,
                    pakan.NamaPakan,
                    pakan.StokKg,
                    pakan.Bulan,
                    pakan.Tahun,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                },
                AllOperasionalsUsingThisPakan = operasionalsUsingPakan.Select(o => new {
                    o.Id,
                    o.PakanId,
                    o.Tanggal,
                    o.Jumlah,
                    Bulan = o.Tanggal.Month,
                    Tahun = o.Tanggal.Year,
                    InPeriod = o.Tanggal >= startDate && o.Tanggal <= endDate
                }),
                OperasionalsInSpecificPeriod = operasionalsInPeriod.Select(o => new {
                    o.Id,
                    o.PakanId,
                    o.Tanggal,
                    o.Jumlah
                }),
                AllOperasionalsWithPakanId = operasionalsWithPakanId,
                TotalOperasionalsWithPakan = operasionalsUsingPakan.Count,
                TotalOperasionalsInPeriod = operasionalsInPeriod.Count(),
                TotalStokTerpakaiAllTime = operasionalsUsingPakan.Sum(o => o.Jumlah),
                TotalStokTerpakaiInPeriod = operasionalsInPeriod.Sum(o => o.Jumlah),
                Debug = new {
                    QueryPeriod = $"{pakan.Bulan}/{pakan.Tahun}",
                    HasOperasionalsWithThisPakan = operasionalsUsingPakan.Any(),
                    HasOperasionalsInPeriod = operasionalsInPeriod.Any()
                }
            };
        }

        private async Task<decimal> GetTotalStokTerpakai(Guid pakanId, int bulan, int tahun)
        {
            try
            {
                // ? FIX: Ensure proper date calculation
                var startDate = new DateTime(tahun, bulan, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

                // ? DEBUG: Log the query parameters (remove in production)
                Console.WriteLine($"[DEBUG] GetTotalStokTerpakai - PakanId: {pakanId}, Periode: {bulan}/{tahun}");
                Console.WriteLine($"[DEBUG] Query range: {startDate:yyyy-MM-dd HH:mm:ss} to {endDate:yyyy-MM-dd HH:mm:ss}");

                // Ambil semua operasional yang menggunakan pakan ini pada periode tersebut
                var operasionals = await _operasionalRepository.GetByPakanIdAndPeriodAsync(pakanId, startDate, endDate);
                
                // ? DEBUG: Log results
                Console.WriteLine($"[DEBUG] Found {operasionals.Count()} operasionals for pakan {pakanId}");
                foreach (var op in operasionals)
                {
                    Console.WriteLine($"[DEBUG] Operasional: {op.Id}, Tanggal: {op.Tanggal:yyyy-MM-dd HH:mm:ss}, Jumlah: {op.Jumlah}");
                }

                // ? FIX: Cast to decimal to match return type
                var totalTerpakai = operasionals.Sum(o => (decimal)o.Jumlah);
                
                Console.WriteLine($"[DEBUG] Total stok terpakai: {totalTerpakai}");
                
                return totalTerpakai;
            }
            catch (Exception ex)
            {
                // ? ENHANCED: Better error logging
                Console.WriteLine($"[ERROR] GetTotalStokTerpakai failed for PakanId {pakanId}: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return 0;
            }
        }

        private static string GetStatusStok(decimal stokTersisa)
        {
            return stokTersisa switch
            {
                0 => "Habis",
                <= 10 => "Kritis",
                <= 50 => "Menipis", 
                _ => "Aman"
            };
        }

        private static string GetRekomendasi(decimal stokTersedia, decimal jumlahDibutuhkan)
        {
            if (stokTersedia >= jumlahDibutuhkan)
            {
                var sisaSetelahPakai = stokTersedia - jumlahDibutuhkan;
                return sisaSetelahPakai switch
                {
                    0 => "Stok akan habis setelah penggunaan ini. Perlu penambahan stok segera.",
                    <= 10 => "Stok akan menjadi kritis setelah penggunaan. Disarankan untuk menambah stok.",
                    <= 50 => "Stok akan menipis setelah penggunaan. Pertimbangkan untuk menambah stok.",
                    _ => "Stok masih aman untuk penggunaan ini."
                };
            }
            else
            {
                var kekurangan = jumlahDibutuhkan - stokTersedia;
                return $"Stok tidak mencukupi. Kekurangan {kekurangan} kg. Perlu penambahan stok minimal {kekurangan} kg.";
            }
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

        public async Task<List<PakanResponseDto>> GetAllPakanWithUsageDetailAsync()
        {
            var pakans = await _pakanRepository.GetAllAsync();
            var result = new List<PakanResponseDto>();

            foreach (var pakan in pakans)
            {
                // ? TRY ALTERNATIVE CALCULATION FIRST
                var stokTerpakai = await GetTotalStokTerpakaiAlternative(pakan.Id, pakan.Bulan, pakan.Tahun);
                
                // ? FALLBACK to original if alternative fails
                if (stokTerpakai == 0)
                {
                    stokTerpakai = await GetTotalStokTerpakai(pakan.Id, pakan.Bulan, pakan.Tahun);
                }
                
                result.Add(PakanResponseDto.FromEntityWithUsage(pakan, stokTerpakai));
            }

            return result;
        }

        /// <summary>
        /// Alternative calculation method untuk debug stok terpakai
        /// </summary>
        private async Task<decimal> GetTotalStokTerpakaiAlternative(Guid pakanId, int bulan, int tahun)
        {
            try
            {
                Console.WriteLine($"[ALT DEBUG] Starting alternative calculation for PakanId: {pakanId}, Period: {bulan}/{tahun}");
                
                // Get ALL operasionals first
                var allOperasionals = await _operasionalRepository.GetAllAsync();
                Console.WriteLine($"[ALT DEBUG] Total operasionals in database: {allOperasionals.Count()}");
                
                // Filter yang menggunakan pakan ini
                var operasionalsUsingThisPakan = allOperasionals
                    .Where(o => o.PakanId.HasValue && o.PakanId == pakanId)
                    .ToList();
                
                Console.WriteLine($"[ALT DEBUG] Operasionals using this pakan: {operasionalsUsingThisPakan.Count}");
                
                // Filter berdasarkan periode (lebih flexible)
                var operasionalsInPeriod = operasionalsUsingThisPakan
                    .Where(o => o.Tanggal.Month == bulan && o.Tanggal.Year == tahun)
                    .ToList();
                
                Console.WriteLine($"[ALT DEBUG] Operasionals in period {bulan}/{tahun}: {operasionalsInPeriod.Count}");
                
                foreach (var op in operasionalsInPeriod)
                {
                    Console.WriteLine($"[ALT DEBUG] - OpId: {op.Id}, Date: {op.Tanggal:yyyy-MM-dd}, Jumlah: {op.Jumlah}");
                }
                
                var totalTerpakai = operasionalsInPeriod.Sum(o => (decimal)o.Jumlah);
                Console.WriteLine($"[ALT DEBUG] Total calculated: {totalTerpakai}");
                
                return totalTerpakai;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ALT ERROR] Alternative calculation failed: {ex.Message}");
                return 0;
            }
        }

        public async Task<PakanResponseDto?> GetPakanWithUsageDetailAsync(Guid id)
        {
            var pakan = await _pakanRepository.GetByIdAsync(id);
            if (pakan == null) return null;

            // ? Use alternative calculation first
            var stokTerpakai = await GetTotalStokTerpakaiAlternative(pakan.Id, pakan.Bulan, pakan.Tahun);
            
            // Fallback to original if needed
            if (stokTerpakai == 0)
            {
                stokTerpakai = await GetTotalStokTerpakai(pakan.Id, pakan.Bulan, pakan.Tahun);
            }
            
            return PakanResponseDto.FromEntityWithUsage(pakan, stokTerpakai);
        }

        public async Task<object?> CheckStockAvailabilityAsync(Guid id, decimal jumlahDibutuhkan)
        {
            var pakan = await _pakanRepository.GetByIdAsync(id);
            if (pakan == null) return null;

            var stokTersedia = pakan.StokKg;
            var isAvailable = stokTersedia >= jumlahDibutuhkan;
            
            // ? Use alternative calculation first
            var stokTerpakai = await GetTotalStokTerpakaiAlternative(pakan.Id, pakan.Bulan, pakan.Tahun);
            if (stokTerpakai == 0)
            {
                stokTerpakai = await GetTotalStokTerpakai(pakan.Id, pakan.Bulan, pakan.Tahun);
            }

            return new
            {
                PakanId = id,
                NamaPakan = pakan.NamaPakan,
                StokTersedia = stokTersedia,
                JumlahDibutuhkan = jumlahDibutuhkan,
                IsAvailable = isAvailable,
                StokKurang = isAvailable ? 0 : jumlahDibutuhkan - stokTersedia,
                StokTerpakai = stokTerpakai,
                StatusStok = GetStatusStok(stokTersedia),
                Rekomendasi = GetRekomendasi(stokTersedia, jumlahDibutuhkan)
            };
        }
    }
}