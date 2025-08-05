using GameplaysApi.Config;
using GameplaysApi.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GameplaysApi.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly AuthConfig _authConfig;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenService(IOptions<AuthConfig> authConfig)
        {
            _authConfig = authConfig.Value;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string CreateToken(List<Claim> claims, TimeSpan expiresIn)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfig.HmacSecretKey));
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
    }
}
