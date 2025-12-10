using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Ayam;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.Repository.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/ayams")]
    public class AyamController : BaseController
    {
        private readonly IAyamService _ayamService;
        private readonly IPanenRepository _panenRepository;
        private readonly IMortalitasRepository _mortalitasRepository;

        public AyamController(
            IAyamService ayamService,
            IPanenRepository panenRepository,
            IMortalitasRepository mortalitasRepository)
        {
            _ayamService = ayamService;
            _panenRepository = panenRepository;
            _mortalitasRepository = mortalitasRepository;
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
                    // ? Use method that loads with comprehensive stock info
                    ayams = await _ayamService.GetAllAyamWithStockInfoAsync();
                }

                var ayamList = ayams.ToList();

                // Apply free-text search (optional)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    ayamList = ayamList.Where(a =>
                        (a.Kandang != null && !string.IsNullOrEmpty(a.Kandang.NamaKandang) && a.Kandang.NamaKandang.Contains(s, StringComparison.OrdinalIgnoreCase))
                        || a.JumlahMasuk.ToString().Contains(s)
                        || a.TanggalMasuk.ToString("yyyy-MM-dd").Contains(s)
                    ).ToList();
                }

                // ? Get stock data for all ayams in bulk
                var ayamIds = ayamList.Select(a => a.Id).ToList();
                var panenData = ayamIds.Any() ? await _panenRepository.GetTotalEkorPanenByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();
                var mortalitasData = ayamIds.Any() ? await _mortalitasRepository.GetTotalMortalitasByAyamIdsAsync(ayamIds) : new Dictionary<Guid, int>();

                // ? Map to DTO with stock information
                var response = AyamResponseDto.FromEntitiesWithStockData(ayamList, panenData, mortalitasData);
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

                // ? Get stock data for single ayam
                var jumlahDipanen = await _panenRepository.GetTotalEkorPanenByAyamAsync(id);
                var jumlahMortalitas = await _mortalitasRepository.GetTotalMortalitasByAyamAsync(id);

                var response = AyamResponseDto.FromEntity(ayam, jumlahDipanen, jumlahMortalitas);
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

                // New ayam will have 0 harvest and 0 mortality
                var response = AyamResponseDto.FromEntity(result.Data!, 0, 0);
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
