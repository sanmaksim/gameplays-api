using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
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

        // [Authorize] omitted to allow through expired jwt for refresh
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
