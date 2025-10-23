using SIMTernakAyam.DTOs.JurnalHarian;
using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    public class JurnalHarianService : IJurnalHarianService
    {
        private readonly IJurnalHarianRepository _jurnalRepository;
        private readonly IUserRepository _userRepository;
        private readonly IKandangRepository _kandangRepository;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<JurnalHarianService> _logger;

        public JurnalHarianService(
            IJurnalHarianRepository jurnalRepository,
            IUserRepository userRepository,
            IKandangRepository kandangRepository,
            INotificationService notificationService,
            IWebHostEnvironment environment,
            ILogger<JurnalHarianService> logger)
        {
            _jurnalRepository = jurnalRepository;
            _userRepository = userRepository;
            _kandangRepository = kandangRepository;
            _notificationService = notificationService;
            _environment = environment;
            _logger = logger;
        }

        public async Task<JurnalHarianResponseDto> CreateAsync(CreateJurnalHarianDto dto, Guid petugasId, IFormFile? fotoKegiatan = null)
        {
            // Validasi waktu
            if (dto.WaktuSelesaiTimeSpan <= dto.WaktuMulaiTimeSpan)
            {
                throw new ArgumentException("Waktu selesai harus lebih besar dari waktu mulai");
            }

            var jurnal = new JurnalHarian
            {
                PetugasId = petugasId,
                Tanggal = dto.Tanggal,
                JudulKegiatan = dto.JudulKegiatan,
                DeskripsiKegiatan = dto.DeskripsiKegiatan,
                WaktuMulai = dto.WaktuMulaiTimeSpan,
                WaktuSelesai = dto.WaktuSelesaiTimeSpan,
                KandangId = dto.KandangId,
                Catatan = dto.Catatan
            };

            // Handle file upload
            if (fotoKegiatan != null && fotoKegiatan.Length > 0)
            {
                jurnal.FotoKegiatan = await SaveFileAsync(fotoKegiatan);
            }

            await _jurnalRepository.AddAsync(jurnal);
            await _jurnalRepository.SaveChangesAsync();

            // Send notification to supervisors (Pemilik & Operator)
            try
            {
                _logger.LogInformation("Mencoba membuat notifikasi untuk jurnal harian ID: {JurnalId}", jurnal.Id);

                var petugas = await _userRepository.GetByIdAsync(petugasId);
                _logger.LogInformation("Data petugas ditemukan: {PetugasName}", petugas?.FullName ?? "Tidak ditemukan");

                var petugasName = petugas?.FullName ?? "Petugas";

                await _notificationService.NotifyJurnalHarianAsync(
                    petugasId,
                    petugasName,
                    dto.JudulKegiatan,
                    dto.KandangId,
                    jurnal.Id
                );

                _logger.LogInformation("✅ Notifikasi berhasil dikirim untuk jurnal harian ID: {JurnalId}", jurnal.Id);
            }
            catch (Exception ex)
            {
                // Jika notifikasi gagal, tidak perlu throw error karena jurnal sudah tersimpan
                _logger.LogError(ex, "❌ Error saat membuat notifikasi untuk jurnal harian ID: {JurnalId}", jurnal.Id);
            }

            return await MapToResponseDto(jurnal);
        }

        public async Task<JurnalHarianResponseDto?> GetByIdAsync(Guid id)
        {
            var jurnal = await _jurnalRepository.GetByIdAsync(id);
            if (jurnal == null) return null;

            return await MapToResponseDto(jurnal);
        }

        public async Task<List<JurnalHarianResponseDto>> GetAllAsync(Guid? petugasId = null, int page = 1, int pageSize = 10)
        {
            List<JurnalHarian> jurnalList;

            if (petugasId.HasValue)
            {
                jurnalList = await _jurnalRepository.GetByPetugasIdAsync(petugasId.Value, page, pageSize);
            }
            else
            {
                jurnalList = (await _jurnalRepository.GetAllAsync())
                    .OrderByDescending(j => j.Tanggal)
                    .ThenByDescending(j => j.WaktuMulai)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            var result = new List<JurnalHarianResponseDto>();
            foreach (var jurnal in jurnalList)
            {
                result.Add(await MapToResponseDto(jurnal));
            }

            return result;
        }

        public async Task<JurnalHarianResponseDto?> UpdateAsync(Guid id, UpdateJurnalHarianDto dto, Guid petugasId, IFormFile? fotoKegiatan = null)
        {
            var jurnal = await _jurnalRepository.GetByIdAsync(id);
            if (jurnal == null) return null;

            // Verifikasi ownership
            if (jurnal.PetugasId != petugasId)
            {
                throw new UnauthorizedAccessException("Anda tidak memiliki akses untuk mengubah jurnal ini");
            }

            // Validasi waktu
            if (dto.WaktuSelesaiTimeSpan <= dto.WaktuMulaiTimeSpan)
            {
                throw new ArgumentException("Waktu selesai harus lebih besar dari waktu mulai");
            }

            jurnal.Tanggal = dto.Tanggal;
            jurnal.JudulKegiatan = dto.JudulKegiatan;
            jurnal.DeskripsiKegiatan = dto.DeskripsiKegiatan;
            jurnal.WaktuMulai = dto.WaktuMulaiTimeSpan;
            jurnal.WaktuSelesai = dto.WaktuSelesaiTimeSpan;
            jurnal.KandangId = dto.KandangId;
            jurnal.Catatan = dto.Catatan;

            // Handle file upload
            if (fotoKegiatan != null && fotoKegiatan.Length > 0)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(jurnal.FotoKegiatan))
                {
                    DeleteFile(jurnal.FotoKegiatan);
                }
                jurnal.FotoKegiatan = await SaveFileAsync(fotoKegiatan);
            }

             _jurnalRepository.UpdateAsync(jurnal);
             await _jurnalRepository.SaveChangesAsync();

            return await MapToResponseDto(jurnal);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid petugasId)
        {
            var jurnal = await _jurnalRepository.GetByIdAsync(id);
            if (jurnal == null) return false;

            // Verifikasi ownership
            if (jurnal.PetugasId != petugasId)
            {
                throw new UnauthorizedAccessException("Anda tidak memiliki akses untuk menghapus jurnal ini");
            }

            // Delete file if exists
            if (!string.IsNullOrEmpty(jurnal.FotoKegiatan))
            {
                DeleteFile(jurnal.FotoKegiatan);
            }

            await _jurnalRepository.Delete(id);
            return true;
        }

        public async Task<LaporanJurnalDto> GetLaporanAsync(DateTime startDate, DateTime endDate, Guid? petugasId = null)
        {
            var jurnalList = await _jurnalRepository.GetByDateRangeAsync(startDate, endDate, petugasId);

            var totalDurasi = TimeSpan.Zero;
            var jurnalPerKandang = new Dictionary<Guid, JurnalPerKandangDto>();

            foreach (var jurnal in jurnalList)
            {
                var durasi = jurnal.WaktuSelesai - jurnal.WaktuMulai;
                totalDurasi += durasi;

                if (jurnal.KandangId.HasValue)
                {
                    if (!jurnalPerKandang.ContainsKey(jurnal.KandangId.Value))
                    {
                        jurnalPerKandang[jurnal.KandangId.Value] = new JurnalPerKandangDto
                        {
                            KandangId = jurnal.KandangId.Value,
                            NamaKandang = jurnal.Kandang?.NamaKandang ?? "-",
                            JumlahKegiatan = 0,
                            TotalDurasi = TimeSpan.Zero
                        };
                    }

                    jurnalPerKandang[jurnal.KandangId.Value].JumlahKegiatan++;
                    jurnalPerKandang[jurnal.KandangId.Value].TotalDurasi += durasi;
                }
            }

            var detailJurnal = new List<JurnalHarianResponseDto>();
            foreach (var jurnal in jurnalList)
            {
                detailJurnal.Add(await MapToResponseDto(jurnal));
            }

            var totalDays = (endDate - startDate).Days + 1;
            var rataRataDurasi = totalDays > 0 ? (decimal)totalDurasi.TotalHours / totalDays : 0;

            return new LaporanJurnalDto
            {
                TanggalMulai = startDate,
                TanggalSelesai = endDate,
                PetugasId = petugasId,
                NamaPetugas = petugasId.HasValue ? jurnalList.FirstOrDefault()?.Petugas?.FullName : null,
                TotalJurnal = jurnalList.Count,
                TotalDurasiKerja = totalDurasi,
                RataRataDurasiPerHari = Math.Round(rataRataDurasi, 2),
                JurnalPerKandang = jurnalPerKandang.Values.ToList(),
                DetailJurnal = detailJurnal
            };
        }

        public async Task<int> GetTotalCountAsync(Guid? petugasId = null)
        {
            if (petugasId.HasValue)
            {
                return await _jurnalRepository.CountByPetugasIdAsync(petugasId.Value);
            }

            var allJurnals = await _jurnalRepository.GetAllAsync();
            return allJurnals.Count();
        }   

        #region Helper Methods

        private async Task<JurnalHarianResponseDto> MapToResponseDto(JurnalHarian jurnal)
        {
            // Load related entities if not loaded
            if (jurnal.Petugas == null || jurnal.Kandang == null)
            {
                jurnal = await _jurnalRepository.GetByIdAsync(jurnal.Id) ?? jurnal;
            }

            return new JurnalHarianResponseDto
            {
                Id = jurnal.Id,
                PetugasId = jurnal.PetugasId,
                NamaPetugas = jurnal.Petugas?.FullName ?? "-",
                UsernamePetugas = jurnal.Petugas?.Username ?? "-",
                Tanggal = jurnal.Tanggal,
                JudulKegiatan = jurnal.JudulKegiatan,
                DeskripsiKegiatan = jurnal.DeskripsiKegiatan,
                WaktuMulai = jurnal.WaktuMulai,
                WaktuSelesai = jurnal.WaktuSelesai,
                DurasiKegiatan = jurnal.WaktuSelesai - jurnal.WaktuMulai,
                KandangId = jurnal.KandangId,
                NamaKandang = jurnal.Kandang?.NamaKandang,
                LokasiKandang = jurnal.Kandang?.Lokasi,
                Catatan = jurnal.Catatan,
                FotoKegiatan = jurnal.FotoKegiatan,
                CreatedAt = jurnal.CreatedAt,
                UpdatedAt = jurnal.UpdateAt,
            };
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "jurnal");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/jurnal/{fileName}";
        }

        private void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        #endregion
    }
}
