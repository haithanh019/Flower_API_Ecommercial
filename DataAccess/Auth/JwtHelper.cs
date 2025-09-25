using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Business.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DataAccess.Auth
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(
            int userId,
            string email,
            UserRole role,
            IConfiguration config,
            TimeSpan? lifetime = null
        )
        {
            var expires = DateTime.UtcNow.Add(
                lifetime
                    ?? TimeSpan.FromMinutes(
                        int.TryParse(config["Jwt:ExpiresMinutes"], out var m) ? m : 30
                    )
            );

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
