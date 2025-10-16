using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.Common;
using SIMTernakAyam.DTOs.Auth;
using SIMTernakAyam.DTOs.User;
using SIMTernakAyam.Services;
using SIMTernakAyam.Services.Interfaces;
using System.Security.Claims;

namespace SIMTernakAyam.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IJwtService jwtService, IConfiguration configuration)
        {
            _userService = userService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        /// <summary>
        /// Login user dengan JWT token
        /// </summary>
        /// <param name="dto">Credentials</param>
        /// <returns>JWT Token dan user info</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(Common.ApiResponse<LoginResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var result = await _userService.ValidateLoginAsync(dto.Username, dto.Password);

                if (!result.Success)
                {
                    return Error(result.Message, 401);
                }

                // Generate JWT token
                var accessToken = _jwtService.GenerateToken(result.User!);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!));

                var response = LoginResponseDto.FromUser(result.User!, accessToken, refreshToken, expiresAt);

                return Success(response, "Login berhasil.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="dto">Refresh token</param>
        /// <returns>New access token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(Common.ApiResponse<LoginResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Note: Dalam implementasi production, refresh token harus disimpan di database
                // dan divalidasi. Untuk sementara, kita generate token baru berdasarkan current user
                
                return Error("Refresh token tidak valid atau expired.", 401);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get current user information (/me endpoint)
        /// </summary>
        /// <returns>Current user data</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), 200)]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid token");
                }

                // ✅ Gunakan method baru yang include kandang info
                var currentUser = await _userService.GetCurrentUserWithKandangsAsync(userId);
                if (currentUser == null)
                {
                    return NotFound("User tidak ditemukan");
                }

                return Success(currentUser, "Berhasil mengambil informasi user");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Logout user (client-side token removal)
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Dalam JWT, logout biasanya dilakukan di client-side dengan menghapus token
                // Untuk server-side logout, bisa implement token blacklist di Redis/Database
                
                return Success("Logout berhasil. Silakan hapus token dari client.", 200);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Register user baru
        /// </summary>
        /// <param name="dto">Data user baru</param>
        /// <returns>User data dan token</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Common.ApiResponse<LoginResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var user = new Models.User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    NoWA = dto.NoWA,
                    Role = dto.Role
                };

                var result = await _userService.CreateAsync(user);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                // Auto login after register
                var accessToken = _jwtService.GenerateToken(result.Data!);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!));

                var response = LoginResponseDto.FromUser(result.Data!, accessToken, refreshToken, expiresAt);

                return Created(response, "Registrasi berhasil.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}