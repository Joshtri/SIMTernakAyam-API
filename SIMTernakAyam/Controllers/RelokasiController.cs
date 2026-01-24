using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Relokasi;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    /// <summary>
    /// Controller untuk manajemen relokasi ayam antar kandang
    /// Digunakan untuk memindahkan ayam sakit ke kandang isolasi atau sebaliknya
    /// </summary>
    [ApiController]
    [Route("api/relokasi")]
    public class RelokasiController : BaseController
    {
        private readonly IRelokasiService _relokasiService;

        public RelokasiController(IRelokasiService relokasiService)
        {
            _relokasiService = relokasiService;
        }

        /// <summary>
        /// Get all relokasi dengan filter optional
        /// </summary>
        /// <param name="search">Search by kandang name, petugas name, catatan</param>
        /// <param name="kandangId">Filter by kandang (as source or destination)</param>
        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<RelokasiResponseDto>>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] Guid? kandangId = null)
        {
            try
            {
                var relokasis = await _relokasiService.SearchRelokasiAsync(search, kandangId);
                var response = RelokasiResponseDto.FromEntities(relokasis);
                return Success(response, $"Berhasil mengambil {response.Count} data relokasi.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get relokasi by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<RelokasiResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var relokasi = await _relokasiService.GetRelokasiWithDetailsAsync(id);
                if (relokasi == null)
                {
                    return NotFound("Data relokasi tidak ditemukan.");
                }

                var response = RelokasiResponseDto.FromEntity(relokasi);
                return Success(response, "Berhasil mengambil data relokasi.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get all relokasi for a specific kandang
        /// </summary>
        [HttpGet("kandang/{kandangId}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<RelokasiResponseDto>>), 200)]
        public async Task<IActionResult> GetByKandang(Guid kandangId)
        {
            try
            {
                var relokasis = await _relokasiService.GetRelokasiByKandangAsync(kandangId);
                var response = RelokasiResponseDto.FromEntities(relokasis);
                return Success(response, $"Berhasil mengambil {response.Count} data relokasi untuk kandang.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Create new relokasi
        /// This will:
        /// 1. Validate stock at source batch
        /// 2. Validate capacity at destination kandang
        /// 3. Create new batch at destination
        /// 4. Record relokasi
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<RelokasiResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateRelokasiDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Error("User ID tidak valid.", 401);
                }

                var result = await _relokasiService.CreateRelokasiAsync(dto, userId);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = RelokasiResponseDto.FromEntity(result.Data!);
                return Created(response, result.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Update relokasi (only catatan and status can be updated)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<RelokasiResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRelokasiDto dto)
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

                var existing = await _relokasiService.GetRelokasiWithDetailsAsync(id);
                if (existing == null)
                {
                    return NotFound("Data relokasi tidak ditemukan.");
                }

                // Only update allowed fields
                if (dto.StatusRelokasi.HasValue)
                {
                    existing.StatusRelokasi = dto.StatusRelokasi.Value;
                }

                if (dto.Catatan != null)
                {
                    existing.Catatan = dto.Catatan;
                }

                var result = await _relokasiService.UpdateAsync(existing);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                // Fetch updated data
                var updated = await _relokasiService.GetRelokasiWithDetailsAsync(id);
                var response = RelokasiResponseDto.FromEntity(updated!);
                return Success(response, "Data relokasi berhasil diupdate.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Cancel relokasi (change status to Dibatalkan)
        /// Note: This does NOT automatically reverse the stock changes
        /// </summary>
        [HttpPut("{id}/batal")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> Batalkan(Guid id)
        {
            try
            {
                var result = await _relokasiService.BatalkanRelokasiAsync(id);

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

        /// <summary>
        /// Delete relokasi (hard delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _relokasiService.DeleteAsync(id);

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
