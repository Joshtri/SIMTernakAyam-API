using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.Common;
using SIMTernakAyam.DTOs.KandangAsisten;
using SIMTernakAyam.Models;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/kandang-asistens")]
    public class KandangAsistenController : BaseController
    {
        private readonly IKandangAsistenService _kandangAsistenService;

        public KandangAsistenController(IKandangAsistenService kandangAsistenService)
        {
            _kandangAsistenService = kandangAsistenService;
        }

        /// <summary>
        /// Get all kandang asistens
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var kandangAsistens = await _kandangAsistenService.GetAllWithDetailsAsync();
                var response = KandangAsistenResponseDto.FromEntities(kandangAsistens);
                return Success(response, "Data asisten kandang berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get kandang asisten by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var kandangAsisten = await _kandangAsistenService.GetWithDetailsAsync(id);
                if (kandangAsisten == null)
                {
                    return NotFound("Data asisten kandang tidak ditemukan");
                }

                var response = KandangAsistenResponseDto.FromEntity(kandangAsisten);
                return Success(response, "Data asisten kandang berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get all asistens for a specific kandang
        /// </summary>
        [HttpGet("by-kandang/{kandangId}")]
        public async Task<IActionResult> GetByKandang(Guid kandangId)
        {
            try
            {
                var kandangAsistens = await _kandangAsistenService.GetAsistensByKandangAsync(kandangId);
                var response = KandangAsistenResponseDto.FromEntities(kandangAsistens);
                return Success(response, "Data asisten kandang berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get active asistens for a specific kandang
        /// </summary>
        [HttpGet("by-kandang/{kandangId}/active")]
        public async Task<IActionResult> GetActiveByKandang(Guid kandangId)
        {
            try
            {
                var kandangAsistens = await _kandangAsistenService.GetActiveAsistensByKandangAsync(kandangId);
                var response = KandangAsistenResponseDto.FromEntities(kandangAsistens);
                return Success(response, "Data asisten aktif kandang berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get all kandangs assisted by a specific user
        /// </summary>
        [HttpGet("by-asisten/{asistenId}")]
        public async Task<IActionResult> GetByAsisten(Guid asistenId)
        {
            try
            {
                var kandangAsistens = await _kandangAsistenService.GetKandangsByAsistenAsync(asistenId);
                var response = KandangAsistenResponseDto.FromEntities(kandangAsistens);
                return Success(response, "Data kandang yang diasisteni berhasil diambil");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Create new kandang asisten
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKandangAsistenDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var kandangAsisten = new KandangAsisten
                {
                    KandangId = dto.KandangId,
                    AsistenId = dto.AsistenId,
                    Catatan = dto.Catatan,
                    IsAktif = dto.IsAktif
                };

                var (success, message, data) = await _kandangAsistenService.CreateAsync(kandangAsisten);

                if (!success)
                {
                    return Error(message);
                }

                var detailedData = await _kandangAsistenService.GetWithDetailsAsync(data!.Id);
                var response = KandangAsistenResponseDto.FromEntity(detailedData!);
                return Created(response, message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Update kandang asisten
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateKandangAsistenDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var existing = await _kandangAsistenService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound("Data asisten kandang tidak ditemukan");
                }

                existing.Catatan = dto.Catatan;
                existing.IsAktif = dto.IsAktif;

                var (success, message) = await _kandangAsistenService.UpdateAsync(existing);

                if (!success)
                {
                    return Error(message);
                }

                var detailedData = await _kandangAsistenService.GetWithDetailsAsync(id);
                var response = KandangAsistenResponseDto.FromEntity(detailedData!);
                return Success(response, message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Delete kandang asisten
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var (success, message) = await _kandangAsistenService.DeleteAsync(id);

                if (!success)
                {
                    return Error(message);
                }

                return Success<object>(null, message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
