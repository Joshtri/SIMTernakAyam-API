using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Biaya;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/biayas")]
    public class BiayaController : BaseController
    {
        private readonly IBiayaService _biayaService;

        public BiayaController(IBiayaService biayaService)
        {
            _biayaService = biayaService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<BiayaResponseDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var biayas = await _biayaService.GetAllAsync();
                var response = BiayaResponseDto.FromEntities(biayas);
                return Success(response, "Berhasil mengambil semua data biaya.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<BiayaResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var biaya = await _biayaService.GetByIdAsync(id);
                if (biaya == null)
                {
                    return NotFound("Data biaya tidak ditemukan.");
                }

                var response = BiayaResponseDto.FromEntity(biaya);
                return Success(response, "Berhasil mengambil data biaya.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<BiayaResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateBiayaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var biaya = new Models.Biaya
                {
                    JenisBiaya = dto.JenisBiaya,
                    Tanggal = dto.Tanggal,
                    Jumlah = dto.Jumlah,
                    PetugasId = dto.PetugasId,
                    OperasionalId = dto.OperasionalId,
                    BuktiUrl = dto.BuktiUrl
                };

                var result = await _biayaService.CreateAsync(biaya);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = BiayaResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBiayaDto dto)
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

                var biaya = new Models.Biaya
                {
                    Id = dto.Id,
                    JenisBiaya = dto.JenisBiaya,
                    Tanggal = dto.Tanggal,
                    Jumlah = dto.Jumlah,
                    PetugasId = dto.PetugasId,
                    OperasionalId = dto.OperasionalId,
                    BuktiUrl = dto.BuktiUrl
                };

                var result = await _biayaService.UpdateAsync(biaya);

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
                var result = await _biayaService.DeleteAsync(id);

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
