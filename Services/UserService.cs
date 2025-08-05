using Dapper;
using SamaraCloudsApi.Data;
using SamaraCloudsApi.Models;
using System.Data;

namespace SamaraCloudsApi.Services
{
    public class UserService : IUserService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public UserService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<UserDto>(
                "sp_user_login_api",
                new { username },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> UpdatePasswordAsync(string username, string newPasswordHash)
        {
            using var connection = _connectionFactory.CreateConnection();
            // Asumsikan SP update password (best practice, jangan direct SQL update!)
            var rows = await connection.ExecuteAsync(
                "sp_user_update_password", // SP ini harus ada di DB kamu
                new { username, newPasswordHash },
                commandType: CommandType.StoredProcedure
            );
            return rows > 0;
        }
    }
}
