using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Kandang;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/kandangs")]
    public class KandangController : BaseController
    {
        private readonly IkandangService _kandangService;

        public KandangController(IkandangService kandangService)
        {
            _kandangService = kandangService;
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

                var response = KandangResponseDto.FromEntities(kandangs);
                return Success(response, "Berhasil mengambil semua kandang.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<KandangResponseDto>), 200)]
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

                var response = KandangResponseDto.FromEntity(kandang);
                return Success(response, "Berhasil mengambil kandang.");
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

                var response = KandangResponseDto.FromEntity(result.Data!);
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
