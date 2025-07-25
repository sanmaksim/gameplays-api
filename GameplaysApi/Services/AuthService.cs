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
        private readonly IRefreshTokenService _refreshTokenService;

        private readonly string? _jwtCookieName = Environment.GetEnvironmentVariable("JWT_COOKIE_NAME");
        private readonly string? _jwtExpirationMinutes = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES");

        private readonly string? _refreshTokenCookieName = Environment.GetEnvironmentVariable("REFRESH_TOKEN_COOKIE_NAME");
        private readonly string? _refreshTokenExpirationDays = Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRATION_DAYS");

        private readonly string? _validIssuer = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDISSUERS");
        private readonly string? _validAudience = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDAUDIENCES");

        public AuthService(
            IHttpContextAccessor contextAccessor,
            ICookieService cookieService,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService)
        {
            _contextAccessor = contextAccessor;
            _cookieService = cookieService;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public void CreateAuthCookie(User user, HttpResponse response)
        {
            if (_validIssuer != null && _validAudience != null && _jwtCookieName != null)
            {
                // create JWT
                var payload = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iss, _validIssuer),
                    new Claim(JwtRegisteredClaimNames.Aud, _validAudience)
                };

                if (int.TryParse(_jwtExpirationMinutes, out int expirationMinutes))
                {
                    var expirationMins = TimeSpan.FromMinutes(expirationMinutes);
                    var jwt = _jwtTokenService.CreateToken(payload, expirationMins);

                    // create cookie
                    var cookieValue = jwt;
                    _cookieService.CreateCookie(response, _jwtCookieName, cookieValue, expirationMins);
                }
            }
            else
            {
                throw new Exception("Error reading configuration.");
            }
        }

        public async Task CreateRefreshTokenCookie(User user, HttpRequest request, HttpResponse response)
        {
            if (_validIssuer != null && _validAudience != null && _refreshTokenCookieName != null
                && int.TryParse(_refreshTokenExpirationDays, out int expirationDays))
            {
                var expDays = TimeSpan.FromDays(expirationDays);

                // create refresh token
                var refreshToken = await _refreshTokenService.CreateRefreshToken(user, request, expDays);

                // create cookie
                var cookieValue = refreshToken;
                _cookieService.CreateCookie(response, _refreshTokenCookieName, cookieValue, expDays);
                
            }
            else
            {
                throw new Exception("Error reading configuration.");
            }
        }

        public void DeleteAuthCookie(HttpResponse response)
        {
            if (_jwtCookieName != null)
                _cookieService.DeleteCookie(response, _jwtCookieName);
            else
                throw new ArgumentNullException($"Required value for '{nameof(_jwtCookieName)}' is missing or empty.");
        }

        public void DeleteRefreshTokenCookie(HttpResponse response)
        {
            if (_refreshTokenCookieName != null)
                _cookieService.DeleteCookie(response, _refreshTokenCookieName);
            else
                throw new ArgumentNullException($"Required value for '{nameof(_refreshTokenCookieName)}' is missing or empty.");
        }

        public string GetCurrentUserId()
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = _contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return userId == null ? throw new InvalidOperationException("Invalid user.") : userId;
        }
    }
}
