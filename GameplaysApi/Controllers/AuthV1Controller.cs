using GameplaysApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthV1Controller : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUsersRepository _usersRepository;

        public AuthV1Controller(
            IAuthService authService,
            IRefreshTokenRepository refreshTokenRepository,
            IUsersRepository usersRepository)
        {
            _authService = authService;
            _refreshTokenRepository = refreshTokenRepository;
            _usersRepository = usersRepository;
        }

        // [Authorize] omitted since no auth middleware
        // configured for refresh token cookie
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
            
            if (refreshToken == null)
            {
                return Unauthorized();
            }

            var hashedRefreshToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
            var existingRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(hashedRefreshToken);
            
            if (existingRefreshToken is null
                || existingRefreshToken.User is null
                || existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized();
            }

            _authService.CreateAuthCookie(existingRefreshToken.User, Response);
            await _authService.CreateRefreshTokenCookie(existingRefreshToken.User, Request, Response);

            return Ok(new
            {
                id = existingRefreshToken.User.Id,
                username = existingRefreshToken.User.Username,
                email = existingRefreshToken.User.Email
            });
        }
    }
}
