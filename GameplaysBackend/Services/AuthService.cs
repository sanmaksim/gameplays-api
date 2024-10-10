using GameplaysBackend.Models;

namespace GameplaysBackend.Services
{
    public class AuthService
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly CookieService _cookieService;

        public AuthService(JwtTokenService jwtTokenService, CookieService cookieService)
        {
            _jwtTokenService = jwtTokenService;
            _cookieService = cookieService;
        }

        public void CreateAuthCookie(User user, HttpResponse response)
        {
            // create JWT
            var payload = new Dictionary<string, object>
            {
                { "id", user.UserId }
            };
            var expirationHours = 24;
            var token = _jwtTokenService.CreateToken(payload, expirationHours);

            // create cookie
            var cookieName = "jwt";
            var cookieValue = token;
            var expirationDays = 1;
            _cookieService.CreateCookie(response, cookieName, cookieValue, expirationDays);
        }
    }
}
