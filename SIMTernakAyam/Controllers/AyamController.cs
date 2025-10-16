using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Ayam;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/ayams")]
    public class AyamController : BaseController
    {
        private readonly IAyamService _ayamService;

        public AyamController(IAyamService ayamService)
        {
            _ayamService = ayamService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<AyamResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? kandangId = null, [FromQuery] string? search = null)
        {
            try
            {
                IEnumerable<Models.Ayam> ayams;

                if (kandangId.HasValue && kandangId.Value != Guid.Empty)
                {
                    ayams = await _ayamService.GetAyamByKandangAsync(kandangId.Value);
                }
                else
                {
                    ayams = await _ayamService.GetAllAyamWithDetailsAsync();
                }

                // Apply free-text search (optional)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    ayams = ayams.Where(a =>
                        (a.Kandang != null && !string.IsNullOrEmpty(a.Kandang.NamaKandang) && a.Kandang.NamaKandang.Contains(s, StringComparison.OrdinalIgnoreCase))
                        || a.JumlahMasuk.ToString().Contains(s)
                        || a.TanggalMasuk.ToString("yyyy-MM-dd").Contains(s)
                    );
                }

                var response = AyamResponseDto.FromEntities(ayams);
                return Success(response, "Berhasil mengambil data ayam.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<AyamResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var ayam = await _ayamService.GetAyamWithDetailsAsync(id);
                if (ayam == null)
                {
                    return NotFound("Data ayam tidak ditemukan.");
                }

                var response = AyamResponseDto.FromEntity(ayam);
                return Success(response, "Berhasil mengambil data ayam.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<AyamResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateAyamDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var ayam = new Models.Ayam
                {
                    KandangId = dto.KandangId,
                    TanggalMasuk = dto.TanggalMasuk,
                    JumlahMasuk = dto.JumlahMasuk
                };

                var result = await _ayamService.CreateAsync(ayam);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = AyamResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAyamDto dto)
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

                var ayam = new Models.Ayam
                {
                    Id = dto.Id,
                    KandangId = dto.KandangId,
                    TanggalMasuk = dto.TanggalMasuk,
                    JumlahMasuk = dto.JumlahMasuk
                };

                var result = await _ayamService.UpdateAsync(ayam);

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
                var result = await _ayamService.DeleteAsync(id);

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
