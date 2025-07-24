using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.AspNetCore.Authorization;
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
        
        // @desc Auth user/create auth & refresh cookies
        // route GET /api/v1/auth/login
        // @access Public
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDto authDto)
        {
            var validator = new AuthDtoValidator();
            var result = validator.Validate(authDto);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            User? user;
            if (authDto.Username != null)
            {
                user = await _usersRepository.GetUserByNameAsync(authDto.Username);
            }
            else if (authDto.Email != null)
            {
                user = await _usersRepository.GetUserByEmailAsync(authDto.Email);
            }
            else
            {
                return BadRequest(new { message = "Please enter a username or email." });
            }

            if (user == null || authDto.Password != null && !user.VerifyPassword(authDto.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid username or password." });

            }

            _authService.CreateAuthCookie(user, Response);
            await _authService.CreateRefreshTokenCookie(user, Request, Response);

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            });
        }

        // @desc Delete auth cookie
        // route POST /api/users/logout
        // @access Private
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authService.DeleteAuthCookie(Response);
            return Ok(new { message = "Logged out successfully." });
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
