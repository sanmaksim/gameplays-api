using GameplaysApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GameplaysApi.Services
{
    public class AuthService
    {
        private readonly CookieService _cookieService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly string _cookieName = "jwt";

        public AuthService(CookieService cookieService, JwtTokenService jwtTokenService)
        {
            _cookieService = cookieService;
            _jwtTokenService = jwtTokenService;
        }

        public void CreateAuthCookie(User user, HttpResponse response)
        {
            var expirationDays = 1;
            var validIssuer = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDISSUERS");
            var validAudience = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDAUDIENCES");
            
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
