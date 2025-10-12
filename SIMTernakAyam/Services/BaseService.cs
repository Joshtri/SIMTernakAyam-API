using SIMTernakAyam.Models;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Services
{
    /// <summary>
    /// Abstract base service class yang mengimplementasikan operasi CRUD standar
    /// Menggunakan konsep OOP: Abstraction, Inheritance, Polymorphism, Encapsulation
    /// </summary>
    public abstract class BaseService<T> : IBaseService<T> where T : BaseModel
    {
        protected readonly IBaseRepository<T> _repository;

        /// <summary>
        /// Constructor dengan dependency injection untuk repository
        /// </summary>
        protected BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Mendapatkan semua data
        /// Virtual untuk memungkinkan override di child class
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Mendapatkan data berdasarkan ID
        /// Virtual untuk memungkinkan override di child class
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Membuat data baru dengan validasi
        /// Virtual untuk memungkinkan override dan custom validation di child class
        /// </summary>
        public virtual async Task<(bool Success, string Message, T? Data)> CreateAsync(T entity)
        {
            try
            {
                // Validasi custom dari child class
                var validationResult = await ValidateOnCreateAsync(entity);
                if (!validationResult.IsValid)
                {
                    return (false, validationResult.ErrorMessage, null);
                }

                // Set default values - Use UTC for PostgreSQL compatibility
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdateAt = DateTime.UtcNow;

                // Hook untuk custom logic sebelum create
                await BeforeCreateAsync(entity);

                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();

                // Hook untuk custom logic setelah create
                await AfterCreateAsync(entity);

                return (true, "Data berhasil dibuat.", entity);
            }
            catch (Exception ex)
            {
                // Include inner exception untuk debugging
                var errorMessage = ex.InnerException != null
                    ? $"Terjadi kesalahan: {ex.Message}. Detail: {ex.InnerException.Message}"
                    : $"Terjadi kesalahan: {ex.Message}";
                return (false, errorMessage, null);
            }
        }

        /// <summary>
        /// Mengupdate data dengan validasi
        /// Virtual untuk memungkinkan override dan custom validation di child class
        /// </summary>
        public virtual async Task<(bool Success, string Message)> UpdateAsync(T entity)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(entity.Id);
                if (existingEntity == null)
                {
                    return (false, "Data tidak ditemukan.");
                }

                // Validasi custom dari child class
                var validationResult = await ValidateOnUpdateAsync(entity, existingEntity);
                if (!validationResult.IsValid)
                {
                    return (false, validationResult.ErrorMessage);
                }

                // Update timestamp - Use UTC for PostgreSQL compatibility
                entity.UpdateAt = DateTime.UtcNow;
                entity.CreatedAt = existingEntity.CreatedAt; // Preserve created date

                // Hook untuk custom logic sebelum update
                await BeforeUpdateAsync(entity, existingEntity);

                _repository.UpdateAsync(entity);
                await _repository.SaveChangesAsync();

                // Hook untuk custom logic setelah update
                await AfterUpdateAsync(entity);

                return (true, "Data berhasil diupdate.");
            }
            catch (Exception ex)
            {
                // Include inner exception untuk debugging
                var errorMessage = ex.InnerException != null
                    ? $"Terjadi kesalahan: {ex.Message}. Detail: {ex.InnerException.Message}"
                    : $"Terjadi kesalahan: {ex.Message}";
                return (false, errorMessage);
            }
        }

        /// <summary>
        /// Menghapus data berdasarkan ID
        /// Virtual untuk memungkinkan override di child class
        /// </summary>
        public virtual async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return (false, "Data tidak ditemukan.");
                }

                // Validasi custom sebelum delete
                var validationResult = await ValidateOnDeleteAsync(entity);
                if (!validationResult.IsValid)
                {
                    return (false, validationResult.ErrorMessage);
                }

                // Hook untuk custom logic sebelum delete
                await BeforeDeleteAsync(entity);

                _repository.Delete(entity);
                await _repository.SaveChangesAsync();

                // Hook untuk custom logic setelah delete
                await AfterDeleteAsync(entity);

                return (true, "Data berhasil dihapus.");
            }
            catch (Exception ex)
            {
                // Include inner exception untuk debugging
                var errorMessage = ex.InnerException != null
                    ? $"Terjadi kesalahan: {ex.Message}. Detail: {ex.InnerException.Message}"
                    : $"Terjadi kesalahan: {ex.Message}";
                return (false, errorMessage);
            }
        }

        /// <summary>
        /// Mengecek apakah data dengan ID tertentu ada
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null;
        }

        #region Protected Virtual Methods - Hooks untuk Custom Logic

        /// <summary>
        /// Validasi sebelum create. Override di child class untuk custom validation.
        /// </summary>
        protected virtual Task<ValidationResult> ValidateOnCreateAsync(T entity)
        {
            return Task.FromResult(new ValidationResult { IsValid = true });
        }

        /// <summary>
        /// Validasi sebelum update. Override di child class untuk custom validation.
        /// </summary>
        protected virtual Task<ValidationResult> ValidateOnUpdateAsync(T entity, T existingEntity)
        {
            return Task.FromResult(new ValidationResult { IsValid = true });
        }

        /// <summary>
        /// Validasi sebelum delete. Override di child class untuk custom validation.
        /// </summary>
        protected virtual Task<ValidationResult> ValidateOnDeleteAsync(T entity)
        {
            return Task.FromResult(new ValidationResult { IsValid = true });
        }

        /// <summary>
        /// Hook yang dipanggil sebelum create
        /// </summary>
        protected virtual Task BeforeCreateAsync(T entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook yang dipanggil setelah create
        /// </summary>
        protected virtual Task AfterCreateAsync(T entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook yang dipanggil sebelum update
        /// </summary>
        protected virtual Task BeforeUpdateAsync(T entity, T existingEntity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook yang dipanggil setelah update
        /// </summary>
        protected virtual Task AfterUpdateAsync(T entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook yang dipanggil sebelum delete
        /// </summary>
        protected virtual Task BeforeDeleteAsync(T entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook yang dipanggil setelah delete
        /// </summary>
        protected virtual Task AfterDeleteAsync(T entity)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Class untuk hasil validasi
        /// </summary>
        protected class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }

        #endregion
    }
}
