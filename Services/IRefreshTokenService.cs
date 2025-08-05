using SamaraCloudsApi.Models;

namespace SamaraCloudsApi.Services
{
    /// <summary>
    /// Interface untuk operasi bisnis refresh token (SP-based)
    /// </summary>
    public interface IRefreshTokenService
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetAsync(string token);
        Task RevokeAsync(string token, string? replacedBy = null);
        Task RevokeAllAsync(string username);
    }
}
