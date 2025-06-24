using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GameplaysApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICookieService _cookieService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly string _cookieName = "jwt";
        
        public AuthService(
            IHttpContextAccessor contextAccessor,
            ICookieService cookieService,
            IJwtTokenService jwtTokenService)
        {
            _contextAccessor = contextAccessor;
            _cookieService = cookieService;
            _jwtTokenService = jwtTokenService;

        }

        public void CreateAuthCookie(User user, HttpResponse response)
        {
            var expirationDays = 1;
            var validIssuer = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDISSUERS");
            var validAudience = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDAUDIENCES");
            
            if (validIssuer != null && validAudience != null)
            {
                // create JWT
                var payload = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
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

        public string GetCurrentUserId()
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = _contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId == null)
            {
                throw new InvalidOperationException("User ID claim (sub) is missing.");
            }
            return userId;
        }
    }
}
