using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services.Interfaces;
using SIMTernakAyam.Enums;

namespace SIMTernakAyam.Services
{
    public class KandangAsistenService : BaseService<KandangAsisten>, IKandangAsistenService
    {
        private readonly IKandangAsistenRepository _kandangAsistenRepository;
        private readonly IKandangRepository _kandangRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAyamRepository _ayamRepository;

        public KandangAsistenService(
            IKandangAsistenRepository kandangAsistenRepository,
            IKandangRepository kandangRepository,
            IUserRepository userRepository,
            IAyamRepository ayamRepository) : base(kandangAsistenRepository)
        {
            _kandangAsistenRepository = kandangAsistenRepository;
            _kandangRepository = kandangRepository;
            _userRepository = userRepository;
            _ayamRepository = ayamRepository;
        }

        public async Task<IEnumerable<KandangAsisten>> GetAsistensByKandangAsync(Guid kandangId)
        {
            return await _kandangAsistenRepository.GetAsistensByKandangIdAsync(kandangId);
        }

        public async Task<IEnumerable<KandangAsisten>> GetKandangsByAsistenAsync(Guid asistenId)
        {
            return await _kandangAsistenRepository.GetKandangsByAsistenIdAsync(asistenId);
        }

        public async Task<IEnumerable<KandangAsisten>> GetActiveAsistensByKandangAsync(Guid kandangId)
        {
            return await _kandangAsistenRepository.GetActiveAsistensByKandangIdAsync(kandangId);
        }

        public async Task<KandangAsisten?> GetWithDetailsAsync(Guid id)
        {
            return await _kandangAsistenRepository.GetWithDetailsAsync(id);
        }

        public async Task<IEnumerable<KandangAsisten>> GetAllWithDetailsAsync()
        {
            return await _kandangAsistenRepository.GetAllWithDetailsAsync();
        }

        public async Task<IEnumerable<Models.Ayam>> GetAyamSisaByKandangAsync(Guid kandangId)
        {
            return await _ayamRepository.GetAyamSisaByKandangIdAsync(kandangId);
        }

        protected override async Task<ValidationResult> ValidateOnCreateAsync(KandangAsisten entity)
        {
            // Validasi kandang exists
            var kandang = await _kandangRepository.GetByIdAsync(entity.KandangId);
            if (kandang == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Kandang tidak ditemukan" };
            }

            // Validasi asisten (user) exists
            var asisten = await _userRepository.GetByIdAsync(entity.AsistenId);
            if (asisten == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "User asisten tidak ditemukan" };
            }

            // Validasi asisten harus role Petugas
            if (asisten.Role != RoleEnum.Petugas)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Asisten harus memiliki role Petugas" };
            }

            // Validasi asisten tidak boleh sama dengan petugas utama
            if (entity.AsistenId == kandang.petugasId)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Asisten tidak boleh sama dengan petugas utama kandang" };
            }

            // Validasi asisten belum terdaftar di kandang ini
            var exists = await _kandangAsistenRepository.IsAsistenExistsInKandangAsync(entity.KandangId, entity.AsistenId);
            if (exists)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = $"{asisten.FullName} sudah terdaftar sebagai asisten di kandang ini" };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task<ValidationResult> ValidateOnUpdateAsync(KandangAsisten entity, KandangAsisten existingEntity)
        {
            // Tidak boleh mengubah KandangId atau AsistenId
            if (entity.KandangId != existingEntity.KandangId)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tidak dapat mengubah Kandang ID" };
            }

            if (entity.AsistenId != existingEntity.AsistenId)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tidak dapat mengubah Asisten ID" };
            }

            return new ValidationResult { IsValid = true };
        }

        protected override async Task<ValidationResult> ValidateOnDeleteAsync(KandangAsisten entity)
        {
            // Bisa langsung hapus, tidak ada constraint khusus
            return await Task.FromResult(new ValidationResult { IsValid = true });
        }
    }
}
