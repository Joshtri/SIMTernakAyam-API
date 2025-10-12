using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Operasional;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/operasionals")]
    public class OperasionalController : BaseController
    {
        private readonly IOperasionalService _operasionalService;

        public OperasionalController(IOperasionalService operasionalService)
        {
            _operasionalService = operasionalService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<OperasionalResponseDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var operasionals = await _operasionalService.GetAllAsync();
                var response = OperasionalResponseDto.FromEntities(operasionals);
                return Success(response, "Berhasil mengambil semua data operasional.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<OperasionalResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var operasional = await _operasionalService.GetByIdAsync(id);
                if (operasional == null)
                {
                    return NotFound("Data operasional tidak ditemukan.");
                }

                var response = OperasionalResponseDto.FromEntity(operasional);
                return Success(response, "Berhasil mengambil data operasional.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-kandang/{kandangId}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<OperasionalResponseDto>>), 200)]
        public async Task<IActionResult> GetByKandangId(Guid kandangId)
        {
            try
            {
                var operasionals = await _operasionalService.GetOperasionalByKandangAsync(kandangId);
                var response = OperasionalResponseDto.FromEntities(operasionals);
                return Success(response, "Berhasil mengambil data operasional berdasarkan kandang.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-jenis-kegiatan/{jenisKegiatanId}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<OperasionalResponseDto>>), 200)]
        public async Task<IActionResult> GetByJenisKegiatanId(Guid jenisKegiatanId)
        {
            try
            {
                var operasionals = await _operasionalService.GetOperasionalByJenisKegiatanAsync(jenisKegiatanId);
                var response = OperasionalResponseDto.FromEntities(operasionals);
                return Success(response, "Berhasil mengambil data operasional berdasarkan jenis kegiatan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-date-range")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<OperasionalResponseDto>>), 200)]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return Error("Tanggal mulai tidak boleh lebih besar dari tanggal akhir.", 400);
                }

                var operasionals = await _operasionalService.GetOperasionalByPeriodAsync(startDate, endDate);
                var response = OperasionalResponseDto.FromEntities(operasionals);
                return Success(response, $"Berhasil mengambil data operasional dari {startDate:yyyy-MM-dd} sampai {endDate:yyyy-MM-dd}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<OperasionalResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateOperasionalDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var operasional = new Models.Operasional
                {
                    JenisKegiatanId = dto.JenisKegiatanId,
                    Tanggal = dto.Tanggal,
                    Jumlah = dto.Jumlah,
                    PetugasId = dto.PetugasId,
                    KandangId = dto.KandangId,
                    PakanId = dto.PakanId,
                    VaksinId = dto.VaksinId
                };

                var result = await _operasionalService.CreateAsync(operasional);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = OperasionalResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOperasionalDto dto)
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

                var operasional = new Models.Operasional
                {
                    Id = dto.Id,
                    JenisKegiatanId = dto.JenisKegiatanId,
                    Tanggal = dto.Tanggal,
                    Jumlah = dto.Jumlah,
                    PetugasId = dto.PetugasId,
                    KandangId = dto.KandangId,
                    PakanId = dto.PakanId,
                    VaksinId = dto.VaksinId
                };

                var result = await _operasionalService.UpdateAsync(operasional);

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
                var result = await _operasionalService.DeleteAsync(id);

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