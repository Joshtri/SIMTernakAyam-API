using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Vaksin;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/vaksins")]
    public class VaksinController : BaseController
    {
        private readonly IVaksinService _vaksinService;

        public VaksinController(IVaksinService vaksinService)
        {
            _vaksinService = vaksinService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<VaksinResponseDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var vaksins = await _vaksinService.GetAllAsync();
                var response = VaksinResponseDto.FromEntities(vaksins);
                return Success(response, "Berhasil mengambil semua data vaksin.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("with-usage-detail")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<VaksinResponseDto>>), 200)]
        public async Task<IActionResult> GetAllWithUsageDetail()
        {
            try
            {
                var vaksinsWithUsage = await _vaksinService.GetAllVaksinWithUsageDetailAsync();
                return Success(vaksinsWithUsage, "Berhasil mengambil semua data vaksin dengan detail penggunaan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}/usage-detail")]
        [ProducesResponseType(typeof(Common.ApiResponse<VaksinResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetUsageDetailById(Guid id)
        {
            try
            {
                var vaksinWithUsage = await _vaksinService.GetVaksinWithUsageDetailAsync(id);
                if (vaksinWithUsage == null)
                {
                    return NotFound("Data vaksin tidak ditemukan.");
                }

                return Success(vaksinWithUsage, "Berhasil mengambil data vaksin dengan detail penggunaan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}/check-availability/{jumlahDibutuhkan}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> CheckAvailability(Guid id, int jumlahDibutuhkan)
        {
            try
            {
                var availability = await _vaksinService.CheckStockAvailabilityAsync(id, jumlahDibutuhkan);
                if (availability == null)
                {
                    return NotFound("Data vaksin tidak ditemukan.");
                }

                return Success(availability, "Berhasil mengecek ketersediaan stok vaksin.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<VaksinResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var vaksin = await _vaksinService.GetByIdAsync(id);
                if (vaksin == null)
                {
                    return NotFound("Data vaksin tidak ditemukan.");
                }

                var response = VaksinResponseDto.FromEntity(vaksin);
                return Success(response, "Berhasil mengambil data vaksin.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<VaksinResponseDto>>), 200)]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5)
        {
            try
            {
                var vaksins = await _vaksinService.GetLowStockVaksinAsync(threshold);
                var response = VaksinResponseDto.FromEntities(vaksins);
                return Success(response, $"Berhasil mengambil data vaksin dengan stok rendah (?{threshold}).");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-name/{namaVaksin}")]
        [ProducesResponseType(typeof(Common.ApiResponse<VaksinResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetByName(string namaVaksin)
        {
            try
            {
                var vaksin = await _vaksinService.GetByNameAsync(namaVaksin);
                if (vaksin == null)
                {
                    return NotFound($"Vaksin dengan nama '{namaVaksin}' tidak ditemukan.");
                }

                var response = VaksinResponseDto.FromEntity(vaksin);
                return Success(response, "Berhasil mengambil data vaksin.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-type/{tipe}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<VaksinResponseDto>>), 200)]
        public async Task<IActionResult> GetByType(VaksinVitaminTypeEnum tipe)
        {
            try
            {
                var vaksins = await _vaksinService.GetByTypeAsync(tipe);
                var response = VaksinResponseDto.FromEntities(vaksins);
                return Success(response, $"Berhasil mengambil data {tipe}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<VaksinResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateVaksinDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var vaksin = new Models.Vaksin
                {
                    NamaVaksin = dto.NamaVaksin,
                    Stok = dto.Stok,
                    Tahun = dto.Tahun,
                    Bulan = dto.Bulan,
                    Tipe = dto.Tipe
                };

                var result = await _vaksinService.CreateAsync(vaksin);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = VaksinResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaksinDto dto)
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

                var vaksin = new Models.Vaksin
                {
                    Id = dto.Id,
                    NamaVaksin = dto.NamaVaksin,
                    Stok = dto.Stok,
                    Tahun = dto.Tahun,
                    Bulan = dto.Bulan,
                    Tipe = dto.Tipe
                };

                var result = await _vaksinService.UpdateAsync(vaksin);

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

        [HttpPut("{id}/update-stock")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int newStok)
        {
            try
            {
                if (newStok < 0)
                {
                    return Error("Stok tidak boleh negatif.", 400);
                }

                var result = await _vaksinService.UpdateStokAsync(id, newStok);

                if (!result.Success)
                {
                    return NotFound(result.Message);
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
                var result = await _vaksinService.DeleteAsync(id);

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