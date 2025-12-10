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
        /// 

        public virtual async Task<(bool Success, string Message, T? Data)> CreateAsync(T entity)
        {
            try
            {
                // Null check for entity
                if (entity == null)
                {
                    return (false, "Entity cannot be null.", null);
                }

                // Set default values - Use UTC for PostgreSQL compatibility
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdateAt = DateTime.UtcNow;

                // Hook untuk custom logic sebelum create (but before saving to DB)
                try
                {
                    await BeforeCreateAsync(entity);
                }
                catch (Exception ex)
                {
                    return (false, $"Error in BeforeCreateAsync: {ex.Message}", null);
                }

                // Validasi custom dari child class - HARUS SEBELUM AddAsync
                ValidationResult validationResult;
                try
                {
                    validationResult = await ValidateOnCreateAsync(entity);
                }
                catch (Exception ex)
                {
                    return (false, $"Error during validation: {ex.Message}", null);
                }

                if (!validationResult.IsValid)
                {
                    return (false, validationResult.ErrorMessage, null);
                }

                // Check if _repository is null
                if (_repository == null)
                {
                    return (false, "Repository is null.", null);
                }

                // Only add to database AFTER all validations pass
                try
                {
                    await _repository.AddAsync(entity);
                    await _repository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return (false, $"Error saving to database: {ex.Message}", null);
                }

                // Hook untuk custom logic setelah create
                try
                {
                    await AfterCreateAsync(entity);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the entire operation since data is already saved
                    // You might want to log this properly in production
                }

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
                // Get existing entity without tracking
                var existingEntity = await GetByIdNoTrackingAsync(entity.Id);
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

                // ? Clear any existing tracked entities to prevent conflicts
                _repository.ClearTrackedEntities();

                // Update entity
                _repository.UpdateAsync(entity);
                await _repository.SaveChangesAsync();

                // ? Detach entity after save to prevent future conflicts
                _repository.DetachEntity(entity);

                // Pass existingEntity to AfterUpdateAsync to avoid new tracking
                await AfterUpdateAsync(entity, existingEntity);

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
        /// Mendapatkan data berdasarkan ID tanpa tracking untuk menghindari konflik saat update
        /// </summary>
        protected virtual async Task<T?> GetByIdNoTrackingAsync(Guid id)
        {
            return await _repository.GetByIdNoTrackingAsync(id);
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
        /// Hook yang dipanggil setelah update (with existing entity to avoid tracking conflicts)
        /// </summary>
        protected virtual Task AfterUpdateAsync(T entity, T existingEntity)
        {
            // Default implementation calls the original method for backward compatibility
            return AfterUpdateAsync(entity);
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
