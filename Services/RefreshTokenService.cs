using Dapper;
using SamaraCloudsApi.Data;
using SamaraCloudsApi.Models;
using System.Data;

namespace SamaraCloudsApi.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public RefreshTokenService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task AddAsync(RefreshToken token)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(
                "sp_refresh_token_insert",
                new
                {
                    token = token.Token,
                    username = token.Username,
                    expiry_date = token.ExpiryDate,
                    is_revoked = token.IsRevoked,
                    created_at = token.CreatedAt,
                    replaced_by_token = token.ReplacedByToken
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<RefreshToken?> GetAsync(string tokenValue)
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<RefreshToken>(
                "sp_refresh_token_get",
                new { token = tokenValue },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task RevokeAsync(string tokenValue, string? replacedBy = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(
                "sp_refresh_token_revoke",
                new
                {
                    token = tokenValue,
                    replaced_by_token = replacedBy
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task RevokeAllAsync(string username)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(
                "sp_refresh_token_revoke_all",
                new { username },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
