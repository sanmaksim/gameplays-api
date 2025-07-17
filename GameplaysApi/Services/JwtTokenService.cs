using GameplaysApi.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GameplaysApi.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenService()
        {
            // _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string CreateToken(List<Claim> claims, TimeSpan expiresIn)
        {
            string? hmacSecretKey = Environment.GetEnvironmentVariable("JWT_HMACSECRETKEY");

            if (hmacSecretKey != null)
            {
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(hmacSecretKey));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTimeOffset.UtcNow.Add(expiresIn).UtcDateTime,
                    SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = _tokenHandler.WriteToken(token);

                return tokenString;
            }
            else
            {
                throw new Exception("HmacSecretKey value must not be null.");
            }
        }
    }
}
