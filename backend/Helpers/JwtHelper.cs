using InterviewPrepSystem.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InterviewPrepSystem.Helpers
{
    public interface IJwtHelper
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtHelper : IJwtHelper
    {
        private readonly IConfiguration _config;

        public JwtHelper(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var secretKey = _config["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer    = _config["JwtSettings:Issuer"];
            var audience  = _config["JwtSettings:Audience"];
            var expiryDays = int.Parse(_config["JwtSettings:ExpiryInDays"] ?? "7");

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name,  user.Name),
                new Claim(ClaimTypes.Role,               user.Role),
                new Claim("userId",                      user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var secretKey = _config["JwtSettings:SecretKey"] ?? "";
                var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var handler    = new JwtSecurityTokenHandler();
                var principal  = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                    ValidateIssuer           = true,
                    ValidIssuer              = _config["JwtSettings:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = _config["JwtSettings:Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch { return null; }
        }
    }

    // Extension to get UserId from ClaimsPrincipal
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("userId") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out var id) ? id : 0;
        }
    }
}
