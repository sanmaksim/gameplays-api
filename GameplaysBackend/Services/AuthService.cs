using GameplaysBackend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GameplaysBackend.Services
{
    public class AuthService
    {
        private readonly CookieService _cookieService;
        private readonly IConfiguration _configuration;
        private readonly JwtTokenService _jwtTokenService;

        private readonly string _cookieName = "jwt";

        public AuthService(CookieService cookieService, IConfiguration configuration, JwtTokenService jwtTokenService)
        {
            _cookieService = cookieService;
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
        }

        public void CreateAuthCookie(User user, HttpResponse response)
        {
            var expirationDays = 1;
            //var validIssuers = _configuration.GetSection("JwtSettings:validIssuers").Get<string[]>();
            var validIssuer = _configuration["JwtSettings:validIssuers"];
            //var validAudiences = _configuration.GetSection("JwtSettings:ValidAudiences").Get<string[]>();
            var validAudience = _configuration["JwtSettings:ValidAudiences"];
            
            //if (validIssuers != null && validAudiences != null)
            if (validIssuer != null && validAudience != null)
            {
                // create JWT
                var payload = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iss, validIssuer),
                    new Claim(JwtRegisteredClaimNames.Aud, validAudience)
                };
                // foreach (var issuer in validIssuers)
                // {
                //     payload.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
                // }
                // foreach (var aud in validAudiences)
                // {
                //     payload.Add(new Claim(JwtRegisteredClaimNames.Aud, aud));
                // }
                var token = _jwtTokenService.CreateToken(payload, expirationDays);

                // create cookie
                var cookieValue = token;
                _cookieService.CreateCookie(response, _cookieName, cookieValue, expirationDays);
            }
            else
            {
                throw new Exception("Audience value must not be null.");
            }
        }

        public void DeleteAuthCookie(HttpResponse response)
        {
            _cookieService.DeleteCookie(response, _cookieName);
        }
    }
}
