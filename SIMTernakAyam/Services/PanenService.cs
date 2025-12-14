using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.DTOs.Panen;

namespace SIMTernakAyam.Services
{
    public class PanenService : BaseService<Panen>, IPanenService
    {
        private readonly IPanenRepository _panenRepository;
        private readonly IAyamRepository _ayamRepository;
        private readonly IHargaPasarService _hargaPasarService;
        private readonly IBiayaRepository _biayaRepository;
        private readonly IOperasionalRepository _operasionalRepository;

        public PanenService(
            IPanenRepository repository, 
            IAyamRepository ayamRepository,
            IHargaPasarService hargaPasarService,
            IBiayaRepository biayaRepository,
            IOperasionalRepository operasionalRepository) : base(repository)
        {
            _panenRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            _ayamRepository = ayamRepository ?? throw new ArgumentNullException(nameof(ayamRepository));
            _hargaPasarService = hargaPasarService ?? throw new ArgumentNullException(nameof(hargaPasarService));
            _biayaRepository = biayaRepository ?? throw new ArgumentNullException(nameof(biayaRepository));
            _operasionalRepository = operasionalRepository ?? throw new ArgumentNullException(nameof(operasionalRepository));
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

        public async Task<AnalisisKeuntunganDto?> GetAnalisisKeuntunganAsync(Guid panenId)
        {
            var panen = await _panenRepository.GetWithDetailsAsync(panenId);
            if (panen == null) return null;

            // Dapatkan harga pasar pada tanggal panen
            var hargaPasar = await _hargaPasarService.GetHargaAktifByTanggalAsync(panen.TanggalPanen);
            if (hargaPasar == null)
            {
                // Jika tidak ada harga pasar pada tanggal panen, gunakan harga terbaru
                hargaPasar = await _hargaPasarService.GetHargaTerbaruAsync();
                if (hargaPasar == null) return null;
            }

            // Hitung total berat
            var totalBeratKg = panen.JumlahEkorPanen * panen.BeratRataRata;
            
            // Hitung pendapatan kotor
            var pendapatanKotor = totalBeratKg * hargaPasar.HargaPerKg;

            // Hitung biaya operasional (dari ayam masuk sampai panen)
            var ayam = panen.Ayam;
            var totalBiaya = await HitungTotalBiayaOperasionalAsync(ayam.Id, ayam.TanggalMasuk, panen.TanggalPanen, panen.JumlahEkorPanen, ayam.JumlahMasuk);

            // Hitung keuntungan
            var keuntunganBersih = pendapatanKotor - totalBiaya.Total;
            var marginKeuntungan = pendapatanKotor > 0 ? (keuntunganBersih / pendapatanKotor) * 100 : 0;
            var roi = totalBiaya.Total > 0 ? (keuntunganBersih / totalBiaya.Total) * 100 : 0;
            var hargaPokokProduksi = totalBeratKg > 0 ? totalBiaya.Total / totalBeratKg : 0;

            // Tentukan status keuntungan
            var statusKeuntungan = keuntunganBersih > 0 ? "Untung" : keuntunganBersih < 0 ? "Rugi" : "Impas";

            return new AnalisisKeuntunganDto
            {
                PanenId = panen.Id,
                TanggalPanen = panen.TanggalPanen,
                JumlahAyam = panen.JumlahEkorPanen,
                BeratRataRata = panen.BeratRataRata,
                TotalBeratKg = totalBeratKg,
                HargaPasarPerKg = hargaPasar.HargaPerKg,
                TanggalHargaPasar = hargaPasar.TanggalMulai,
                WilayahHarga = hargaPasar.Wilayah,
                PendapatanKotor = pendapatanKotor,
                TotalBiayaOperasional = totalBiaya.Total,
                BiayaPakan = totalBiaya.BiayaPakan,
                BiayaVaksin = totalBiaya.BiayaVaksin,
                BiayaLainnya = totalBiaya.BiayaLainnya,
                KeuntunganBersih = keuntunganBersih,
                MarginKeuntungan = marginKeuntungan,
                ROI = roi,
                HargaPokokProduksi = hargaPokokProduksi,
                StatusKeuntungan = statusKeuntungan
            };
        }

        public async Task<List<AnalisisKeuntunganDto>> GetAnalisisKeuntunganByPeriodAsync(DateTime startDate, DateTime endDate, Guid? kandangId = null)
        {
            var panens = await _panenRepository.GetByDateRangeAsync(startDate, endDate);
            
            if (kandangId.HasValue)
            {
                panens = panens.Where(p => p.Ayam?.KandangId == kandangId.Value);
            }

            var result = new List<AnalisisKeuntunganDto>();
            
            foreach (var panen in panens)
            {
                var analisis = await GetAnalisisKeuntunganAsync(panen.Id);
                if (analisis != null)
                {
                    result.Add(analisis);
                }
            }

            return result.OrderByDescending(a => a.TanggalPanen).ToList();
        }

        public async Task<object> GetRingkasanKeuntunganAsync(DateTime startDate, DateTime endDate)
        {
            var analisisList = await GetAnalisisKeuntunganByPeriodAsync(startDate, endDate);
            
            if (!analisisList.Any())
            {
                return new
                {
                    Periode = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                    TotalPanen = 0,
                    TotalPendapatan = 0m,
                    TotalBiaya = 0m,
                    TotalKeuntungan = 0m,
                    RataRataMargin = 0m,
                    RataRataROI = 0m,
                    JumlahUntung = 0,
                    JumlahRugi = 0,
                    JumlahImpas = 0
                };
            }

            return new
            {
                Periode = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                TotalPanen = analisisList.Count,
                TotalPendapatan = analisisList.Sum(a => a.PendapatanKotor),
                TotalBiaya = analisisList.Sum(a => a.TotalBiayaOperasional),
                TotalKeuntungan = analisisList.Sum(a => a.KeuntunganBersih),
                RataRataMargin = analisisList.Average(a => a.MarginKeuntungan),
                RataRataROI = analisisList.Average(a => a.ROI),
                JumlahUntung = analisisList.Count(a => a.StatusKeuntungan == "Untung"),
                JumlahRugi = analisisList.Count(a => a.StatusKeuntungan == "Rugi"),
                JumlahImpas = analisisList.Count(a => a.StatusKeuntungan == "Impas"),
                TotalPendapatanFormatted = $"Rp {analisisList.Sum(a => a.PendapatanKotor):N0}",
                TotalBiayaFormatted = $"Rp {analisisList.Sum(a => a.TotalBiayaOperasional):N0}",
                TotalKeuntunganFormatted = $"Rp {analisisList.Sum(a => a.KeuntunganBersih):N0}",
                Detail = analisisList
            };
        }

        private async Task<(decimal Total, decimal BiayaPakan, decimal BiayaVaksin, decimal BiayaLainnya)> HitungTotalBiayaOperasionalAsync(
            Guid ayamId, DateTime tanggalMasuk, DateTime tanggalPanen, int jumlahEkorPanen, int totalAyamMasuk)
        {
            try
            {
                // Hitung proporsi panen
                var proporsiPanen = totalAyamMasuk > 0 ? (decimal)jumlahEkorPanen / totalAyamMasuk : 1;

                // 1. Biaya dari tabel Biaya (periode ayam masuk sampai panen)
                var biayaLangsung = await _biayaRepository.GetByDateRangeAsync(tanggalMasuk, tanggalPanen);
                var totalBiayaLangsung = biayaLangsung.Sum(b => b.Jumlah) * proporsiPanen;

                // 2. Biaya operasional (pakan dan vaksin) berdasarkan penggunaan aktual
                var operasionals = await _operasionalRepository.GetByDateRangeAsync(tanggalMasuk, tanggalPanen);
                
                // Filter operasional berdasarkan kandang ayam
                var ayam = await _ayamRepository.GetByIdAsync(ayamId);
                if (ayam != null)
                {
                    operasionals = operasionals.Where(o => o.KandangId == ayam.KandangId);
                }

                decimal biayaPakan = 0;
                decimal biayaVaksin = 0;

                foreach (var op in operasionals)
                {
                    if (op.PakanId.HasValue)
                    {
                        // Asumsi harga pakan (bisa diambil dari tabel terpisah jika ada)
                        // Atau bisa menggunakan biaya rata-rata dari tabel Biaya
                        biayaPakan += op.Jumlah * 5000; // Estimasi Rp 5000 per kg pakan
                    }
                    
                    if (op.VaksinId.HasValue)
                    {
                        // Asumsi biaya vaksin per dosis
                        biayaVaksin += op.Jumlah * 2000; // Estimasi Rp 2000 per dosis vaksin
                    }
                }

                // Sesuaikan dengan proporsi panen
                biayaPakan *= proporsiPanen;
                biayaVaksin *= proporsiPanen;

                var totalBiayaOperasional = biayaPakan + biayaVaksin;
                var totalSemuaBiaya = totalBiayaLangsung + totalBiayaOperasional;

                return (totalSemuaBiaya, biayaPakan, biayaVaksin, totalBiayaLangsung);
            }
            catch (Exception ex)
            {
                // Log error jika diperlukan
                Console.WriteLine($"Error menghitung biaya operasional: {ex.Message}");
                return (0, 0, 0, 0);
            }
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