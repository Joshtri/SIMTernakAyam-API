using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Panen;
using SIMTernakAyam.Services.Interfaces;

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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var panens = await _panenService.GetAllAsync();
                var response = PanenResponseDto.FromEntities(panens);
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

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<PanenResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreatePanenDto dto)
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
