namespace SIMTernakAyam.Common
{
    /// <summary>
    /// Generic API Response wrapper untuk standardisasi response
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Status keberhasilan request
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Pesan response
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Data response (nullable jika error)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error details (untuk validation errors)
        /// </summary>
        public Dictionary<string, List<string>>? Errors { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Constructor untuk success response
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Berhasil", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Constructor untuk error response
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, Dictionary<string, List<string>>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Constructor untuk validation error response
        /// </summary>
        public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, List<string>> errors, string message = "Validasi gagal")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors,
                StatusCode = 422,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Constructor untuk not found response
        /// </summary>
        public static ApiResponse<T> NotFoundResponse(string message = "Data tidak ditemukan")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                StatusCode = 404,
                Timestamp = DateTime.Now
            };
        }
    }

    /// <summary>
    /// API Response tanpa data (untuk delete, update, dll)
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public new static ApiResponse SuccessResponse(string message = "Berhasil", int statusCode = 200)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = statusCode,
                Timestamp = DateTime.Now
            };
        }

        public new static ApiResponse ErrorResponse(string message, int statusCode = 400)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Timestamp = DateTime.Now
            };
        }
    }
}
