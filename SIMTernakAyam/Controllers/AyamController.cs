using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Ayam;
using SIMTernakAyam.DTOs.Kandang;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.Repository.Interfaces;
using System.Globalization;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/ayams")]
    public class AyamController : BaseController
    {
        private readonly IAyamService _ayamService;
        private readonly IPanenRepository _panenRepository;
        private readonly IMortalitasRepository _mortalitasRepository;

        public AyamController(
            IAyamService ayamService,
            IPanenRepository panenRepository,
            IMortalitasRepository mortalitasRepository)
        {
            _ayamService = ayamService;
            _panenRepository = panenRepository;
            _mortalitasRepository = mortalitasRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<AyamResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? kandangId = null, [FromQuery] string? search = null, [FromQuery] string? period = null)
        {
            try
            {
                // Parse period parameter or default to current month
                bool applyPeriodFilter = true;
                DateTime filterDate = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(period))
                {
                    // Check if user wants to show all data
                    if (period.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        applyPeriodFilter = false;
                    }
                    else
                    {
                        // Try to parse the period (expected format: yyyy-MM)
                        if (DateTime.TryParseExact(period + "-01", "yyyy-MM-dd", null, DateTimeStyles.None, out var parsedDate))
                        {
                            filterDate = parsedDate;
                        }
                        else
                        {
                            return Error("Format period tidak valid. Gunakan format yyyy-MM (contoh: 2026-01) atau 'all' untuk semua data.", 400);
                        }
                    }
                }

                int filterYear = filterDate.Year;
                int filterMonth = filterDate.Month;

                IEnumerable<Models.Ayam> ayams;

                if (kandangId.HasValue && kandangId.Value != Guid.Empty)
                {
                    ayams = await _ayamService.GetAyamByKandangAsync(kandangId.Value);
                }
                else
                {
                    // ? Use method that loads with comprehensive stock info
                    ayams = await _ayamService.GetAllAyamWithStockInfoAsync();
                }

                var ayamList = ayams.ToList();

                // Apply period filter (by month and year of TanggalMasuk) if not "all"
                if (applyPeriodFilter)
                {
                    ayamList = ayamList.Where(a =>
                        a.TanggalMasuk.Year == filterYear &&
                        a.TanggalMasuk.Month == filterMonth
                    ).ToList();
                }

                // Apply free-text search (optional)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    ayamList = ayamList.Where(a =>
                        (a.Kandang != null && !string.IsNullOrEmpty(a.Kandang.NamaKandang) && a.Kandang.NamaKandang.Contains(s, StringComparison.OrdinalIgnoreCase))
                        || a.JumlahMasuk.ToString().Contains(s)
                        || a.TanggalMasuk.ToString("yyyy-MM-dd").Contains(s)
                    ).ToList();
                }

                // ? Get stock data for all ayams in bulk
                var ayamIds = ayamList.Select(a => a.Id).ToList();
                var panenData = ayamIds.Any() ? await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();
                var mortalitasData = ayamIds.Any() ? await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();

                // ? Map to DTO with stock information
                var response = AyamResponseDto.FromEntitiesWithStockData(ayamList, panenData, mortalitasData);
                return Success(response, "Berhasil mengambil data ayam.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<AyamResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var ayam = await _ayamService.GetAyamWithDetailsAsync(id);
                if (ayam == null)
                {
                    return NotFound("Data ayam tidak ditemukan.");
                }

                // ? Get stock data for single ayam
                var jumlahDipanen = await _panenRepository.GetTotalEkorPanenByAyamAsync(id);
                var jumlahMortalitas = await _mortalitasRepository.GetTotalMortalitasByAyamAsync(id);

                var response = AyamResponseDto.FromEntity(ayam, jumlahDipanen, jumlahMortalitas);
                return Success(response, "Berhasil mengambil data ayam.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<AyamResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateAyamDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Validate alasanInput jika forceInput = true
                if (dto.ForceInput && string.IsNullOrWhiteSpace(dto.AlasanInput))
                {
                    return Error("Alasan input wajib diisi jika ForceInput = true.", 400);
                }

                // Get userId from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Error("User ID tidak valid.", 401);
                }

                var ayam = new Models.Ayam
                {
                    KandangId = dto.KandangId,
                    TanggalMasuk = dto.TanggalMasuk,
                    JumlahMasuk = dto.JumlahMasuk
                };

                // Use new CreateWithValidationAsync method
                var result = await _ayamService.CreateWithValidationAsync(ayam, dto.ForceInput, dto.AlasanInput, userId);

                if (!result.Success)
                {
                    // Jika ada info kapasitas, sertakan dalam response
                    if (result.KapasitasInfo != null)
                    {
                        return Error(result.Message, 400, result.KapasitasInfo);
                    }
                    return Error(result.Message, 400);
                }

                // New ayam will have 0 harvest and 0 mortality
                var response = AyamResponseDto.FromEntity(result.Data!, 0, 0);
                return Created(response, result.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get informasi kapasitas dan sisa ayam di kandang
        /// </summary>
        [HttpGet("kandang/{kandangId}/kapasitas")]
        [ProducesResponseType(typeof(Common.ApiResponse<KapasitasKandangDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetKapasitasKandang(Guid kandangId, [FromQuery] string? periodeRencana = null)
        {
            try
            {
                // Parse periode format yyyy-MM
                DateTime? tanggalMasukRencana = null;
                if (!string.IsNullOrWhiteSpace(periodeRencana))
                {
                    // Expected format: yyyy-MM (contoh: 2026-02)
                    if (DateTime.TryParseExact(periodeRencana + "-01", "yyyy-MM-dd", 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        tanggalMasukRencana = parsedDate;
                    }
                    else
                    {
                        return Error("Format periode tidak valid. Gunakan format yyyy-MM (contoh: 2026-02).", 400);
                    }
                }

                var kapasitas = await _ayamService.GetKapasitasKandangAsync(kandangId, tanggalMasukRencana);
                return Success(kapasitas, "Berhasil mengambil informasi kapasitas kandang.");
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAyamDto dto)
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

                var ayam = new Models.Ayam
                {
                    Id = dto.Id,
                    KandangId = dto.KandangId,
                    TanggalMasuk = dto.TanggalMasuk,
                    JumlahMasuk = dto.JumlahMasuk
                };

                var result = await _ayamService.UpdateAsync(ayam);

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
                var result = await _ayamService.DeleteAsync(id);

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

        /// <summary>
        /// Get ayam yang masih ada sisa (stok masih tersedia) di kandang tertentu
        /// </summary>
        [HttpGet("kandang/{kandangId}/sisa")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<AyamResponseDto>>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAyamDenganSisaByKandang(Guid kandangId)
        {
            try
            {
                // Get all ayam in kandang
                var ayams = await _ayamService.GetAyamByKandangAsync(kandangId);
                var ayamList = ayams.ToList();

                if (!ayamList.Any())
                {
                    return NotFound("Tidak ada data ayam di kandang ini.");
                }

                // Get stock data
                var ayamIds = ayamList.Select(a => a.Id).ToList();
                var panenData = await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds);
                var mortalitasData = await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds);

                // Filter only ayam with remaining stock (SisaAyamHidup > 0)
                var ayamDenganSisa = new List<AyamResponseDto>();

                foreach (var ayam in ayamList)
                {
                    var jumlahDipanen = panenData.GetValueOrDefault(ayam.Id, 0);
                    var jumlahMortalitas = mortalitasData.GetValueOrDefault(ayam.Id, 0);
                    var dto = AyamResponseDto.FromEntity(ayam, jumlahDipanen, jumlahMortalitas);

                    // Only include if there's remaining stock
                    if (dto.SisaAyamHidup > 0)
                    {
                        ayamDenganSisa.Add(dto);
                    }
                }

                if (!ayamDenganSisa.Any())
                {
                    return Success(
                        ayamDenganSisa, 
                        "Tidak ada ayam dengan sisa stok di kandang ini. Semua ayam sudah habis (dipanen/mortalitas)."
                    );
                }

                var totalSisa = ayamDenganSisa.Sum(a => a.SisaAyamHidup);
                return Success(
                    ayamDenganSisa, 
                    $"Berhasil mengambil {ayamDenganSisa.Count} record ayam dengan total sisa {totalSisa} ekor."
                );
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
