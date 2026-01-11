using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SIMTernakAyam.Common;

namespace SIMTernakAyam.Controllers
{
    /// <summary>
    /// Base Controller dengan helper methods untuk response handling
    /// Menggunakan konsep OOP: Inheritance & Encapsulation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Return success response dengan data
        /// </summary>
        protected IActionResult Success<T>(T data, string message = "Berhasil", int statusCode = 200)
        {
            var response = ApiResponse<T>.SuccessResponse(data, message, statusCode);
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Return success response tanpa data
        /// </summary>
        protected IActionResult Success(string message = "Berhasil", int statusCode = 200)
        {
            var response = ApiResponse.SuccessResponse(message, statusCode);
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Return success response dengan pagination
        /// </summary>
        protected IActionResult SuccessWithPagination<T>(T data, int totalCount, int page, int pageSize, string message = "Berhasil")
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var response = new
            {
                success = true,
                message = message,
                data = data,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                },
                errors = (object?)null,
                statusCode = 200,
                timestamp = DateTime.UtcNow
            };
            return StatusCode(200, response);
        }

        /// <summary>
        /// Return error response
        /// </summary>
        protected IActionResult Error(string message, int statusCode = 400)
        {
            var response = ApiResponse.ErrorResponse(message, statusCode);
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Return error response dengan data
        /// </summary>
        protected IActionResult Error<T>(string message, int statusCode = 400)
        {
            var response = ApiResponse<T>.ErrorResponse(message, statusCode);
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Return error response dengan data dan object tambahan
        /// </summary>
        protected IActionResult Error<T>(string message, int statusCode, T data)
        {
            var response = new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = data,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Return not found response
        /// </summary>
        protected IActionResult NotFound(string message = "Data tidak ditemukan")
        {
            var response = ApiResponse.ErrorResponse(message, 404);
            return StatusCode(404, response);
        }

        /// <summary>
        /// Return validation error response dari ModelState
        /// </summary>
        protected IActionResult ValidationError(ModelStateDictionary? modelState = null)
        {
            var errors = new Dictionary<string, List<string>>();

            if (modelState != null)
            {
                foreach (var key in modelState.Keys)
                {
                    var state = modelState[key];
                    if (state != null && state.Errors.Count > 0)
                    {
                        errors[key] = state.Errors
                            .Select(error => error.ErrorMessage)
                            .ToList();
                    }
                }
            }

            var response = ApiResponse<object>.ValidationErrorResponse(errors);
            return StatusCode(422, response);
        }

        /// <summary>
        /// Return validation error response dengan custom errors
        /// </summary>
        protected IActionResult ValidationError(Dictionary<string, List<string>> errors, string message = "Validasi gagal")
        {
            var response = ApiResponse<object>.ValidationErrorResponse(errors, message);
            return StatusCode(422, response);
        }

        /// <summary>
        /// Return created response (201)
        /// </summary>
        protected IActionResult Created<T>(T data, string message = "Data berhasil dibuat")
        {
            var response = ApiResponse<T>.SuccessResponse(data, message, 201);
            return StatusCode(201, response);
        }

        /// <summary>
        /// Return no content response (204)
        /// </summary>
        protected IActionResult NoContent()
        {
            return StatusCode(204);
        }

        /// <summary>
        /// Return unauthorized response (401)
        /// </summary>
        protected IActionResult Unauthorized(string message = "Unauthorized")
        {
            var response = ApiResponse.ErrorResponse(message, 401);
            return StatusCode(401, response);
        }

        /// <summary>
        /// Return forbidden response (403)
        /// </summary>
        protected IActionResult Forbidden(string message = "Forbidden")
        {
            var response = ApiResponse.ErrorResponse(message, 403);
            return StatusCode(403, response);
        }

        /// <summary>
        /// Return internal server error response (500)
        /// </summary>
        protected IActionResult InternalServerError(string message = "Terjadi kesalahan pada server")
        {
            var response = ApiResponse.ErrorResponse(message, 500);
            return StatusCode(500, response);
        }

        /// <summary>
        /// Handle exception dan return appropriate error response
        /// </summary>
        protected IActionResult HandleException(Exception ex)
        {
            // Log exception here
            // _logger.LogError(ex, "An error occurred");

            return InternalServerError($"Terjadi kesalahan: {ex.Message}");
        }
    }
}
