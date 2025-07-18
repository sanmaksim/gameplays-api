using GameplaysApi.Interfaces;
using GameplaysApi.Models;
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

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId))
            {
                return Forbid();
            }

            if (!int.TryParse(jwtUserId, out int userId))
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }

            var user = await _usersRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var hashedRefreshToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

            var existingRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(userId, hashedRefreshToken);
            if (existingRefreshToken is null || existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized();
            }

            _authService.CreateAuthCookie(user, Response);
            await _authService.CreateRefreshTokenCookie(user, Request, Response);

            return Ok();
        }
    }
}
