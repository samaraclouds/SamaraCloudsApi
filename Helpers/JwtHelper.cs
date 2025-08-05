using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SamaraCloudsApi.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generate JWT Access Token
        /// </summary>
        public string GenerateAccessToken(string username)
        {
            var jwtConfig = _configuration.GetSection("Jwt");

            var secret = jwtConfig["Secret"];
            var issuer = jwtConfig["Issuer"];
            var audience = jwtConfig["Audience"];
            var expiresStr = jwtConfig["ExpiresInMinutes"];

            if (string.IsNullOrWhiteSpace(secret) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(expiresStr))
            {
                throw new Exception("JWT config is not set properly in appsettings.json");
            }

            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                    // Tambahkan claim lain jika perlu, misal role, dsb
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(expiresStr)), // Expiry pendek, ex: 10-15 menit
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate Secure Refresh Token (random 64 bytes, base64)
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
