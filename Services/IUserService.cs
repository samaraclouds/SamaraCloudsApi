using SamaraCloudsApi.Models;

namespace SamaraCloudsApi.Services
{
    /// <summary>
    /// Interface for user business logic operations (SP-based)
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get user detail by username
        /// </summary>
        /// <param name="username">The username to search</param>
        /// <returns>UserDto or null if not found</returns>
        Task<UserDto?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Update password for specified username
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="newPasswordHash">New password hash (hashed with BCrypt)</param>
        /// <returns>True jika berhasil update</returns>
        Task<bool> UpdatePasswordAsync(string username, string newPasswordHash);
        
        // Tambahkan signature method lain jika diperlukan
        // Task<bool> RegisterAsync(UserCreateRequest user);
    }
}
