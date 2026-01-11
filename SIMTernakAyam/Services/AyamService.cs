using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;
using SIMTernakAyam.DTOs.Kandang;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using System.Globalization;

namespace SIMTernakAyam.Services
{
    public class AyamService : BaseService<Ayam>, IAyamService
    {
        private readonly IAyamRepository _ayamRepository;
        private readonly IKandangRepository _kandangRepository;
        private readonly IPanenRepository _panenRepository;
        private readonly IMortalitasRepository _mortalitasRepository;
        private readonly ApplicationDbContext _context;

        public AyamService(
            IAyamRepository repository,
            IKandangRepository kandangRepository,
            IPanenRepository panenRepository,
            IMortalitasRepository mortalitasRepository,
            ApplicationDbContext context) : base(repository)
        {
            _ayamRepository = repository;
            _kandangRepository = kandangRepository;
            _panenRepository = panenRepository;
            _mortalitasRepository = mortalitasRepository;
            _context = context;
        }

        public async Task<IEnumerable<Ayam>> GetAyamByKandangAsync(Guid kandangId)
        {
            return await _ayamRepository.GetByKandangIdAsync(kandangId);
        }

        // ? FIXED: Match interface method name
        public async Task<int> GetTotalAyamCountInKandangAsync(Guid kandangId)
        {
            return await _ayamRepository.GetTotalAyamInKandangAsync(kandangId);
        }

        public async Task<IEnumerable<Ayam>> GetAllAyamWithDetailsAsync()
        {
            return await _ayamRepository.GetAyamWithKandangAsync();
        }

        public async Task<Ayam?> GetAyamWithDetailsAsync(Guid id)
        {
            return await _ayamRepository.GetWithDetailsAsync(id);
        }

        // ? NEW: Get ayam with comprehensive stock information
        public async Task<IEnumerable<Ayam>> GetAllAyamWithStockInfoAsync()
        {
            // Get all ayam with relations
            var ayams = await _ayamRepository.GetAllAsync();
            var ayamList = ayams.ToList();

            if (!ayamList.Any())
                return ayamList;

            // Get ayam IDs for bulk queries
            var ayamIds = ayamList.Select(a => a.Id).ToList();

            // Get panen and mortalitas data in bulk (efficient queries)
            var panenData = await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds);
            var mortalitasData = await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds);

            // Note: The stock calculations will be done in the DTO mapping
            // Return ayams with loaded relations - DTO will calculate stock info
            return ayamList;
        }

        public async Task<(bool Success, string Message, Ayam? Data)> AddAyamToKandangAsync(Ayam ayam)
        {
            // Check kandang capacity before adding
            var isAvailable = await _kandangRepository.IsKandangAvailableAsync(ayam.KandangId, ayam.JumlahMasuk);
            if (!isAvailable)
            {
                return (false, "Kapasitas kandang tidak mencukupi untuk menambah ayam sebanyak ini.", null);
            }

            return await CreateAsync(ayam);
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Ayam entity)
        {
            if (entity.JumlahMasuk <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah ayam masuk harus lebih dari 0." };
            }

            //if (entity.TanggalMasuk > DateTime.UtcNow)
            //{
            //    return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal masuk tidak boleh di masa depan." };
            //}

            // Check if kandang exists
            var kandang = await _kandangRepository.GetByIdAsync(entity.KandangId);
            if (kandang == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Kandang tidak ditemukan." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(Ayam entity)
        {
            // Ensure TanggalMasuk is UTC
            if (entity.TanggalMasuk.Kind != DateTimeKind.Utc)
            {
                entity.TanggalMasuk = entity.TanggalMasuk.ToUniversalTime();
            }
            await Task.CompletedTask;
        }

        public async Task<KapasitasKandangDto> GetKapasitasKandangAsync(Guid kandangId, DateTime? tanggalMasukRencana = null)
        {
            var kandang = await _kandangRepository.GetByIdAsync(kandangId);
            if (kandang == null)
            {
                throw new Exception("Kandang tidak ditemukan.");
            }

            // Get all ayam di kandang
            var ayamList = await _ayamRepository.GetByKandangIdAsync(kandangId);
            var ayamIds = ayamList.Select(a => a.Id).ToList();

            // Get panen dan mortalitas data
            var panenData = ayamIds.Any() ? await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();
            var mortalitasData = ayamIds.Any() ? await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();

            // Calculate total ayam hidup
            int totalAyamHidup = 0;
            foreach (var ayam in ayamList)
            {
                var dipanen = panenData.ContainsKey(ayam.Id) ? panenData[ayam.Id] : 0;
                var mati = mortalitasData.ContainsKey(ayam.Id) ? mortalitasData[ayam.Id] : 0;
                var hidup = ayam.JumlahMasuk - dipanen - mati;
                totalAyamHidup += hidup;
            }

            // LOGIC BARU: Jika ada tanggalMasukRencana, cek ayam SEBELUM periode rencana
            int sisaDariPeriodeSebelumnya = 0;
            string? periodeAyamSisa = null;
            bool adaSisaAyam = false;

            if (tanggalMasukRencana.HasValue)
            {
                // Buat tanggal awal periode rencana (tanggal 1 bulan tersebut)
                var periodeRencanaStart = new DateTime(
                    tanggalMasukRencana.Value.Year, 
                    tanggalMasukRencana.Value.Month, 
                    1, 0, 0, 0, DateTimeKind.Utc);

                // Cek ayam yang TanggalMasuk SEBELUM periode rencana
                var ayamSebelumPeriodeRencana = ayamList
                    .Where(a => a.TanggalMasuk < periodeRencanaStart)
                    .OrderByDescending(a => a.TanggalMasuk) // Urutkan dari yang paling baru
                    .ToList();

                if (ayamSebelumPeriodeRencana.Any())
                {
                    foreach (var ayam in ayamSebelumPeriodeRencana)
                    {
                        var dipanen = panenData.ContainsKey(ayam.Id) ? panenData[ayam.Id] : 0;
                        var mati = mortalitasData.ContainsKey(ayam.Id) ? mortalitasData[ayam.Id] : 0;
                        var hidup = ayam.JumlahMasuk - dipanen - mati;
                        if (hidup > 0)
                        {
                            sisaDariPeriodeSebelumnya += hidup;
                        }
                    }

                    if (sisaDariPeriodeSebelumnya > 0)
                    {
                        adaSisaAyam = true;
                        // Get periode dari ayam yang paling baru (tapi masih sebelum periode rencana)
                        var ayamTerbaru = ayamSebelumPeriodeRencana.FirstOrDefault();
                        if (ayamTerbaru != null)
                        {
                            periodeAyamSisa = ayamTerbaru.TanggalMasuk.ToString("MMMM yyyy", new CultureInfo("id-ID"));
                        }
                    }
                }
            }
            else
            {
                // LOGIC LAMA: Cek ayam yang sudah di-flag sebagai IsAyamSisa
                var ayamSisa = ayamList.Where(a => a.IsAyamSisa).ToList();

                if (ayamSisa.Any())
                {
                    foreach (var ayam in ayamSisa)
                    {
                        var dipanen = panenData.ContainsKey(ayam.Id) ? panenData[ayam.Id] : 0;
                        var mati = mortalitasData.ContainsKey(ayam.Id) ? mortalitasData[ayam.Id] : 0;
                        var hidup = ayam.JumlahMasuk - dipanen - mati;
                        sisaDariPeriodeSebelumnya += hidup;
                    }

                    // Get periode dari ayam sisa terlama
                    var ayamSisaTerlama = ayamSisa.OrderBy(a => a.TanggalMasuk).FirstOrDefault();
                    if (ayamSisaTerlama != null)
                    {
                        periodeAyamSisa = ayamSisaTerlama.TanggalMasuk.ToString("MMMM yyyy", new CultureInfo("id-ID"));
                    }
                }

                adaSisaAyam = sisaDariPeriodeSebelumnya > 0;
            }

            var kapasitasTersedia = kandang.Kapasitas - totalAyamHidup;
            var persentasePengisian = kandang.Kapasitas > 0 ? (decimal)totalAyamHidup / kandang.Kapasitas * 100 : 0;

            return new KapasitasKandangDto
            {
                KandangId = kandang.Id,
                NamaKandang = kandang.NamaKandang,
                KapasitasKandang = kandang.Kapasitas,
                TotalAyamHidup = totalAyamHidup,
                SisaAyamDariPeriodeSebelumnya = sisaDariPeriodeSebelumnya,
                KapasitasTersedia = kapasitasTersedia,
                PeriodeAyamSisa = periodeAyamSisa,
                AdaSisaAyam = adaSisaAyam,
                PersentasePengisian = Math.Round(persentasePengisian, 2)
            };
        }

        public async Task<List<Ayam>> GetAyamHidupFIFOAsync(Guid kandangId)
        {
            // Get all ayam di kandang, sorted by TanggalMasuk (FIFO)
            var ayamList = (await _ayamRepository.GetByKandangIdAsync(kandangId))
                .OrderBy(a => a.TanggalMasuk)
                .ThenBy(a => a.IsAyamSisa ? 0 : 1) // Prioritas ayam sisa dulu
                .ToList();

            var ayamIds = ayamList.Select(a => a.Id).ToList();
            var panenData = ayamIds.Any() ? await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();
            var mortalitasData = ayamIds.Any() ? await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();

            // Filter hanya ayam yang masih hidup
            var ayamHidup = new List<Ayam>();
            foreach (var ayam in ayamList)
            {
                var dipanen = panenData.ContainsKey(ayam.Id) ? panenData[ayam.Id] : 0;
                var mati = mortalitasData.ContainsKey(ayam.Id) ? mortalitasData[ayam.Id] : 0;
                var hidup = ayam.JumlahMasuk - dipanen - mati;

                if (hidup > 0)
                {
                    ayamHidup.Add(ayam);
                }
            }

            return ayamHidup;
        }

        /// <summary>
        /// Get ayam by tipe periode: "lama" (IsAyamSisa=true) atau "baru" (IsAyamSisa=false)
        /// </summary>
        public async Task<List<(Ayam Ayam, int JumlahHidup)>> GetAyamByPeriodeTypeAsync(Guid kandangId, bool isAyamLama)
        {
            // Get all ayam di kandang
            var ayamList = (await _ayamRepository.GetByKandangIdAsync(kandangId))
                .Where(a => a.IsAyamSisa == isAyamLama)
                .OrderBy(a => a.TanggalMasuk)
                .ToList();

            if (!ayamList.Any())
                return new List<(Ayam, int)>();

            var ayamIds = ayamList.Select(a => a.Id).ToList();
            var panenData = await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds);
            var mortalitasData = await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds);

            // Calculate jumlah hidup untuk setiap ayam
            var result = new List<(Ayam Ayam, int JumlahHidup)>();
            foreach (var ayam in ayamList)
            {
                var dipanen = panenData.ContainsKey(ayam.Id) ? panenData[ayam.Id] : 0;
                var mati = mortalitasData.ContainsKey(ayam.Id) ? mortalitasData[ayam.Id] : 0;
                var hidup = ayam.JumlahMasuk - dipanen - mati;

                if (hidup > 0)
                {
                    result.Add((ayam, hidup));
                }
            }

            return result;
        }

        public async Task<(bool Success, string Message, Ayam? Data, KapasitasKandangDto? KapasitasInfo)> CreateWithValidationAsync(
            Ayam ayam,
            bool forceInput,
            string? alasanInput,
            Guid petugasId)
        {
            // 1. Get kapasitas info dengan tanggal masuk yang direncanakan
            var kapasitasInfo = await GetKapasitasKandangAsync(ayam.KandangId, ayam.TanggalMasuk);

            // 2. Check apakah ada sisa ayam
            if (kapasitasInfo.AdaSisaAyam && !forceInput)
            {
                // Ada sisa ayam dan user tidak force input
                // Return error dengan info kapasitas
                return (false,
                    $"Kandang masih memiliki {kapasitasInfo.SisaAyamDariPeriodeSebelumnya} ayam sisa dari periode sebelumnya ({kapasitasInfo.PeriodeAyamSisa}). Maksimal input: {kapasitasInfo.KapasitasTersedia} ekor.",
                    null,
                    kapasitasInfo);
            }

            // 3. Check kapasitas
            if (ayam.JumlahMasuk > kapasitasInfo.KapasitasTersedia)
            {
                return (false,
                    $"Jumlah input ({ayam.JumlahMasuk} ekor) melebihi kapasitas tersedia ({kapasitasInfo.KapasitasTersedia} ekor).",
                    null,
                    kapasitasInfo);
            }

            // 4. Jika forceInput dan ada sisa ayam, flag ayam lama sebagai IsAyamSisa
            if (forceInput && kapasitasInfo.AdaSisaAyam)
            {
                if (string.IsNullOrWhiteSpace(alasanInput))
                {
                    return (false, "Alasan input wajib diisi jika ada sisa ayam dari periode sebelumnya.", null, kapasitasInfo);
                }

                // Flag semua ayam yang belum di-flag sebagai sisa
                var ayamLama = await _context.Ayams
                    .Where(a => a.KandangId == ayam.KandangId && !a.IsAyamSisa)
                    .ToListAsync();

                foreach (var ayamOld in ayamLama)
                {
                    ayamOld.IsAyamSisa = true;
                    ayamOld.AlasanSisa = alasanInput;
                    ayamOld.TanggalDitandaiSisa = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Log ke LogPeriodeKandang
                var kandang = await _kandangRepository.GetByIdAsync(ayam.KandangId);
                var logPeriode = new LogPeriodeKandang
                {
                    KandangId = ayam.KandangId,
                    Periode = ayam.TanggalMasuk.ToString("MMMM yyyy", new CultureInfo("id-ID")),
                    TanggalInput = DateTime.UtcNow,
                    JumlahInputBaru = ayam.JumlahMasuk,
                    SisaDariPeriodeSebelumnya = kapasitasInfo.SisaAyamDariPeriodeSebelumnya,
                    AlasanAdaSisa = alasanInput,
                    PetugasId = petugasId,
                    KapasitasKandang = kandang!.Kapasitas,
                    TotalAyamSetelahInput = kapasitasInfo.TotalAyamHidup + ayam.JumlahMasuk
                };

                await _context.LogPeriodeKandangs.AddAsync(logPeriode);
                await _context.SaveChangesAsync();
            }

            // 5. Create ayam baru
            var result = await CreateAsync(ayam);

            return (result.Success, result.Message, result.Data, kapasitasInfo);
        }
    }
}