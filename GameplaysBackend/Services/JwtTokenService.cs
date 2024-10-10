using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace GameplaysBackend.Services
{
    public class JwtTokenService
    {
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenService()
        {
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string CreateToken(Dictionary<string, object> payload, int expirationHours)
        {
            var configuration = new ConfigurationBuilder()
                                    .AddUserSecrets<JwtTokenService>()
                                    .Build();
            
            string? hmacSecretKey = configuration["HmacSecretKey"];
            
            SymmetricSecurityKey? symmetricSecurityKey = null;

            if (hmacSecretKey != null)
            {
                symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(hmacSecretKey));
            }

            var claims = payload.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
