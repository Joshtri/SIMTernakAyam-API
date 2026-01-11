using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Operasional;
using SIMTernakAyam.Services.Interfaces;
using System.Globalization;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/operasionals")]
    public class OperasionalController : BaseController
    {
        private readonly IOperasionalService _operasionalService;
        private readonly IVaksinService _vaksinService;
        private readonly IPakanService _pakanService;
        private readonly IJenisKegiatanService _jenisKegiatanService;

        public OperasionalController(
            IOperasionalService operasionalService,
            IVaksinService vaksinService,
            IPakanService pakanService,
            IJenisKegiatanService jenisKegiatanService)
        {
            _operasionalService = operasionalService;
            _vaksinService = vaksinService;
            _pakanService = pakanService;
            _jenisKegiatanService = jenisKegiatanService;
        }

        [HttpGet("form-data")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        public async Task<IActionResult> GetFormData()
        {
            try
            {
                var jenisKegiatans = await _jenisKegiatanService.GetAllAsync();
                var vaksins = await _vaksinService.GetAllVaksinWithUsageDetailAsync();
                var pakans = await _pakanService.GetAllPakanWithUsageDetailAsync();

                var formData = new
                {
                    JenisKegiatan = jenisKegiatans.Select(jk => new
                    {
                        Id = jk.Id,
                        Nama = jk.NamaKegiatan,
                        Deskripsi = jk.Deskripsi,
                        BiayaDefault = jk.BiayaDefault,
                        // Tambahan info untuk frontend
                        MembutuhkanVaksin = jk.NamaKegiatan?.Contains("Vaksin") ?? false,
                        MembutuhkanPakan = jk.NamaKegiatan?.Contains("Pakan") ?? false
                    }),
                    Vaksins = vaksins.Select(v => new
                    {
                        v.Id,
                        v.NamaVaksin,
                        v.StokTersisa,
                        v.StatusStok,
                        v.IsStokCukup,
                        v.Tipe,
                        Info = $"{v.NamaVaksin} - Sisa: {v.StokTersisa} dosis ({v.StatusStok})"
                    }),
                    Pakans = pakans.Select(p => new
                    {
                        p.Id,
                        p.NamaPakan,
                        p.StokTersisaKg,
                        p.StatusStok,
                        p.IsStokCukup,
                        Info = $"{p.NamaPakan} - Sisa: {p.StokTersisaKg:F1} kg ({p.StatusStok})"
                    })
                };

                return Success(formData, "Berhasil mengambil data untuk form operasional.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("create-with-validation")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateWithValidation([FromBody] CreateOperasionalWithValidationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Step 1: Validasi stok terlebih dahulu jika menggunakan pakan/vaksin
                var validationResults = new List<object>();
                var errors = new List<string>();

                if (dto.VaksinId.HasValue && dto.VaksinId != Guid.Empty)
                {
                    var vaksinAvailability = await _vaksinService.CheckStockAvailabilityAsync(dto.VaksinId.Value, dto.Jumlah);
                    if (vaksinAvailability != null)
                    {
                        var availability = vaksinAvailability as dynamic;
                        if (availability != null && !(bool)availability.IsAvailable)
                        {
                            errors.Add($"Stok vaksin tidak mencukupi. {availability.Rekomendasi}");
                        }
                        else
                        {
                            validationResults.Add(new { Type = "Vaksin", Data = vaksinAvailability });
                        }
                    }
                }

                if (dto.PakanId.HasValue && dto.PakanId != Guid.Empty)
                {
                    var pakanAvailability = await _pakanService.CheckStockAvailabilityAsync(dto.PakanId.Value, dto.Jumlah);
                    if (pakanAvailability != null)
                    {
                        var availability = pakanAvailability as dynamic;
                        if (availability != null && !(bool)availability.IsAvailable)
                        {
                            errors.Add($"Stok pakan tidak mencukupi. {availability.Rekomendasi}");
                        }
                        else
                        {
                            validationResults.Add(new { Type = "Pakan", Data = pakanAvailability });
                        }
                    }
                }

                // Jika ada error validasi stok, return error
                if (errors.Any())
                {
                    return Error(string.Join(" ", errors), 400);
                }

                // Step 2: Create operasional entity
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

                // Step 3: Create operasional
                var result = await _operasionalService.CreateAsync(operasional);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                // Step 4: Return success with details
                var response = new
                {
                    Operasional = OperasionalResponseDto.FromEntity(result.Data!),
                    StockValidation = validationResults,
                    Message = result.Message
                };

                return Created(response, "Operasional berhasil dibuat dengan validasi stok.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}/update-with-validation")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateWithValidation(Guid id, [FromBody] UpdateOperasionalWithValidationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Step 1: Cek apakah operasional exists
                var existingOperasional = await _operasionalService.GetByIdAsync(id);
                if (existingOperasional == null)
                {
                    return NotFound("Data operasional tidak ditemukan.");
                }

                // Step 2: Validasi stok jika ada perubahan pakan/vaksin
                var validationResults = new List<object>();
                var errors = new List<string>();

                if (dto.VaksinId.HasValue && dto.VaksinId != Guid.Empty)
                {
                    var vaksinAvailability = await _vaksinService.CheckStockAvailabilityAsync(dto.VaksinId.Value, dto.Jumlah);
                    if (vaksinAvailability != null)
                    {
                        var availability = vaksinAvailability as dynamic;
                        if (availability != null && !(bool)availability.IsAvailable)
                        {
                            errors.Add($"Stok vaksin tidak mencukupi. {availability.Rekomendasi}");
                        }
                        else
                        {
                            validationResults.Add(new { Type = "Vaksin", Data = vaksinAvailability });
                        }
                    }
                }

                if (dto.PakanId.HasValue && dto.PakanId != Guid.Empty)
                {
                    var pakanAvailability = await _pakanService.CheckStockAvailabilityAsync(dto.PakanId.Value, dto.Jumlah);
                    if (pakanAvailability != null)
                    {
                        var availability = pakanAvailability as dynamic;
                        if (availability != null && !(bool)availability.IsAvailable)
                        {
                            errors.Add($"Stok pakan tidak mencukupi. {availability.Rekomendasi}");
                        }
                        else
                        {
                            validationResults.Add(new { Type = "Pakan", Data = pakanAvailability });
                        }
                    }
                }

                // Jika ada error validasi stok, return error
                if (errors.Any())
                {
                    return Error(string.Join(" ", errors), 400);
                }

                // Step 3: Update operasional
                var operasional = new Models.Operasional
                {
                    Id = id, // Gunakan ID dari URL
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
                    return Error(result.Message, 400);
                }

                // Step 4: Return success with details
                var response = new
                {
                    Message = result.Message,
                    StockValidation = validationResults,
                    UpdatedId = id
                };

                return Success(response, "Operasional berhasil diupdate dengan validasi stok.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("validate-stock")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        public async Task<IActionResult> ValidateStock([FromBody] ValidateOperasionalStockDto dto)
        {
            try
            {
                var warnings = new List<object>();
                var errors = new List<string>();

                // Validasi stok vaksin jika ada
                if (dto.VaksinId.HasValue && dto.VaksinId != Guid.Empty && dto.Jumlah > 0)
                {
                    var vaksinAvailability = await _vaksinService.CheckStockAvailabilityAsync(dto.VaksinId.Value, dto.Jumlah);
                    if (vaksinAvailability != null)
                    {
                        var availability = vaksinAvailability as dynamic;
                        if (availability != null && !(bool)availability.IsAvailable)
                        {
                            errors.Add($"Stok vaksin tidak mencukupi. {availability.Rekomendasi}");
                        }
                        else
                        {
                            warnings.Add(new
                            {
                                Type = "Vaksin",
                                Data = vaksinAvailability
                            });
                        }
                    }
                }

                // Validasi stok pakan jika ada
                if (dto.PakanId.HasValue && dto.PakanId != Guid.Empty && dto.Jumlah > 0)
                {
                    var pakanAvailability = await _pakanService.CheckStockAvailabilityAsync(dto.PakanId.Value, dto.Jumlah);
                    if (pakanAvailability != null)
                    {
                        var availability = pakanAvailability as dynamic;
                        if (availability != null && !(bool)availability.IsAvailable)
                        {
                            errors.Add($"Stok pakan tidak mencukupi. {availability.Rekomendasi}");
                        }
                        else
                        {
                            warnings.Add(new
                            {
                                Type = "Pakan",
                                Data = pakanAvailability
                            });
                        }
                    }
                }

                if (errors.Any())
                {
                    return Error(string.Join(" ", errors), 400);
                }

                return Success(new
                {
                    CanProceed = true,
                    Warnings = warnings,
                    Message = warnings.Any() ? "Validasi berhasil dengan peringatan." : "Stok mencukupi untuk operasional ini."
                }, "Validasi stok berhasil.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<OperasionalResponseDto>>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? kandangId = null,
            [FromQuery] Guid? petugasId = null,
            [FromQuery] Guid? pakanId = null,
            [FromQuery] Guid? vaksinId = null,
            [FromQuery] string? period = null)
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

                var operasionals = await _operasionalService.GetAllOperasionalWithDetailsAsync();
                var operasionalList = operasionals.ToList();

                // Apply kandangId filter if provided
                if (kandangId.HasValue && kandangId.Value != Guid.Empty)
                {
                    operasionalList = operasionalList.Where(o => o.KandangId == kandangId.Value).ToList();
                }

                // Apply petugasId filter if provided
                if (petugasId.HasValue && petugasId.Value != Guid.Empty)
                {
                    operasionalList = operasionalList.Where(o => o.PetugasId == petugasId.Value).ToList();
                }

                // Apply pakanId filter if provided
                if (pakanId.HasValue && pakanId.Value != Guid.Empty)
                {
                    operasionalList = operasionalList.Where(o => o.PakanId.HasValue && o.PakanId.Value == pakanId.Value).ToList();
                }

                // Apply vaksinId filter if provided
                if (vaksinId.HasValue && vaksinId.Value != Guid.Empty)
                {
                    operasionalList = operasionalList.Where(o => o.VaksinId.HasValue && o.VaksinId.Value == vaksinId.Value).ToList();
                }

                // Apply period filter (by month and year of Tanggal) if not "all"
                if (applyPeriodFilter)
                {
                    operasionalList = operasionalList.Where(o =>
                        o.Tanggal.Year == filterYear &&
                        o.Tanggal.Month == filterMonth
                    ).ToList();
                }

                var response = OperasionalResponseDto.FromEntities(operasionalList);
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
                var operasional = await _operasionalService.GetOperasionalWithDetailsAsync(id);
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

                // ? ID validation - consistent with other controllers
                if (id != dto.Id)
                {
                    return Error("ID di URL tidak sesuai dengan ID di body.", 400);
                }

                // Cek apakah operasional ada
                var existingOperasional = await _operasionalService.GetByIdAsync(id);
                if (existingOperasional == null)
                {
                    return NotFound("Data operasional tidak ditemukan.");
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

    // DTO untuk validasi stok operasional
    public class ValidateOperasionalStockDto
    {
        public Guid? VaksinId { get; set; }
        public Guid? PakanId { get; set; }
        public int Jumlah { get; set; }
    }

    // DTO untuk create operasional dengan validasi
    public class CreateOperasionalWithValidationDto
    {
        public Guid JenisKegiatanId { get; set; }
        public DateTime Tanggal { get; set; }
        public int Jumlah { get; set; }
        public Guid PetugasId { get; set; }
        public Guid KandangId { get; set; }
        public Guid? PakanId { get; set; }
        public Guid? VaksinId { get; set; }
    }

    // DTO untuk update operasional dengan validasi (tidak perlu ID di body)
    public class UpdateOperasionalWithValidationDto
    {
        public Guid JenisKegiatanId { get; set; }
        public DateTime Tanggal { get; set; }
        public int Jumlah { get; set; }
        public Guid PetugasId { get; set; }
        public Guid KandangId { get; set; }
        public Guid? PakanId { get; set; }
        public Guid? VaksinId { get; set; }
    }
}