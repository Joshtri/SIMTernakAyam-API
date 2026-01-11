using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Panen;
using SIMTernakAyam.Services.Interfaces;
using System.Globalization;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/panens")]
    public class PanenController : BaseController
    {
        private readonly IPanenService _panenService;

        public PanenController(IPanenService panenService)
        {
            _panenService = panenService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<PanenResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? kandangId = null, [FromQuery] string? period = null)
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

                var panens = await _panenService.GetAllAsync();
                var panenList = panens.ToList();

                // Apply kandangId filter if provided
                if (kandangId.HasValue && kandangId.Value != Guid.Empty)
                {
                    panenList = panenList.Where(p => p.Ayam != null && p.Ayam.KandangId == kandangId.Value).ToList();
                }

                // Apply period filter (by month and year of TanggalPanen) if not "all"
                if (applyPeriodFilter)
                {
                    panenList = panenList.Where(p =>
                        p.TanggalPanen.Year == filterYear &&
                        p.TanggalPanen.Month == filterMonth
                    ).ToList();
                }

                var response = PanenResponseDto.FromEntities(panenList);
                return Success(response, "Berhasil mengambil semua panen.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<PanenResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var panen = await _panenService.GetByIdAsync(id);
                if (panen == null)
                {
                    return NotFound("Panen tidak ditemukan.");
                }

                var response = PanenResponseDto.FromEntity(panen);
                return Success(response, "Berhasil mengambil panen.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Create panen dengan mode support (auto-fifo atau manual-split)
        /// RECOMMENDED: Gunakan mode "manual-split"
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<List<PanenResponseDto>>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> Create([FromBody] CreatePanenWithModeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Validate mode
                if (dto.Mode != "auto-fifo" && dto.Mode != "manual-split")
                {
                    return Error("Mode harus 'auto-fifo' atau 'manual-split'.", 400);
                }

                // Validate manual-split specific requirements
                if (dto.Mode == "manual-split")
                {
                    if (!dto.JumlahDariAyamLama.HasValue || !dto.JumlahDariAyamBaru.HasValue)
                    {
                        return Error("Mode 'manual-split' memerlukan JumlahDariAyamLama dan JumlahDariAyamBaru.", 400);
                    }

                    var totalManual = dto.JumlahDariAyamLama.Value + dto.JumlahDariAyamBaru.Value;
                    if (totalManual != dto.JumlahEkorPanen)
                    {
                        return Error(
                            $"Total JumlahDariAyamLama ({dto.JumlahDariAyamLama.Value}) + " +
                            $"JumlahDariAyamBaru ({dto.JumlahDariAyamBaru.Value}) = {totalManual} " +
                            $"harus sama dengan JumlahEkorPanen ({dto.JumlahEkorPanen}).", 400);
                    }

                    if (dto.JumlahDariAyamLama.Value < 0 || dto.JumlahDariAyamBaru.Value < 0)
                    {
                        return Error("Jumlah panen tidak boleh negatif.", 400);
                    }

                    if (totalManual <= 0)
                    {
                        return Error("Total jumlah panen harus lebih dari 0.", 400);
                    }
                }

                // Pilih method berdasarkan mode
                List<Models.Panen>? panenList;
                string message;
                bool success;

                if (dto.Mode == "auto-fifo")
                {
                    // AUTO FIFO MODE
                    var result = await _panenService.CreatePanenAutoFifoAsync(
                        dto.KandangId,
                        dto.TanggalPanen,
                        dto.JumlahEkorPanen,
                        dto.BeratRataRata);

                    success = result.Success;
                    message = result.Message;
                    panenList = result.Data;
                }
                else
                {
                    // MANUAL SPLIT MODE
                    var result = await _panenService.CreatePanenManualSplitAsync(
                        dto.KandangId,
                        dto.TanggalPanen,
                        dto.JumlahDariAyamLama!.Value,
                        dto.JumlahDariAyamBaru!.Value,
                        dto.BeratRataRata);

                    success = result.Success;
                    message = result.Message;
                    panenList = result.Data;
                }

                if (!success)
                {
                    return Error(message, 400);
                }

                // Convert to response DTOs
                var responseDtos = PanenResponseDto.FromEntities(panenList!);

                return Created(responseDtos, message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Create panen single record (original method - untuk backward compatibility)
        /// </summary>
        [HttpPost("single")]
        [ProducesResponseType(typeof(Common.ApiResponse<PanenResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> CreateSingle([FromBody] CreatePanenDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var panen = new Models.Panen
                {
                    AyamId = dto.AyamId,
                    TanggalPanen = dto.TanggalPanen,
                    JumlahEkorPanen = dto.JumlahEkorPanen,
                    BeratRataRata = dto.BeratRataRata
                };

                var result = await _panenService.CreateAsync(panen);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = PanenResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePanenDto dto)
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

                var panen = new Models.Panen
                {
                    Id = dto.Id,
                    AyamId = dto.AyamId,
                    TanggalPanen = dto.TanggalPanen,
                    JumlahEkorPanen = dto.JumlahEkorPanen,
                    BeratRataRata = dto.BeratRataRata
                };

                var result = await _panenService.UpdateAsync(panen);

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
                var result = await _panenService.DeleteAsync(id);

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

        [HttpGet("stok-ayam/{ayamId}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetStokAyam(Guid ayamId)
        {
            try
            {
                var stokInfo = await _panenService.GetStokAyamAsync(ayamId);
                
                if (stokInfo.TotalMasuk == 0)
                {
                    return NotFound("Data ayam tidak ditemukan.");
                }

                var response = new
                {
                    ayamId = ayamId,
                    totalMasuk = stokInfo.TotalMasuk,
                    sudahDipanen = stokInfo.SudahDipanen,
                    sisaTersedia = stokInfo.SisaTersedia,
                    persentaseDipanen = stokInfo.TotalMasuk > 0 ? Math.Round((decimal)stokInfo.SudahDipanen / stokInfo.TotalMasuk * 100, 2) : 0,
                    bisaDipanen = stokInfo.SisaTersedia > 0
                };

                return Success(response, "Berhasil mengambil informasi stok ayam.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan analisis keuntungan untuk panen tertentu
        /// </summary>
        /// <param name="id">ID panen</param>
        /// <returns>Detail analisis keuntungan</returns>
        [HttpGet("{id}/analisis-keuntungan")]
        [ProducesResponseType(typeof(Common.ApiResponse<AnalisisKeuntunganDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAnalisisKeuntungan(Guid id)
        {
            try
            {
                var analisis = await _panenService.GetAnalisisKeuntunganAsync(id);
                if (analisis == null)
                {
                    return NotFound("Panen tidak ditemukan atau belum ada harga pasar yang tersedia.");
                }

                return Success(analisis, "Berhasil mengambil analisis keuntungan panen.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan analisis keuntungan untuk beberapa panen dalam periode tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai (format: yyyy-MM-dd)</param>
        /// <param name="endDate">Tanggal akhir (format: yyyy-MM-dd)</param>
        /// <param name="kandangId">ID kandang (opsional)</param>
        /// <returns>List analisis keuntungan</returns>
        [HttpGet("analisis-keuntungan")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<AnalisisKeuntunganDto>>), 200)]
        public async Task<IActionResult> GetAnalisisKeuntunganByPeriod(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate, 
            [FromQuery] Guid? kandangId = null)
        {
            try
            {
                if (startDate > endDate)
                {
                    return Error("Tanggal mulai tidak boleh lebih besar dari tanggal akhir.", 400);
                }

                var analisisList = await _panenService.GetAnalisisKeuntunganByPeriodAsync(startDate, endDate, kandangId);
                
                var message = kandangId.HasValue 
                    ? $"Berhasil mengambil analisis keuntungan untuk kandang tertentu dari {startDate:dd/MM/yyyy} sampai {endDate:dd/MM/yyyy}."
                    : $"Berhasil mengambil analisis keuntungan dari {startDate:dd/MM/yyyy} sampai {endDate:dd/MM/yyyy}.";

                return Success(analisisList, message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan ringkasan keuntungan dalam periode tertentu
        /// </summary>
        /// <param name="startDate">Tanggal mulai (format: yyyy-MM-dd)</param>
        /// <param name="endDate">Tanggal akhir (format: yyyy-MM-dd)</param>
        /// <returns>Ringkasan keuntungan</returns>
        [HttpGet("ringkasan-keuntungan")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        public async Task<IActionResult> GetRingkasanKeuntungan(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return Error("Tanggal mulai tidak boleh lebih besar dari tanggal akhir.", 400);
                }

                var ringkasan = await _panenService.GetRingkasanKeuntunganAsync(startDate, endDate);
                return Success(ringkasan, $"Berhasil mengambil ringkasan keuntungan dari {startDate:dd/MM/yyyy} sampai {endDate:dd/MM/yyyy}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
