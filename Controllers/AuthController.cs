using SamaraCloudsApi.Helpers;
using SamaraCloudsApi.Models;
using SamaraCloudsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                return BadRequest(new
                {
                    success = false,
                    error = "empty_request",
                    message = "Request body cannot be empty."
                });

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new
                {
                    success = false,
                    error = "validation_error",
                    message = "Username and password are required."
                });

            var user = await _userService.GetUserByUsernameAsync(request.Username);
            if (user == null)
                return Unauthorized(new
                {
                    success = false,
                    error = "invalid_credentials",
                    message = "Username or password is incorrect."
                });

            if (string.IsNullOrEmpty(user.PasswordHash))
                return BadRequest(new
                {
                    success = false,
                    error = "invalid_user",
                    message = "Password in database is empty."
                });

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new
                {
                    success = false,
                    error = "invalid_credentials",
                    message = "Username or password is incorrect."
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

            return Ok(new
            {
                success = true,
                message = "Login successful.",
                data = new
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
                return Unauthorized(new
                {
                    success = false,
                    error = "invalid_refresh_token",
                    message = "Refresh token is invalid or expired."
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

            return Ok(new
            {
                success = true,
                message = "Token refreshed.",
                data = new
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
            return Ok(new
            {
                success = true,
                message = "Logout successful."
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
                return BadRequest(new
                {
                    success = false,
                    error = "validation_error",
                    message = "Old password and new password are required."
                });

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new
                {
                    success = false,
                    error = "unauthorized",
                    message = "Unauthorized."
                });

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return Unauthorized(new
                {
                    success = false,
                    error = "user_not_found",
                    message = "User not found."
                });

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return BadRequest(new
                {
                    success = false,
                    error = "invalid_old_password",
                    message = "Old password is incorrect."
                });

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            var updated = await _userService.UpdatePasswordAsync(username, newPasswordHash);
            if (!updated)
                return StatusCode(500, new
                {
                    success = false,
                    error = "update_failed",
                    message = "Failed to update password."
                });

            return Ok(new
            {
                success = true,
                message = "Password changed successfully."
            });
        }
    }
}
