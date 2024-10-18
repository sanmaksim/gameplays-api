using GameplaysBackend.Models;
using System.Security.Claims;

namespace GameplaysBackend.Services
{
    public class AuthService
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly CookieService _cookieService;

        private readonly string _cookieName = "jwt";

        public AuthService(JwtTokenService jwtTokenService, CookieService cookieService)
        {
            _jwtTokenService = jwtTokenService;
            _cookieService = cookieService;
        }

        public void CreateAuthCookie(User user, HttpResponse response)
        {
            var expirationDays = 1;
            
            // create JWT
            var payload = new Claim[]
            {
                new Claim("id", user.UserId.ToString())
            };
            var token = _jwtTokenService.CreateToken(payload, expirationDays);

            // create cookie
            var cookieValue = token;
            _cookieService.CreateCookie(response, _cookieName, cookieValue, expirationDays);
        }

        public void DeleteAuthCookie(HttpResponse response)
        {
            _cookieService.DeleteCookie(response, _cookieName);
        }
    }
}
