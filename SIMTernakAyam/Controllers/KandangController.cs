using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Kandang;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SIMTernakAyam.Data;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/kandangs")]
    public class KandangController : BaseController
    {
        private readonly IkandangService _kandangService;
        private readonly IKandangRepository _kandangRepository;
        private readonly ApplicationDbContext _context;

        public KandangController(
            IkandangService kandangService, 
            IKandangRepository kandangRepository,
            ApplicationDbContext context)
        {
            _kandangService = kandangService;
            _kandangRepository = kandangRepository;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<KandangResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            try
            {
                var kandangs = await _kandangService.GetAllAsync();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    kandangs = kandangs.Where(k =>
                        k.NamaKandang.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                        k.Lokasi.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                        (k.User != null && (
                            (!string.IsNullOrEmpty(k.User.FullName) && k.User.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                            k.User.Username.Contains(s, StringComparison.OrdinalIgnoreCase)
                        ))
                    );
                }

                var kandangList = kandangs.ToList();

                // ? Get jumlah ayam hidup untuk setiap kandang
                var ayamHidupData = new Dictionary<Guid, int>();
                foreach (var kandang in kandangList)
                {
                    var jumlahAyamHidup = await _kandangRepository.GetCurrentAyamCountAsync(kandang.Id);
                    ayamHidupData[kandang.Id] = jumlahAyamHidup;
                }

                var response = KandangResponseDto.FromEntitiesWithStockData(kandangList, ayamHidupData);
                return Success(response, "Berhasil mengambil semua kandang.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<KandangDetailDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var kandang = await _kandangService.GetByIdAsync(id);
                if (kandang == null)
                {
                    return NotFound("Kandang tidak ditemukan.");
                }

                // Get jumlah ayam hidup
                var jumlahAyamHidup = await _kandangRepository.GetCurrentAyamCountAsync(id);
                var kapasitasTersedia = Math.Max(0, kandang.Kapasitas - jumlahAyamHidup);
                var persentaseTerisi = kandang.Kapasitas > 0 
                    ? Math.Round((decimal)jumlahAyamHidup / kandang.Kapasitas * 100, 2) 
                    : 0;

                // Tentukan status kapasitas
                string statusKapasitas;
                if (jumlahAyamHidup == 0)
                    statusKapasitas = "Kosong";
                else if (persentaseTerisi >= 100)
                    statusKapasitas = "Penuh";
                else if (persentaseTerisi >= 80)
                    statusKapasitas = "Hampir Penuh";
                else
                    statusKapasitas = "Tersedia";

                // ? Get History Ayam Masuk
                var ayamList = await _context.Ayams
                    .Where(a => a.KandangId == id)
                    .OrderByDescending(a => a.TanggalMasuk)
                    .ToListAsync();

                var historyAyamMasuk = new List<AyamHistoryDto>();
                foreach (var ayam in ayamList)
                {
                    // ⭐ EXCLUDE SOFT DELETED records
                    var totalPanen = await _context.Panens
                        .Where(p => p.AyamId == ayam.Id && !p.IsDeleted)
                        .SumAsync(p => p.JumlahEkorPanen);

                    var totalMortalitas = await _context.Mortalitas
                        .Where(m => m.AyamId == ayam.Id && !m.IsDeleted)
                        .SumAsync(m => m.JumlahKematian);

                    var sisaHidup = Math.Max(0, ayam.JumlahMasuk - totalPanen - totalMortalitas);

                    historyAyamMasuk.Add(new AyamHistoryDto
                    {
                        Id = ayam.Id,
                        TanggalMasuk = ayam.TanggalMasuk,
                        JumlahMasuk = ayam.JumlahMasuk,
                        SisaHidup = sisaHidup
                    });
                }

                // ? NEW: Get Ayam Sisa dari Periode Sebelumnya (bukan batch terbaru yang masih ada sisanya)
                var ayamSisaDetails = new List<AyamSisaDetailDto>();
                
                // Jika ada lebih dari 1 batch ayam, maka batch selain yang terbaru adalah "ayam sisa periode sebelumnya"
                if (ayamList.Count > 1)
                {
                    // Skip batch terbaru (index 0 karena sudah diorder by TanggalMasuk desc)
                    var ayamPeriodeSebelumnya = ayamList.Skip(1).ToList();
                    
                    foreach (var ayamLama in ayamPeriodeSebelumnya)
                    {
                        // ⭐ EXCLUDE SOFT DELETED records
                        var totalPanen = await _context.Panens
                            .Where(p => p.AyamId == ayamLama.Id && !p.IsDeleted)
                            .SumAsync(p => p.JumlahEkorPanen);

                        var totalMortalitas = await _context.Mortalitas
                            .Where(m => m.AyamId == ayamLama.Id && !m.IsDeleted)
                            .SumAsync(m => m.JumlahKematian);

                        var sisaHidup = Math.Max(0, ayamLama.JumlahMasuk - totalPanen - totalMortalitas);
                        
                        // Hanya tampilkan jika masih ada sisa hidup
                        if (sisaHidup > 0)
                        {
                            var umurAyam = (DateTime.UtcNow - ayamLama.TanggalMasuk).Days;
                            
                            ayamSisaDetails.Add(new AyamSisaDetailDto
                            {
                                Id = ayamLama.Id,
                                TanggalMasuk = ayamLama.TanggalMasuk,
                                JumlahMasukAwal = ayamLama.JumlahMasuk,
                                SisaHidup = sisaHidup,
                                AlasanSisa = ayamLama.AlasanSisa ?? $"Sisa dari periode {ayamLama.TanggalMasuk:dd/MM/yyyy}",
                                TanggalDitandaiSisa = ayamLama.TanggalDitandaiSisa,
                                UmurAyam = umurAyam,
                                PerluPerhatian = umurAyam > 60 // Alert jika umur > 60 hari
                            });
                        }
                    }
                }

                // ? Get History Panen
                var ayamIds = ayamList.Select(a => a.Id).ToList();
                var historyPanen = await _context.Panens
                    .Include(p => p.Ayam)
                    .Where(p => ayamIds.Contains(p.AyamId))
                    .OrderByDescending(p => p.TanggalPanen)
                    .Select(p => new PanenHistoryDto
                    {
                        Id = p.Id,
                        TanggalPanen = p.TanggalPanen,
                        JumlahEkorPanen = p.JumlahEkorPanen,
                        BeratRataRata = p.BeratRataRata,
                        TotalBerat = p.JumlahEkorPanen * p.BeratRataRata,
                        Keterangan = null,
                        NamaAyamBatch = $"Batch {p.Ayam.TanggalMasuk:yyyy-MM-dd}"
                    })
                    .ToListAsync();

                // ? Get History Mortalitas
                var historyMortalitas = await _context.Mortalitas
                    .Include(m => m.Ayam)
                    .Where(m => ayamIds.Contains(m.AyamId))
                    .OrderByDescending(m => m.TanggalKematian)
                    .Select(m => new MortalitasHistoryDto
                    {
                        Id = m.Id,
                        TanggalKematian = m.TanggalKematian,
                        JumlahKematian = m.JumlahKematian,
                        PenyebabKematian = m.PenyebabKematian,
                        NamaAyamBatch = $"Batch {m.Ayam.TanggalMasuk:yyyy-MM-dd}"
                    })
                    .ToListAsync();

                // ? Get History Operasional
                var historyOperasional = await _context.Operasionals
                    .Include(o => o.JenisKegiatan)
                    .Include(o => o.Pakan)
                    .Include(o => o.Vaksin)
                    .Include(o => o.Petugas)
                    .Where(o => o.KandangId == id)
                    .OrderByDescending(o => o.Tanggal)
                    .Select(o => new OperasionalHistoryDto
                    {
                        Id = o.Id,
                        Tanggal = o.Tanggal,
                        JenisKegiatan = o.JenisKegiatan.NamaKegiatan,
                        Jumlah = o.Jumlah,
                        Satuan = o.JenisKegiatan.Satuan,
                        ItemNama = o.Pakan != null ? o.Pakan.NamaPakan : o.Vaksin != null ? o.Vaksin.NamaVaksin : null,
                        PetugasNama = o.Petugas.FullName ?? o.Petugas.Username,
                        Keterangan = null
                    })
                    .ToListAsync();

                // Build response dengan history lengkap + ayam sisa
                var response = new KandangDetailDto
                {
                    Id = kandang.Id,
                    NamaKandang = kandang.NamaKandang,
                    Kapasitas = kandang.Kapasitas,
                    Lokasi = kandang.Lokasi,
                    PetugasId = kandang.petugasId,
                    PetugasNama = kandang.User?.FullName ?? kandang.User?.Username,
                    
                    // Informasi Stok
                    JumlahAyamTerisi = jumlahAyamHidup,
                    KapasitasTersedia = kapasitasTersedia,
                    PersentaseTerisi = persentaseTerisi,
                    IsKandangPenuh = jumlahAyamHidup >= kandang.Kapasitas,
                    StatusKapasitas = statusKapasitas,
                    
                    // Summary
                    TotalAyamMasuk = ayamList.Sum(a => a.JumlahMasuk),
                    TotalPanen = historyPanen.Sum(p => p.JumlahEkorPanen),
                    TotalMortalitas = historyMortalitas.Sum(m => m.JumlahKematian),
                    TotalOperasional = historyOperasional.Count,
                    
                    // ? NEW: Ayam Sisa Information
                    AyamSisaList = ayamSisaDetails.OrderByDescending(a => a.TanggalDitandaiSisa).ToList(),
                    TotalAyamSisa = ayamSisaDetails.Sum(a => a.SisaHidup),
                    
                    // History
                    HistoryAyamMasuk = historyAyamMasuk,
                    HistoryPanen = historyPanen,
                    HistoryMortalitas = historyMortalitas,
                    HistoryOperasional = historyOperasional,
                    
                    CreatedAt = kandang.CreatedAt,
                    UpdateAt = kandang.UpdateAt
                };

                return Success(response, "Berhasil mengambil detail kandang dengan history lengkap.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<KandangResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateKandangDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var kandang = new Models.Kandang
                {
                    NamaKandang = dto.NamaKandang,
                    Kapasitas = dto.Kapasitas,
                    Lokasi = dto.Lokasi,
                    petugasId = dto.PetugasId
                };

                var result = await _kandangService.CreateAsync(kandang);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                // Kandang baru pasti kosong (0 ayam)
                var response = KandangResponseDto.FromEntity(result.Data!, 0);
                return Created(response, result.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateKandangDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                if (id != dto.Id)
                {
                    return Error("ID di URL tidak sesuai dengan ID di body.", 400);
                }
                                    
                var kandang = new Models.Kandang
                {
                    Id = dto.Id,
                    NamaKandang = dto.NamaKandang,
                    Kapasitas = dto.Kapasitas,
                    Lokasi = dto.Lokasi,
                    petugasId = dto.PetugasId
                };

                var result = await _kandangService.UpdateAsync(kandang);

                if (!result.Success)
                {
                    if (result.Message.Contains("tidak ditemukan"))
                    {
                        return NotFound(result.Message);
                    }
                    return Error(result.Message, 400);
                }

                return Success(result.Message, 200);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _kandangService.DeleteAsync(id);

                if (!result.Success)
                {
                    if (result.Message.Contains("tidak ditemukan"))
                    {
                        return NotFound(result.Message);
                    }
                    return Error(result.Message, 400);
                }

                return Success(result.Message, 200);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}