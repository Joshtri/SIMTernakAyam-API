using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.Biaya;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/biayas")]
    public class BiayaController : BaseController
    {
        private readonly IBiayaService _biayaService;

        public BiayaController(IBiayaService biayaService)
        {
            _biayaService = biayaService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<BiayaListResponseDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var biayas = await _biayaService.GetAllAsync();
                var response = BiayaListResponseDto.FromEntities(biayas);
                return Success(response, "Berhasil mengambil semua data biaya.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<BiayaResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var biaya = await _biayaService.GetByIdAsync(id);
                if (biaya == null)
                {
                    return NotFound("Data biaya tidak ditemukan.");
                }

                var response = BiayaResponseDto.FromEntity(biaya);
                return Success(response, "Berhasil mengambil data biaya.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<BiayaResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Create([FromBody] CreateBiayaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var biaya = new Models.Biaya
                {
                    JenisBiaya = dto.JenisBiaya,
                    KategoriBiaya = dto.KategoriBiaya,
                    Tanggal = dto.Tanggal,
                    Jumlah = dto.Jumlah,
                    PetugasId = dto.PetugasId,
                    OperasionalId = dto.OperasionalId,
                    KandangId = dto.KandangId,
                    BuktiBase64 = dto.BuktiBase64,
                    Catatan = dto.Catatan,
                    Bulan = dto.Bulan,
                    Tahun = dto.Tahun
                };

                var result = await _biayaService.CreateAsync(biaya);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = BiayaResponseDto.FromEntity(result.Data!);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBiayaDto dto)
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

                var biaya = new Models.Biaya
                {
                    Id = dto.Id,
                    JenisBiaya = dto.JenisBiaya,
                    KategoriBiaya = dto.KategoriBiaya,
                    Tanggal = dto.Tanggal,
                    Jumlah = dto.Jumlah,
                    PetugasId = dto.PetugasId,
                    OperasionalId = dto.OperasionalId,
                    KandangId = dto.KandangId,
                    BuktiBase64 = dto.BuktiBase64,
                    Catatan = dto.Catatan,
                    Bulan = dto.Bulan,
                    Tahun = dto.Tahun
                };

                var result = await _biayaService.UpdateAsync(biaya);

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
                var result = await _biayaService.DeleteAsync(id);

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

        [HttpGet("rekap-bulanan/{bulan}/{tahun}")]
        [ProducesResponseType(typeof(Common.ApiResponse<RekapBiayaBulananDto>), 200)]
        public async Task<IActionResult> GetRekapBulanan(int bulan, int tahun)
        {
            try
            {
                if (bulan < 1 || bulan > 12)
                {
                    return Error("Bulan harus antara 1-12.", 400);
                }

                var rekap = await _biayaService.GetRekapBiayaBulananAsync(bulan, tahun);
                return Success(rekap, $"Berhasil mengambil rekap biaya bulanan untuk {bulan}/{tahun}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("kategori/{kategori}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<BiayaListResponseDto>>), 200)]
        public async Task<IActionResult> GetByKategori(int kategori)
        {
            try
            {
                if (!Enum.IsDefined(typeof(KategoriBiayaEnum), kategori))
                {
                    return Error("Kategori biaya tidak valid.", 400);
                }

                var kategoriBiaya = (KategoriBiayaEnum)kategori;
                var biayas = await _biayaService.GetBiayaByKategoriAsync(kategoriBiaya);
                var response = BiayaListResponseDto.FromEntities(biayas);
                return Success(response, $"Berhasil mengambil biaya dengan kategori {kategoriBiaya}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("kategori/{kategori}/periode")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<BiayaListResponseDto>>), 200)]
        public async Task<IActionResult> GetByKategoriAndPeriode(int kategori, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (!Enum.IsDefined(typeof(KategoriBiayaEnum), kategori))
                {
                    return Error("Kategori biaya tidak valid.", 400);
                }

                if (startDate > endDate)
                {
                    return Error("Tanggal awal tidak boleh lebih besar dari tanggal akhir.", 400);
                }

                var kategoriBiaya = (KategoriBiayaEnum)kategori;
                var biayas = await _biayaService.GetBiayaByKategoriAndPeriodAsync(kategoriBiaya, startDate, endDate);
                var response = BiayaListResponseDto.FromEntities(biayas);
                return Success(response, $"Berhasil mengambil biaya kategori {kategoriBiaya} periode {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("kategori/{kategori}/total")]
        [ProducesResponseType(typeof(Common.ApiResponse<decimal>), 200)]
        public async Task<IActionResult> GetTotalByKategoriAndPeriode(int kategori, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (!Enum.IsDefined(typeof(KategoriBiayaEnum), kategori))
                {
                    return Error("Kategori biaya tidak valid.", 400);
                }

                if (startDate > endDate)
                {
                    return Error("Tanggal awal tidak boleh lebih besar dari tanggal akhir.", 400);
                }

                var kategoriBiaya = (KategoriBiayaEnum)kategori;
                var total = await _biayaService.GetTotalBiayaByKategoriAndPeriodAsync(kategoriBiaya, startDate, endDate);
                return Success(total, $"Berhasil mengambil total biaya kategori {kategoriBiaya} periode {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
