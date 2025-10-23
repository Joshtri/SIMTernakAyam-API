using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Mortalitas;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/mortalitas")]
    public class MortalitasController : BaseController
    {
        private readonly IMortalitasService _mortalitasService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IAyamService _ayamService;

        public MortalitasController(
            IMortalitasService mortalitasService,
            INotificationService notificationService,
            IUserService userService,
            IAyamService ayamService)
        {
            _mortalitasService = mortalitasService;
            _notificationService = notificationService;
            _userService = userService;
            _ayamService = ayamService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User tidak terautentikasi");
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<MortalitasResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null, [FromQuery] Guid? kandangId = null)
        {
            try
            {
                var response = await _mortalitasService.GetEnhancedMortalitasAsync(search, kandangId);
                return Success(response, "Berhasil mengambil data mortalitas dengan detail lengkap.");
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
                var response = await _mortalitasService.GetMortalitasWithDetailsAsync(id);
                if (response == null)
                {
                    return NotFound("Data mortalitas tidak ditemukan.");
                }

                return Success(response, "Berhasil mengambil detail data mortalitas.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("kandang/{kandangId}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<MortalitasResponseDto>>), 200)]
        public async Task<IActionResult> GetByKandang(Guid kandangId)
        {
            try
            {
                var response = await _mortalitasService.GetMortalitasByKandangAsync(kandangId);
                return Success(response, $"Berhasil mengambil data mortalitas untuk kandang {kandangId}.");
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

                // Get enhanced response with calculations
                var enhancedResponse = await _mortalitasService.GetMortalitasWithDetailsAsync(result.Data!.Id);

                // 🔔 KIRIM NOTIFIKASI OTOMATIS (HIGH PRIORITY - Mortalitas!)
                try
                {
                    var userId = GetCurrentUserId();
                    var user = await _userService.GetByIdAsync(userId);
                    var ayam = await _ayamService.GetByIdAsync(dto.AyamId);

                    if (ayam?.Kandang != null)
                    {
                        await _notificationService.NotifyMortalitasAsync(
                            userId,
                            user?.FullName ?? user?.Username ?? "Petugas",
                            ayam.Kandang.NamaKandang,
                            dto.JumlahKematian,
                            dto.PenyebabKematian,
                            ayam.Kandang.Id
                        );
                    }
                }
                catch (Exception notifEx)
                {
                    // Log error tapi jangan fail request
                    Console.WriteLine($"Failed to send notification: {notifEx.Message}");
                }

                return Created(enhancedResponse, result.Message);
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
