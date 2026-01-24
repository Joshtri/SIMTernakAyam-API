using SIMTernakAyam.Models;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.DTOs.Mortalitas;

namespace SIMTernakAyam.Services
{
    public class MortalitasService : BaseService<Mortalitas>, IMortalitasService
    {
        private readonly IMortalitasRepository _mortalitasRepository;
        private readonly IAyamRepository _ayamRepository;
        private readonly IKandangRepository _kandangRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IFifoService _fifoService;

        public MortalitasService(
            IMortalitasRepository mortalitasRepository,
            IAyamRepository ayamRepository,
            IKandangRepository kandangRepository,
            IWebHostEnvironment environment,
            IFifoService fifoService) : base(mortalitasRepository)
        {
            _mortalitasRepository = mortalitasRepository;
            _ayamRepository = ayamRepository;
            _kandangRepository = kandangRepository;
            _environment = environment;
            _fifoService = fifoService ?? throw new ArgumentNullException(nameof(fifoService));
        }

        // Get all mortalitas with detailed calculations
        public async Task<IEnumerable<Mortalitas>> GetAllMortalitasWithDetailsAsync()
        {
            return await _mortalitasRepository.GetMortalitasWithCalculationsAsync();
        }

        // Get enhanced mortalitas response with calculations
        public async Task<List<MortalitasResponseDto>> GetEnhancedMortalitasAsync(string? search = null, Guid? kandangId = null)
        {
            var mortalitasList = await _mortalitasRepository.SearchMortalitasAsync(search, kandangId);
            var enhancedList = new List<MortalitasResponseDto>();

            foreach (var mortalitas in mortalitasList)
            {
                // Calculate total ayam before mortality (excluding this mortality)
                var totalSebelum = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                    mortalitas.AyamId, mortalitas.TanggalKematian);

                // Calculate total ayam AFTER this specific mortality (including same-date mortalities up to this one)
                var totalSesudah = await _mortalitasRepository.GetTotalAyamAfterMortalityAsync(
                    mortalitas.AyamId, mortalitas.Id, mortalitas.TanggalKematian);

                // Get kandang capacity
                var kapasitas = await _mortalitasRepository.GetKandangCapacityAsync(
                    mortalitas.Ayam?.KandangId ?? Guid.Empty);

                var enhancedDto = MortalitasResponseDto.FromEntity(mortalitas, totalSebelum, totalSesudah, kapasitas);

                // Convert image to base64 if exists (optional for list - might be slow)
                // Uncomment if you want base64 in list response too
                // if (!string.IsNullOrEmpty(mortalitas.FotoMortalitas))
                // {
                //     enhancedDto.FotoMortalitasBase64 = await ConvertImageToBase64(mortalitas.FotoMortalitas);
                // }

                enhancedList.Add(enhancedDto);
            }

            return enhancedList;
        }

        // Get mortalitas by kandang with calculations
        public async Task<List<MortalitasResponseDto>> GetMortalitasByKandangAsync(Guid kandangId)
        {
            var mortalitasList = await _mortalitasRepository.GetByKandangIdAsync(kandangId);
            var enhancedList = new List<MortalitasResponseDto>();

            foreach (var mortalitas in mortalitasList)
            {
                var totalSebelum = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                    mortalitas.AyamId, mortalitas.TanggalKematian);
                
                var totalSesudah = await _mortalitasRepository.GetTotalAyamAfterMortalityAsync(
                    mortalitas.AyamId, mortalitas.Id, mortalitas.TanggalKematian);
                
                var kapasitas = await _mortalitasRepository.GetKandangCapacityAsync(kandangId);
                
                var enhancedDto = MortalitasResponseDto.FromEntity(mortalitas, totalSebelum, totalSesudah, kapasitas);
                enhancedList.Add(enhancedDto);
            }

            return enhancedList;
        }

        // Get single mortalitas with details
        public async Task<MortalitasResponseDto?> GetMortalitasWithDetailsAsync(Guid id)
        {
            var mortalitas = await _mortalitasRepository.GetByIdAsync(id);
            if (mortalitas == null) return null;

            var totalSebelum = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                mortalitas.AyamId, mortalitas.TanggalKematian);

            var totalSesudah = await _mortalitasRepository.GetTotalAyamAfterMortalityAsync(
                mortalitas.AyamId, mortalitas.Id, mortalitas.TanggalKematian);

            var kapasitas = await _mortalitasRepository.GetKandangCapacityAsync(
                mortalitas.Ayam?.KandangId ?? Guid.Empty);

            var response = MortalitasResponseDto.FromEntity(mortalitas, totalSebelum, totalSesudah, kapasitas);

            // Convert image file to base64 if exists
            if (!string.IsNullOrEmpty(mortalitas.FotoMortalitas))
            {
                response.FotoMortalitasBase64 = await ConvertImageToBase64(mortalitas.FotoMortalitas);
            }

            return response;
        }

        // Get mortalitas by ayam ID
        public async Task<IEnumerable<Mortalitas>> GetMortalitasByAyamAsync(Guid ayamId)
        {
            return await _mortalitasRepository.GetByAyamIdAsync(ayamId);
        }

        // Get mortalitas by period
        public async Task<IEnumerable<Mortalitas>> GetMortalitasByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _mortalitasRepository.GetByDateRangeAsync(startDate, endDate);
        }

        // Get total mortalitas by kandang
        public async Task<int> GetTotalMortalitasByKandangAsync(Guid kandangId)
        {
            return await _mortalitasRepository.GetTotalMortalitasByKandangAsync(kandangId);
        }

        // Get total mortalitas by period
        public async Task<int> GetTotalMortalitasByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _mortalitasRepository.GetTotalMortalitasByDateRangeAsync(startDate, endDate);
        }

        /// <summary>
        /// ? UPDATED: Create mortalitas untuk ayam BATCH TERBARU saja
        /// Method ini hanya mencatat kematian pada batch ayam terbaru (bukan FIFO)
        /// Karena dalam prakteknya, mortalitas lebih sering terjadi pada ayam batch terbaru
        /// </summary>
        public async Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasAutoFifoAsync(
            Guid kandangId,
            DateTime tanggalKematian,
            TimeOnly waktuKematian,
            int jumlahKematian,
            string penyebabKematian,
            IFormFile? fotoMortalitas = null)
        {
            try
            {
                // Validate basic inputs
                if (jumlahKematian <= 0)
                {
                    return (false, "Jumlah kematian harus lebih dari 0.", null);
                }

                if (string.IsNullOrWhiteSpace(penyebabKematian))
                {
                    return (false, "Penyebab kematian wajib diisi.", null);
                }

                // ?? COMMENTED FOR DEVELOPMENT - Uncomment in production
                // if (tanggalKematian > DateTime.UtcNow)
                // {
                //     return (false, "Tanggal kematian tidak boleh di masa depan.", null);
                // }

                // Ensure tanggalKematian is UTC
                if (tanggalKematian.Kind == DateTimeKind.Unspecified)
                {
                    tanggalKematian = DateTime.SpecifyKind(tanggalKematian, DateTimeKind.Utc);
                }
                else if (tanggalKematian.Kind == DateTimeKind.Local)
                {
                    tanggalKematian = tanggalKematian.ToUniversalTime();
                }

                // Get all ayam in kandang, ordered by TanggalMasuk (newest first)
                var ayamList = (await _ayamRepository.GetByKandangIdAsync(kandangId))
                    .OrderByDescending(a => a.TanggalMasuk)
                    .ToList();

                if (!ayamList.Any())
                {
                    return (false, "Tidak ada data ayam di kandang ini.", null);
                }

                // ? Get batch TERBARU (newest batch)
                var ayamTerbaru = ayamList.First();

                // Validate stock availability
                var totalAyamTersedia = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                    ayamTerbaru.Id, tanggalKematian);

                if (jumlahKematian > totalAyamTersedia)
                {
                    return (false, 
                        $"Jumlah kematian ({jumlahKematian}) melebihi sisa ayam yang tersedia ({totalAyamTersedia}). " +
                        $"Batch ayam terbaru masuk pada {ayamTerbaru.TanggalMasuk:dd/MM/yyyy}.", null);
                }

                // Create mortalitas for batch terbaru
                var mortalitas = new Mortalitas
                {
                    AyamId = ayamTerbaru.Id,
                    TanggalKematian = tanggalKematian,
                    WaktuKematian = waktuKematian,
                    JumlahKematian = jumlahKematian,
                    PenyebabKematian = penyebabKematian
                };

                var result = await CreateAsync(mortalitas, fotoMortalitas);
                if (!result.Success)
                {
                    return (false, $"Gagal membuat mortalitas: {result.Message}", null);
                }

                var message = $"Berhasil mencatat {jumlahKematian} ekor kematian pada batch ayam terbaru " +
                    $"(masuk: {ayamTerbaru.TanggalMasuk:dd/MM/yyyy} pukul {waktuKematian:HH:mm}). " +
                    $"Sisa ayam di batch ini: {totalAyamTersedia - jumlahKematian} ekor.";

                return (true, message, new List<Mortalitas> { result.Data! });
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Create mortalitas dengan Manual Split - user menentukan jumlah dari ayam lama dan baru
        /// Ini adalah cara yang BENAR untuk handle mortalitas dengan distribusi manual
        /// </summary>
        public async Task<(bool Success, string Message, List<Mortalitas>? Data)> CreateMortalitasManualSplitAsync(
            Guid kandangId,
            DateTime tanggalKematian,
            TimeOnly waktuKematian,
            int jumlahDariAyamLama,
            int jumlahDariAyamBaru,
            string penyebabKematian,
            IFormFile? fotoMortalitas = null)
        {
            try
            {
                var totalKematian = jumlahDariAyamLama + jumlahDariAyamBaru;

                // Validate basic inputs
                if (totalKematian <= 0)
                {
                    return (false, "Total jumlah kematian harus lebih dari 0.", null);
                }

                if (jumlahDariAyamLama < 0 || jumlahDariAyamBaru < 0)
                {
                    return (false, "Jumlah kematian tidak boleh negatif.", null);
                }

                if (string.IsNullOrWhiteSpace(penyebabKematian))
                {
                    return (false, "Penyebab kematian wajib diisi.", null);
                }

                // ?? COMMENTED FOR DEVELOPMENT - Uncomment in production
                // if (tanggalKematian > DateTime.UtcNow)
                // {
                //     return (false, "Tanggal kematian tidak boleh di masa depan.", null);
                // }

                // Ensure tanggalKematian is UTC
                if (tanggalKematian.Kind == DateTimeKind.Unspecified)
                {
                    tanggalKematian = DateTime.SpecifyKind(tanggalKematian, DateTimeKind.Utc);
                }
                else if (tanggalKematian.Kind == DateTimeKind.Local)
                {
                    tanggalKematian = tanggalKematian.ToUniversalTime();
                }

                // Get all ayam in kandang, ordered by TanggalMasuk
                var ayamList = (await _ayamRepository.GetByKandangIdAsync(kandangId))
                    .OrderBy(a => a.TanggalMasuk)
                    .ToList();

                if (!ayamList.Any())
                {
                    return (false, "Tidak ada data ayam di kandang ini.", null);
                }

                // Split into "lama" (oldest) and "baru" (newest)
                var ayamLama = ayamList.First();  // Oldest entry
                var ayamBaru = ayamList.Last();   // Newest entry

                // Validate stock availability for ayam lama
                if (jumlahDariAyamLama > 0)
                {
                    var totalAyamLamaSebelum = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                        ayamLama.Id, tanggalKematian);

                    if (jumlahDariAyamLama > totalAyamLamaSebelum)
                    {
                        return (false, 
                            $"Jumlah kematian dari ayam lama ({jumlahDariAyamLama}) melebihi sisa ayam yang tersedia ({totalAyamLamaSebelum}). " +
                            $"Ayam lama masuk pada {ayamLama.TanggalMasuk:dd/MM/yyyy}.", null);
                    }
                }

                // Validate stock availability for ayam baru
                if (jumlahDariAyamBaru > 0)
                {
                    var totalAyamBaruSebelum = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                        ayamBaru.Id, tanggalKematian);

                    if (jumlahDariAyamBaru > totalAyamBaruSebelum)
                    {
                        return (false, 
                            $"Jumlah kematian dari ayam baru ({jumlahDariAyamBaru}) melebihi sisa ayam yang tersedia ({totalAyamBaruSebelum}). " +
                            $"Ayam baru masuk pada {ayamBaru.TanggalMasuk:dd/MM/yyyy}.", null);
                    }
                }

                // Create mortalitas records
                var mortalitasList = new List<Mortalitas>();

                // Create mortalitas for ayam lama if any
                if (jumlahDariAyamLama > 0)
                {
                    var mortalitasLama = new Mortalitas
                    {
                        AyamId = ayamLama.Id,
                        TanggalKematian = tanggalKematian,
                        WaktuKematian = waktuKematian,
                        JumlahKematian = jumlahDariAyamLama,
                        PenyebabKematian = penyebabKematian
                    };

                    var resultLama = await CreateAsync(mortalitasLama, fotoMortalitas);
                    if (!resultLama.Success)
                    {
                        return (false, $"Gagal membuat mortalitas untuk ayam lama: {resultLama.Message}", null);
                    }

                    mortalitasList.Add(resultLama.Data!);
                }

                // Create mortalitas for ayam baru if any
                if (jumlahDariAyamBaru > 0)
                {
                    var mortalitasBaru = new Mortalitas
                    {
                        AyamId = ayamBaru.Id,
                        TanggalKematian = tanggalKematian,
                        WaktuKematian = waktuKematian,
                        JumlahKematian = jumlahDariAyamBaru,
                        PenyebabKematian = penyebabKematian
                    };

                    var resultBaru = await CreateAsync(mortalitasBaru, fotoMortalitas);
                    if (!resultBaru.Success)
                    {
                        return (false, $"Gagal membuat mortalitas untuk ayam baru: {resultBaru.Message}", null);
                    }

                    mortalitasList.Add(resultBaru.Data!);
                }

                var message = $"Berhasil membuat {mortalitasList.Count} record mortalitas pada pukul {waktuKematian:HH:mm}. " +
                    $"Total kematian: {totalKematian} ekor " +
                    $"(Ayam lama: {jumlahDariAyamLama}, Ayam baru: {jumlahDariAyamBaru}).";

                return (true, message, mortalitasList);
            }
            catch (Exception ex)
            {
                return (false, $"Error saat membuat mortalitas Manual Split: {ex.Message}", null);
            }
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(Mortalitas entity)
        {
            // Validate ayam exists
            var ayam = await _ayamRepository.GetByIdAsync(entity.AyamId);
            if (ayam == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Data ayam tidak ditemukan." };
            }

            // Validate jumlah kematian tidak melebihi total ayam yang ada
            var totalAyamSebelum = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                entity.AyamId, entity.TanggalKematian);
            
            if (entity.JumlahKematian > totalAyamSebelum)
            {
                return new ValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"Jumlah kematian ({entity.JumlahKematian}) tidak boleh melebihi total ayam yang ada ({totalAyamSebelum})." 
                };
            }

            // Validate tanggal kematian tidak di masa depan
            // if (entity.TanggalKematian > DateTime.UtcNow)
            // {
            //     return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal kematian tidak boleh di masa depan." };
            // }

            // Validate jumlah kematian > 0
            if (entity.JumlahKematian <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Jumlah kematian harus lebih dari 0." };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task BeforeCreateAsync(Mortalitas entity)
        {
            // Ensure UTC datetime
            if (entity.TanggalKematian.Kind == DateTimeKind.Unspecified)
            {
                // Treat unspecified as UTC
                entity.TanggalKematian = DateTime.SpecifyKind(entity.TanggalKematian, DateTimeKind.Utc);
            }
            else if (entity.TanggalKematian.Kind == DateTimeKind.Local)
            {
                entity.TanggalKematian = entity.TanggalKematian.ToUniversalTime();
            }
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Mortalitas entity, Mortalitas existingEntity)
        {
            // Ensure UTC datetime
            if (entity.TanggalKematian.Kind == DateTimeKind.Unspecified)
            {
                // Treat unspecified as UTC
                entity.TanggalKematian = DateTime.SpecifyKind(entity.TanggalKematian, DateTimeKind.Utc);
            }
            else if (entity.TanggalKematian.Kind == DateTimeKind.Local)
            {
                entity.TanggalKematian = entity.TanggalKematian.ToUniversalTime();
            }
            await Task.CompletedTask;
        }

        // Overload CreateAsync with IFormFile support
        public new async Task<(bool Success, string Message, Mortalitas? Data)> CreateAsync(Mortalitas entity, IFormFile? fotoMortalitas = null)
        {
            // Handle file upload
            if (fotoMortalitas != null && fotoMortalitas.Length > 0)
            {
                entity.FotoMortalitas = await SaveFileAsync(fotoMortalitas);
            }

            return await base.CreateAsync(entity);
        }

        // Overload UpdateAsync with IFormFile support
        public new async Task<(bool Success, string Message)> UpdateAsync(Mortalitas entity, IFormFile? fotoMortalitas = null)
        {
            // Get existing entity
            var existingEntity = await _mortalitasRepository.GetByIdNoTrackingAsync(entity.Id);
            if (existingEntity == null)
            {
                return (false, "Data mortalitas tidak ditemukan.");
            }

            // Handle file upload
            if (fotoMortalitas != null && fotoMortalitas.Length > 0)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingEntity.FotoMortalitas))
                {
                    DeleteFile(existingEntity.FotoMortalitas);
                }
                entity.FotoMortalitas = await SaveFileAsync(fotoMortalitas);
            }
            else
            {
                // Keep existing photo if no new photo provided
                entity.FotoMortalitas = existingEntity.FotoMortalitas;
            }

            return await base.UpdateAsync(entity);
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            // Get web root path or use ContentRootPath as fallback
            var webRoot = _environment.WebRootPath ?? _environment.ContentRootPath;

            // If WebRootPath is null, create wwwroot folder in ContentRootPath
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(webRoot))
                {
                    Directory.CreateDirectory(webRoot);
                }
            }

            var uploadsFolder = Path.Combine(webRoot, "uploads", "mortalitas");
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

            return $"/uploads/mortalitas/{fileName}";
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var fullPath = Path.Combine(webRoot, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Log error but don't throw
            }
        }

        // REMOVED: AfterCreateAsync yang mengurangi JumlahMasuk
        // JumlahMasuk seharusnya TIDAK PERNAH berubah (data historis)
        // Sisa ayam dihitung dengan: JumlahMasuk - TotalMortalitas - TotalPanen

        // REMOVED: AfterDeleteAsync yang mengembalikan JumlahMasuk
        // Karena JumlahMasuk tidak lagi diubah saat CREATE, tidak perlu rollback saat DELETE

        /// <summary>
        /// Convert image file to base64 string
        /// </summary>
        private async Task<string?> ConvertImageToBase64(string imagePath)
        {
            try
            {
                var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var fullPath = Path.Combine(webRoot, imagePath.TrimStart('/'));

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                // Read file as bytes
                var imageBytes = await File.ReadAllBytesAsync(fullPath);

                // Detect content type from file extension
                var extension = Path.GetExtension(fullPath).ToLower();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".webp" => "image/webp",
                    _ => "image/jpeg" // default
                };

                // Convert to base64 with data URI prefix
                var base64String = Convert.ToBase64String(imageBytes);
                return $"data:{contentType};base64,{base64String}";
            }
            catch
            {
                // If conversion fails, return null
                return null;
            }
        }
    }
}
