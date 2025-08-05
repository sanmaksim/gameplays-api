using GameplaysApi.Config;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GameplaysApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthConfig _authConfig;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICookieService _cookieService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        
        public AuthService(
            IOptions<AuthConfig> authConfig,
            IHttpContextAccessor contextAccessor,
            ICookieService cookieService,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService)
        {
            _authConfig = authConfig.Value;
            _contextAccessor = contextAccessor;
            _cookieService = cookieService;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public string GetCurrentUserId()
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = _contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return userId == null ? throw new InvalidOperationException("Invalid user.") : userId;
        }

        public void CreateAuthCookie(User user, HttpResponse response)
        {
            // Create JSON Web Token
            var payload = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _authConfig.ValidIssuers),
                new Claim(JwtRegisteredClaimNames.Aud, _authConfig.ValidAudiences)
            };

            if (int.TryParse(_authConfig.JwtExpMins, out int expirationMinutes))
            {
                var expirationMins = TimeSpan.FromMinutes(expirationMinutes);
                var jwt = _jwtTokenService.CreateToken(payload, expirationMins);

                var cookieValue = jwt;
                _cookieService.CreateCookie(response, _authConfig.JwtCookieName, cookieValue, expirationMins);
            }
        }

        public void DeleteAuthCookie(HttpResponse response)
        {
            _cookieService.DeleteCookie(response, _authConfig.JwtCookieName);
        }

        public async Task CreateRefreshTokenCookie(User user, HttpRequest request, HttpResponse response)
        {
            var expDays = TimeSpan.FromDays(int.Parse(_authConfig.RefreshExpDays));

            var tokenString = await _refreshTokenService.CreateRefreshToken(user, request, expDays);

            var cookieValue = tokenString;
            _cookieService.CreateCookie(response, _authConfig.RefreshCookieName, cookieValue, expDays);
        }

        public async Task UpdateRefreshTokenCookie(User user, HttpRequest request, HttpResponse response, RefreshToken refreshToken)
        {
            var expDays = TimeSpan.FromDays(int.Parse(_authConfig.RefreshExpDays));

            var tokenString = await _refreshTokenService.UpdateRefreshToken(user, request, expDays, refreshToken);

            var cookieValue = tokenString;
            _cookieService.CreateCookie(response, _authConfig.RefreshCookieName, cookieValue, expDays);
        }

        public void DeleteRefreshTokenCookie(HttpResponse response)
        {
            _cookieService.DeleteCookie(response, _authConfig.RefreshCookieName);
        }
    }
}
