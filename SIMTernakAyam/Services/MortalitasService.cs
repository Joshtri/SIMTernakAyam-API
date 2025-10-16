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

        public MortalitasService(
            IMortalitasRepository mortalitasRepository, 
            IAyamRepository ayamRepository,
            IKandangRepository kandangRepository) : base(mortalitasRepository)
        {
            _mortalitasRepository = mortalitasRepository;
            _ayamRepository = ayamRepository;
            _kandangRepository = kandangRepository;
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
                // Calculate total ayam before mortality
                var totalSebelum = await _mortalitasRepository.GetTotalAyamBeforeMortalityAsync(
                    mortalitas.AyamId, mortalitas.TanggalKematian);

                // Get kandang capacity
                var kapasitas = await _mortalitasRepository.GetKandangCapacityAsync(
                    mortalitas.Ayam?.KandangId ?? Guid.Empty);

                var enhancedDto = MortalitasResponseDto.FromEntity(mortalitas, totalSebelum, kapasitas);
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
                
                var kapasitas = await _mortalitasRepository.GetKandangCapacityAsync(kandangId);
                
                var enhancedDto = MortalitasResponseDto.FromEntity(mortalitas, totalSebelum, kapasitas);
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

            var kapasitas = await _mortalitasRepository.GetKandangCapacityAsync(
                mortalitas.Ayam?.KandangId ?? Guid.Empty);

            return MortalitasResponseDto.FromEntity(mortalitas, totalSebelum, kapasitas);
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
            if (entity.TanggalKematian > DateTime.UtcNow)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tanggal kematian tidak boleh di masa depan." };
            }

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
    }
}