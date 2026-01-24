using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Mortalitas;
using SIMTernakAyam.Models;
using SIMTernakAyam.Services.Interfaces;
using System.Globalization;
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

                fileName = fileName ?? $"mortalitas_{Guid.NewGuid()}.{extension}";

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

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<MortalitasResponseDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null, [FromQuery] Guid? kandangId = null, [FromQuery] String? period = null)
        {
            try
            {
                // Parse period parameter or default to current month
                bool applyPeriodFilter = true;
                DateTime filterDate = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(period))
                {
                    // Check if user wants to show all data
                    if (period.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        applyPeriodFilter = false;
                    }
                    else
                    {
                        // Try to parse the period (expected format: yyyy-MM)
                        if (DateTime.TryParseExact(period + "-01", "yyyy-MM-dd", null, DateTimeStyles.None, out var parsedDate))
                        {
                            filterDate = parsedDate;
                        }
                        else
                        {
                            return Error("Format period tidak valid. Gunakan format yyyy-MM (contoh: 2026-01) atau 'all' untuk semua data.", 400);
                        }
                    }
                }

                int filterYear = filterDate.Year;
                int filterMonth = filterDate.Month;

                var response = await _mortalitasService.GetEnhancedMortalitasAsync(search, kandangId);

                // Apply period filter (by month and year of TanggalKematian) if not "all"
                if (applyPeriodFilter)
                {
                    response = response.Where(m =>
                        m.TanggalKematian.Year == filterYear &&
                        m.TanggalKematian.Month == filterMonth
                    ).ToList();
                }

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
        [ProducesResponseType(typeof(Common.ApiResponse<List<MortalitasResponseDto>>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> Create([FromBody] CreateMortalitasDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Validate mode
                if (dto.Mode != "auto-fifo" && dto.Mode != "manual-split")
                {
                    return Error("Mode harus 'auto-fifo' atau 'manual-split'.", 400);
                }

                // Validate manual-split specific requirements
                if (dto.Mode == "manual-split")
                {
                    if (!dto.JumlahDariAyamLama.HasValue || !dto.JumlahDariAyamBaru.HasValue)
                    {
                        return Error("Mode 'manual-split' memerlukan JumlahDariAyamLama dan JumlahDariAyamBaru.", 400);
                    }

                    var totalManual = dto.JumlahDariAyamLama.Value + dto.JumlahDariAyamBaru.Value;
                    if (totalManual != dto.JumlahKematian)
                    {
                        return Error(
                            $"Total JumlahDariAyamLama ({dto.JumlahDariAyamLama.Value}) + " +
                            $"JumlahDariAyamBaru ({dto.JumlahDariAyamBaru.Value}) = {totalManual} " +
                            $"harus sama dengan JumlahKematian ({dto.JumlahKematian}).", 400);
                    }

                    if (dto.JumlahDariAyamLama.Value < 0 || dto.JumlahDariAyamBaru.Value < 0)
                    {
                        return Error("Jumlah kematian tidak boleh negatif.", 400);
                    }

                    if (totalManual <= 0)
                    {
                        return Error("Total jumlah kematian harus lebih dari 0.", 400);
                    }
                }

                // Handle foto upload jika ada base64
                IFormFile? fotoFile = null;
                if (!string.IsNullOrWhiteSpace(dto.FotoMortalitasBase64))
                {
                    fotoFile = ConvertBase64ToFormFile(dto.FotoMortalitasBase64, dto.FotoMortalitasFileName);
                }

                // Pilih method berdasarkan mode
                List<Mortalitas>? mortalitasList;
                string message;
                bool success;

                if (dto.Mode == "auto-fifo")
                {
                    // AUTO FIFO MODE
                    var result = await _mortalitasService.CreateMortalitasAutoFifoAsync(
                        dto.KandangId,
                        dto.TanggalKematian,
                        dto.WaktuKematian,
                        dto.JumlahKematian,
                        dto.PenyebabKematian,
                        fotoFile);

                    success = result.Success;
                    message = result.Message;
                    mortalitasList = result.Data;
                }
                else
                {
                    // MANUAL SPLIT MODE
                    var result = await _mortalitasService.CreateMortalitasManualSplitAsync(
                        dto.KandangId,
                        dto.TanggalKematian,
                        dto.WaktuKematian,
                        dto.JumlahDariAyamLama!.Value,
                        dto.JumlahDariAyamBaru!.Value,
                        dto.PenyebabKematian,
                        fotoFile);

                    success = result.Success;
                    message = result.Message;
                    mortalitasList = result.Data;
                }

                if (!success)
                {
                    return Error(message, 400);
                }

                // Convert to response DTOs
                var responseDtos = new List<MortalitasResponseDto>();
                foreach (var mortalitas in mortalitasList!)
                {
                    // Get enhanced details using service
                    var detailDto = await _mortalitasService.GetMortalitasWithDetailsAsync(mortalitas.Id);
                    if (detailDto != null)
                    {
                        responseDtos.Add(detailDto);
                    }
                }

                // 🔔 KIRIM NOTIFIKASI OTOMATIS (HIGH PRIORITY - Mortalitas!)
                try
                {
                    var userId = GetCurrentUserId();
                    var user = await _userService.GetByIdAsync(userId);

                    // Get kandang name from first mortalitas record
                    if (mortalitasList.Any())
                    {
                        var firstAyam = await _ayamService.GetByIdAsync(mortalitasList.First().AyamId);
                        if (firstAyam?.Kandang != null)
                        {
                            await _notificationService.NotifyMortalitasAsync(
                                userId,
                                user?.FullName ?? user?.Username ?? "Petugas",
                                firstAyam.Kandang.NamaKandang,
                                dto.JumlahKematian,
                                dto.PenyebabKematian,
                                firstAyam.Kandang.Id
                            );
                        }
                    }
                }
                catch (Exception notifEx)
                {
                    // Log error tapi jangan fail request
                    Console.WriteLine($"Failed to send notification: {notifEx.Message}");
                }

                return Created(responseDtos, message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Create mortalitas dengan Auto FIFO - sistem otomatis distribute ke ayam-ayam berdasarkan FIFO
        /// </summary>
        [HttpPost("auto-fifo")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<MortalitasResponseDto>>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> CreateAutoFifo([FromBody] CreateMortalitasAutoFifoDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Convert base64 to IFormFile if exists
                IFormFile? fotoFile = null;
                if (!string.IsNullOrEmpty(dto.FotoMortalitasBase64))
                {
                    fotoFile = ConvertBase64ToFormFile(dto.FotoMortalitasBase64, dto.FotoMortalitasFileName);
                }

                var result = await _mortalitasService.CreateMortalitasAutoFifoAsync(
                    dto.KandangId,
                    dto.TanggalKematian,
                    dto.WaktuKematian,
                    dto.JumlahKematian,
                    dto.PenyebabKematian,
                    fotoFile);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                // Get enhanced response list with calculations for all created mortalitas
                var enhancedResponseList = new List<MortalitasResponseDto>();
                foreach (var mortalitas in result.Data!)
                {
                    var enhancedDto = await _mortalitasService.GetMortalitasWithDetailsAsync(mortalitas.Id);
                    if (enhancedDto != null)
                    {
                        enhancedResponseList.Add(enhancedDto);
                    }
                }

                // 🔔 KIRIM NOTIFIKASI OTOMATIS (HIGH PRIORITY - Mortalitas!)
                try
                {
                    var userId = GetCurrentUserId();
                    var user = await _userService.GetByIdAsync(userId);

                    // Get kandang name from first mortalitas record
                    if (result.Data.Any())
                    {
                        var firstAyam = await _ayamService.GetByIdAsync(result.Data.First().AyamId);
                        if (firstAyam?.Kandang != null)
                        {
                            await _notificationService.NotifyMortalitasAsync(
                                userId,
                                user?.FullName ?? user?.Username ?? "Petugas",
                                firstAyam.Kandang.NamaKandang,
                                dto.JumlahKematian,
                                dto.PenyebabKematian,
                                firstAyam.Kandang.Id
                            );
                        }
                    }
                }
                catch (Exception notifEx)
                {
                    // Log error tapi jangan fail request
                    Console.WriteLine($"Failed to send notification: {notifEx.Message}");
                }

                return Created(enhancedResponseList, result.Message);
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

                // Convert base64 to IFormFile if exists
                IFormFile? fotoFile = null;
                if (!string.IsNullOrEmpty(dto.FotoMortalitasBase64))
                {
                    fotoFile = ConvertBase64ToFormFile(dto.FotoMortalitasBase64, dto.FotoMortalitasFileName);
                }

                var mortalitas = new Models.Mortalitas
                {
                    Id = dto.Id,
                    AyamId = dto.AyamId,
                    TanggalKematian = dto.TanggalKematian,
                    WaktuKematian = dto.WaktuKematian,
                    JumlahKematian = dto.JumlahKematian,
                    PenyebabKematian = dto.PenyebabKematian
                };

                var result = await _mortalitasService.UpdateAsync(mortalitas, fotoFile);

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
