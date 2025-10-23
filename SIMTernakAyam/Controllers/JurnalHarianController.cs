using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.JurnalHarian;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/jurnal-harian")]
    [Authorize]
    public class JurnalHarianController : BaseController
    {
        private readonly IJurnalHarianService _jurnalService;

        public JurnalHarianController(IJurnalHarianService jurnalService)
        {
            _jurnalService = jurnalService;
        }

        #region Helper Methods

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
        }

        private bool IsOperatorOrPemilik()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim == null || !Enum.TryParse<RoleEnum>(roleClaim.Value, out var role))
            {
                return false;
            }

            return role == RoleEnum.Operator || role == RoleEnum.Pemilik;
        }

        #endregion

        /// <summary>
        /// Membuat jurnal harian baru (dengan foto) - menggunakan form-data
        /// </summary>
        [HttpPost("with-photo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Common.ApiResponse<JurnalHarianResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateWithPhoto([FromForm] CreateJurnalHarianDto dto)
        {
            try
            {
                var petugasId = GetUserId();
                var result = await _jurnalService.CreateAsync(dto, petugasId, dto.FotoKegiatan);
                return Created(result, "Jurnal harian berhasil dibuat dengan foto.");
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Membuat jurnal harian baru - menggunakan JSON (dengan/tanpa foto base64)
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Common.ApiResponse<JurnalHarianResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> Create([FromBody] CreateJurnalHarianJsonDto dto)
        {
            try
            {
                var petugasId = GetUserId();

                // Convert base64 to IFormFile if exists
                IFormFile? fotoFile = null;
                if (!string.IsNullOrEmpty(dto.FotoKegiatanBase64))
                {
                    fotoFile = ConvertBase64ToFormFile(dto.FotoKegiatanBase64, dto.FotoKegiatanFileName);
                }

                // Map to original DTO
                var createDto = new CreateJurnalHarianDto
                {
                    Tanggal = dto.Tanggal,
                    JudulKegiatan = dto.JudulKegiatan,
                    DeskripsiKegiatan = dto.DeskripsiKegiatan,
                    WaktuMulai = dto.WaktuMulai,
                    WaktuSelesai = dto.WaktuSelesai,
                    KandangId = dto.KandangId,
                    Catatan = dto.Catatan,
                    FotoKegiatan = fotoFile
                };

                var result = await _jurnalService.CreateAsync(createDto, petugasId, fotoFile);
                return Created(result, "Jurnal harian berhasil dibuat.");
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Helper method untuk convert base64 string ke IFormFile
        /// </summary>
        private IFormFile? ConvertBase64ToFormFile(string base64String, string? fileName = null)
        {
            try
            {
                // Remove data:image/jpeg;base64, prefix if exists
                var base64Data = base64String;
                if (base64String.Contains(","))
                {
                    base64Data = base64String.Split(',')[1];
                }

                var bytes = Convert.FromBase64String(base64Data);
                var stream = new MemoryStream(bytes);

                // Determine file name and content type
                var extension = "jpg";
                var contentType = "image/jpeg";

                if (!string.IsNullOrEmpty(fileName))
                {
                    extension = Path.GetExtension(fileName).TrimStart('.');
                }
                else
                {
                    // Try to detect from base64 prefix
                    if (base64String.StartsWith("data:image/png"))
                    {
                        extension = "png";
                        contentType = "image/png";
                    }
                }

                fileName = fileName ?? $"jurnal_{Guid.NewGuid()}.{extension}";

                return new FormFile(stream, 0, bytes.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Mendapatkan list jurnal harian
        /// Petugas hanya bisa melihat jurnalnya sendiri, Operator/Pemilik bisa melihat semua
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<JurnalHarianResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();
                var isAdminRole = IsOperatorOrPemilik();

                Guid? filterPetugasId = isAdminRole ? null : userId;

                var result = await _jurnalService.GetAllAsync(filterPetugasId, page, pageSize);
                var totalCount = await _jurnalService.GetTotalCountAsync(filterPetugasId);

                return SuccessWithPagination(result, totalCount, page, pageSize, "Berhasil mengambil data jurnal harian.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan detail jurnal harian berdasarkan ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<JurnalHarianResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _jurnalService.GetByIdAsync(id);
                if (result == null)
                {
                    return NotFound("Jurnal harian tidak ditemukan.");
                }

                // Check authorization
                var userId = GetUserId();
                var isAdminRole = IsOperatorOrPemilik();

                if (!isAdminRole && result.PetugasId != userId)
                {
                    return Forbidden("Anda tidak memiliki akses untuk melihat jurnal ini.");
                }

                return Success(result, "Berhasil mengambil detail jurnal harian.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mengupdate jurnal harian (dengan foto) - menggunakan form-data
        /// </summary>
        [HttpPut("{id}/with-photo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Common.ApiResponse<JurnalHarianResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> UpdateWithPhoto(Guid id, [FromForm] UpdateJurnalHarianDto dto)
        {
            try
            {
                var petugasId = GetUserId();
                var result = await _jurnalService.UpdateAsync(id, dto, petugasId, dto.FotoKegiatan);

                if (result == null)
                {
                    return NotFound("Jurnal harian tidak ditemukan.");
                }

                return Success(result, "Jurnal harian berhasil diupdate dengan foto.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mengupdate jurnal harian - menggunakan JSON (dengan/tanpa foto base64)
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Common.ApiResponse<JurnalHarianResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJurnalHarianJsonDto dto)
        {
            try
            {
                var petugasId = GetUserId();

                // Convert base64 to IFormFile if exists
                IFormFile? fotoFile = null;
                if (!string.IsNullOrEmpty(dto.FotoKegiatanBase64))
                {
                    fotoFile = ConvertBase64ToFormFile(dto.FotoKegiatanBase64, dto.FotoKegiatanFileName);
                }

                // Map to original DTO
                var updateDto = new UpdateJurnalHarianDto
                {
                    Tanggal = dto.Tanggal,
                    JudulKegiatan = dto.JudulKegiatan,
                    DeskripsiKegiatan = dto.DeskripsiKegiatan,
                    WaktuMulai = dto.WaktuMulai,
                    WaktuSelesai = dto.WaktuSelesai,
                    KandangId = dto.KandangId,
                    Catatan = dto.Catatan,
                    FotoKegiatan = fotoFile
                };

                var result = await _jurnalService.UpdateAsync(id, updateDto, petugasId, fotoFile);

                if (result == null)
                {
                    return NotFound("Jurnal harian tidak ditemukan.");
                }

                return Success(result, "Jurnal harian berhasil diupdate.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Menghapus jurnal harian
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 403)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var petugasId = GetUserId();
                var result = await _jurnalService.DeleteAsync(id, petugasId);

                if (!result)
                {
                    return NotFound("Jurnal harian tidak ditemukan.");
                }

                return Success<object>(null!, "Jurnal harian berhasil dihapus.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan laporan jurnal harian
        /// </summary>
        /// <param name="startDate">Tanggal mulai</param>
        /// <param name="endDate">Tanggal selesai</param>
        /// <param name="petugasId">ID Petugas (opsional, untuk filter spesifik petugas)</param>
        [HttpGet("laporan")]
        [ProducesResponseType(typeof(Common.ApiResponse<LaporanJurnalDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> GetLaporan(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? petugasId = null)
        {
            try
            {
                var userId = GetUserId();
                var isAdminRole = IsOperatorOrPemilik();

                // Default ke bulan ini
                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? DateTime.Now;

                if (start > end)
                {
                    return Error("Tanggal mulai tidak boleh lebih besar dari tanggal selesai");
                }

                // Petugas hanya bisa melihat laporannya sendiri
                Guid? filterPetugasId = petugasId;
                if (!isAdminRole)
                {
                    filterPetugasId = userId;
                }

                var result = await _jurnalService.GetLaporanAsync(start, end, filterPetugasId);
                return Success(result, "Berhasil mengambil laporan jurnal harian.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
