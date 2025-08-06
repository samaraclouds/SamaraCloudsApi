using SamaraCloudsApi.Helpers;
using SamaraCloudsApi.Models;
using SamaraCloudsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SamaraCloudsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly JwtHelper _jwtHelper;

        public AuthController(
            IUserService userService,
            IRefreshTokenService refreshTokenService,
            JwtHelper jwtHelper)
        {
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _jwtHelper = jwtHelper;
        }

        /// <summary>
        /// Login endpoint. Returns JWT access & refresh tokens.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "EMPTY_REQUEST",
                    Message = "Request body cannot be empty.",
                    Errors = null,
                    Data = null
                });

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "VALIDATION_ERROR",
                    Message = "Username and password are required.",
                    Errors = null,
                    Data = null
                });

            var user = await _userService.GetUserByUsernameAsync(request.Username);
            if (user == null)
                return Unauthorized(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Code = "INVALID_CREDENTIALS",
                    Message = "Username or password is incorrect.",
                    Errors = null,
                    Data = null
                });

            if (string.IsNullOrEmpty(user.PasswordHash))
                return BadRequest(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "INVALID_USER",
                    Message = "Password in database is empty.",
                    Errors = null,
                    Data = null
                });

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Code = "INVALID_CREDENTIALS",
                    Message = "Username or password is incorrect.",
                    Errors = null,
                    Data = null
                });

            var accessToken = _jwtHelper.GenerateAccessToken(user.Username);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Save refresh token to DB
            var refresh = new RefreshToken
            {
                Token = refreshToken,
                Username = user.Username,
                ExpiryDate = DateTime.UtcNow.AddDays(14),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenService.AddAsync(refresh);

            return Ok(new ApiResponse<object>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Login successful.",
                Errors = null,
                Data = new
                {
                    accessToken,
                    refreshToken
                }
            });
        }

        /// <summary>
        /// Refresh token endpoint. Returns new JWT access & refresh tokens.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var token = await _refreshTokenService.GetAsync(refreshToken);
            if (token == null || token.IsRevoked || token.ExpiryDate < DateTime.UtcNow)
                return Unauthorized(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Code = "INVALID_REFRESH_TOKEN",
                    Message = "Refresh token is invalid or expired.",
                    Errors = null,
                    Data = null
                });

            var newAccessToken = _jwtHelper.GenerateAccessToken(token.Username);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken();
            await _refreshTokenService.RevokeAsync(refreshToken, newRefreshToken);
            var newTokenObj = new RefreshToken
            {
                Token = newRefreshToken,
                Username = token.Username,
                ExpiryDate = DateTime.UtcNow.AddDays(14),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenService.AddAsync(newTokenObj);

            return Ok(new ApiResponse<object>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Token refreshed.",
                Errors = null,
                Data = new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken
                }
            });
        }

        /// <summary>
        /// Logout endpoint. Revokes refresh token.
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            await _refreshTokenService.RevokeAsync(refreshToken);
            return Ok(new ApiResponse<object>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Logout successful.",
                Errors = null,
                Data = null
            });
        }

        /// <summary>
        /// Change password endpoint (user must be logged in).
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "VALIDATION_ERROR",
                    Message = "Old password and new password are required.",
                    Errors = null,
                    Data = null
                });

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Code = "UNAUTHORIZED",
                    Message = "Unauthorized.",
                    Errors = null,
                    Data = null
                });

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return Unauthorized(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Code = "USER_NOT_FOUND",
                    Message = "User not found.",
                    Errors = null,
                    Data = null
                });

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return BadRequest(new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "INVALID_OLD_PASSWORD",
                    Message = "Old password is incorrect.",
                    Errors = null,
                    Data = null
                });

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            var updated = await _userService.UpdatePasswordAsync(username, newPasswordHash);
            if (!updated)
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Code = "UPDATE_FAILED",
                    Message = "Failed to update password.",
                    Errors = null,
                    Data = null
                });

            return Ok(new ApiResponse<object>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Password changed successfully.",
                Errors = null,
                Data = null
            });
        }
    }
}
