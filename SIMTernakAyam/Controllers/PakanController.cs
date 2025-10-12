using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Pakan;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/pakans")]
    public class PakanController : BaseController
    {
        private readonly IPakanService _pakanService;

        public PakanController(IPakanService pakanService)
        {
            _pakanService = pakanService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<PakanResponseDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var pakans = await _pakanService.GetAllAsync();
                var response = PakanResponseDto.FromEntities(pakans);
                return Success(response, "Berhasil mengambil semua data pakan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<PakanResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var pakan = await _pakanService.GetByIdAsync(id);
                if (pakan == null)
                {
                    return NotFound("Data pakan tidak ditemukan.");
                }

                var response = PakanResponseDto.FromEntity(pakan);
                return Success(response, "Berhasil mengambil data pakan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<PakanResponseDto>>), 200)]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
        {
            try
            {
                var pakans = await _pakanService.GetLowStockPakanAsync(threshold);
                var response = PakanResponseDto.FromEntities(pakans);
                return Success(response, $"Berhasil mengambil data pakan dengan stok rendah (?{threshold}).");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-name/{namaPakan}")]
        [ProducesResponseType(typeof(Common.ApiResponse<PakanResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetByName(string namaPakan)
        {
            try
            {
                var pakan = await _pakanService.GetByNameAsync(namaPakan);
                if (pakan == null)
                {
                    return NotFound($"Pakan dengan nama '{namaPakan}' tidak ditemukan.");
                }

                var response = PakanResponseDto.FromEntity(pakan);
                return Success(response, "Berhasil mengambil data pakan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<PakanResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreatePakanDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var pakan = new Models.Pakan
                {
                    NamaPakan = dto.NamaPakan,
                    Stok = dto.Stok
                };

                var result = await _pakanService.CreateAsync(pakan);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = PakanResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePakanDto dto)
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

                var pakan = new Models.Pakan
                {
                    Id = dto.Id,
                    NamaPakan = dto.NamaPakan,
                    Stok = dto.Stok
                };

                var result = await _pakanService.UpdateAsync(pakan);

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

                var result = await _pakanService.UpdateStokAsync(id, newStok);

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
                var result = await _pakanService.DeleteAsync(id);

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