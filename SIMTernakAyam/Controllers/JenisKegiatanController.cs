using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.JenisKegiatan;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/jenis-kegiatans")]
    public class JenisKegiatanController : BaseController
    {
        private readonly IJenisKegiatanService _jenisKegiatanService;

        public JenisKegiatanController(IJenisKegiatanService jenisKegiatanService)
        {
            _jenisKegiatanService = jenisKegiatanService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<JenisKegiatanResponseDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var jenisKegiatans = await _jenisKegiatanService.GetAllAsync();
                var response = JenisKegiatanResponseDto.FromEntities(jenisKegiatans);
                return Success(response, "Berhasil mengambil semua data jenis kegiatan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<JenisKegiatanResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var jenisKegiatan = await _jenisKegiatanService.GetByIdAsync(id);
                if (jenisKegiatan == null)
                {
                    return NotFound("Data jenis kegiatan tidak ditemukan.");
                }

                var response = JenisKegiatanResponseDto.FromEntity(jenisKegiatan);
                return Success(response, "Berhasil mengambil data jenis kegiatan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-name/{namaKegiatan}")]
        [ProducesResponseType(typeof(Common.ApiResponse<JenisKegiatanResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetByName(string namaKegiatan)
        {
            try
            {
                var jenisKegiatan = await _jenisKegiatanService.GetByNameAsync(namaKegiatan);
                if (jenisKegiatan == null)
                {
                    return NotFound($"Jenis kegiatan dengan nama '{namaKegiatan}' tidak ditemukan.");
                }

                var response = JenisKegiatanResponseDto.FromEntity(jenisKegiatan);
                return Success(response, "Berhasil mengambil data jenis kegiatan.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("by-satuan/{satuan}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<JenisKegiatanResponseDto>>), 200)]
        public async Task<IActionResult> GetBySatuan(string satuan)
        {
            try
            {
                var jenisKegiatans = await _jenisKegiatanService.GetBySatuanAsync(satuan);
                var response = JenisKegiatanResponseDto.FromEntities(jenisKegiatans);
                return Success(response, $"Berhasil mengambil data jenis kegiatan dengan satuan '{satuan}'.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<JenisKegiatanResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateJenisKegiatanDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var jenisKegiatan = new Models.JenisKegiatan
                {
                    NamaKegiatan = dto.NamaKegiatan,
                    Deskripsi = dto.Deskripsi,
                    //Satuan = dto.Satuan,
                    //BiayaDefault = dto.BiayaDefault
                };

                var result = await _jenisKegiatanService.CreateAsync(jenisKegiatan);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = JenisKegiatanResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJenisKegiatanDto dto)
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

                var jenisKegiatan = new Models.JenisKegiatan
                {
                    Id = dto.Id,
                    NamaKegiatan = dto.NamaKegiatan,
                    Deskripsi = dto.Deskripsi,
                    //Satuan = dto.Satuan,
                    //BiayaDefault = dto.BiayaDefault
                };

                var result = await _jenisKegiatanService.UpdateAsync(jenisKegiatan);

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
                var result = await _jenisKegiatanService.DeleteAsync(id);

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