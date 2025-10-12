using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Mortalitas;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/mortalitas")]
    public class MortalitasController : BaseController
    {
        private readonly IMortalitasService _mortalitasService;

        public MortalitasController(IMortalitasService mortalitasService)
        {
            _mortalitasService = mortalitasService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<MortalitasResponseDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var mortalitas = await _mortalitasService.GetAllAsync();
                var response = MortalitasResponseDto.FromEntities(mortalitas);
                return Success(response, "Berhasil mengambil semua data mortalitas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<MortalitasResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var mortalitas = await _mortalitasService.GetByIdAsync(id);
                if (mortalitas == null)
                {
                    return NotFound("Data mortalitas tidak ditemukan.");
                }

                var response = MortalitasResponseDto.FromEntity(mortalitas);
                return Success(response, "Berhasil mengambil data mortalitas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<MortalitasResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateMortalitasDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var mortalitas = new Models.Mortalitas
                {
                    AyamId = dto.AyamId,
                    TanggalKematian = dto.TanggalKematian,
                    JumlahKematian = dto.JumlahKematian,
                    PenyebabKematian = dto.PenyebabKematian
                };

                var result = await _mortalitasService.CreateAsync(mortalitas);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = MortalitasResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMortalitasDto dto)
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

                var mortalitas = new Models.Mortalitas
                {
                    Id = dto.Id,
                    AyamId = dto.AyamId,
                    TanggalKematian = dto.TanggalKematian,
                    JumlahKematian = dto.JumlahKematian,
                    PenyebabKematian = dto.PenyebabKematian
                };

                var result = await _mortalitasService.UpdateAsync(mortalitas);

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
                var result = await _mortalitasService.DeleteAsync(id);

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
