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
    }
}
