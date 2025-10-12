using Microsoft.AspNetCore.Mvc;
using SIMTernakAyam.DTOs.User;
using SIMTernakAyam.Enums;
using SIMTernakAyam.Services.Interfaces;

namespace SIMTernakAyam.Controllers
{
    /// <summary>
    /// User Controller untuk mengelola user
    /// Extends BaseController untuk memanfaatkan helper methods
    /// </summary>
    [ApiController]
    //[Route("api/[controller]")]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Mendapatkan semua user
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Common.ApiResponse<List<UserResponseDto>>), 200)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                var response = UserResponseDto.FromEntities(users);
                return Success(response, "Berhasil mengambil semua user.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan user berdasarkan ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User detail</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);

                if (user == null)
                {
                    return NotFound("User tidak ditemukan.");
                }

                var response = UserResponseDto.FromEntity(user);
                return Success(response, "Berhasil mengambil user.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan user berdasarkan username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User detail</returns>
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(Common.ApiResponse<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    return NotFound("User tidak ditemukan.");
                }

                var response = UserResponseDto.FromEntity(user);
                return Success(response, "Berhasil mengambil user.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mendapatkan user berdasarkan role
        /// </summary>
        /// <param name="role">Role enum</param>
        /// <returns>List of users dengan role tertentu</returns>
        [HttpGet("role/{role}")]
        [ProducesResponseType(typeof(Common.ApiResponse<List<UserResponseDto>>), 200)]
        public async Task<IActionResult> GetUsersByRole(RoleEnum role)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(role);
                var response = UserResponseDto.FromEntities(users);
                return Success(response, $"Berhasil mengambil user dengan role {role}.");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Membuat user baru
        /// </summary>
        /// <param name="dto">Data user baru</param>
        /// <returns>User yang baru dibuat</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Common.ApiResponse<UserResponseDto>), 201)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                // Validasi ModelState
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Map DTO ke Entity
                var user = new Models.User
                {
                    Username = dto.Username,
                    Password = dto.Password,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    NoWA = dto.NoWA,
                    Role = dto.Role
                };

                // Create user via service
                var result = await _userService.CreateAsync(user);

                if (!result.Success)
                {
                    return Error(result.Message, 400);
                }

                var response = UserResponseDto.FromEntity(result.Data!);
                return Created(response, result.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mengupdate user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">Data user yang akan diupdate</param>
        /// <returns>Success message</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                // Validasi ModelState
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Validasi ID dari route harus sama dengan ID di body
                if (id != dto.Id)
                {
                    return Error("ID di URL tidak sesuai dengan ID di body.", 400);
                }

                // Map DTO ke Entity
                var user = new Models.User
                {
                    Id = dto.Id,
                    Username = dto.Username,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    NoWA = dto.NoWA,
                    Role = dto.Role,
                    Password = dto.Password ?? string.Empty // Password optional
                };

                // Update user via service
                var result = await _userService.UpdateAsync(user);

                if (!result.Success)
                {
                    if (result.Message.Contains("tidak ditemukan"))
                    {
                        return NotFound(result.Message);
                    }
                    return Error(result.Message, 400);
                }

                return Success(result.Message,200);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Menghapus user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteAsync(id);

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

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="dto">Credentials</param>
        /// <returns>User data jika login berhasil</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(Common.ApiResponse<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // Validasi ModelState
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var result = await _userService.ValidateLoginAsync(dto.Username, dto.Password);

                if (!result.Success)
                {
                    return Error(result.Message, 401);
                }

                var response = UserResponseDto.FromEntity(result.User!);
                return Success(response, result.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Mengubah password user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">Data password lama dan baru</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/change-password")]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(Common.ApiResponse<object>), 422)]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                // Validasi ModelState
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var result = await _userService.ChangePasswordAsync(id, dto.OldPassword, dto.NewPassword);

                if (!result.Success)
                {
                    if (result.Message.Contains("tidak ditemukan"))
                    {
                        return NotFound(result.Message);
                    }
                    return Error(result.Message, 400);
                }

                return Success("Password berhasil diubah.", 200);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
