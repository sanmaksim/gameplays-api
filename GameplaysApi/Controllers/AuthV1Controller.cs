using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthV1Controller : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUsersRepository _usersRepository;

        public AuthV1Controller(
            IAuthService authService,
            IRefreshTokenService refreshTokenService,
            IUsersRepository usersRepository)
        {
            _authService = authService;
            _refreshTokenService = refreshTokenService;
            _usersRepository = usersRepository;
        }
        
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Authenticates user",
            Description = "Creates user access & refresh tokens.",
            OperationId = "Login"
        )]
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

        [Authorize]
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Logs the user out",
            Description = "Deletes user access & refresh tokens, " +
            "removes associated user refresh token entry from the database.",
            OperationId = "Logout"
        )]
        public async Task<IActionResult> Logout()
        {
            Request.Cookies.TryGetValue("refreshToken", out string? tokenString);
            if (tokenString == null)
            {
                return Unauthorized();
            }

            RefreshToken? refreshToken = await _refreshTokenService.GetRefreshToken(tokenString);

            if (refreshToken is null
                || refreshToken.User is null
                || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized();
            }

            _authService.DeleteAuthCookie(Response);
            _authService.DeleteRefreshTokenCookie(Response);
            await _refreshTokenService.DeleteRefreshToken(refreshToken);
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("refresh")]
        [SwaggerOperation(
            Summary = "Regenerates the user refresh token",
            Description = "Overwrites the existing user refresh token" +
            "and associated user refresh token entry in the database.",
            OperationId = "Refresh"
        )]
        public async Task<IActionResult> Refresh()
        {
            Request.Cookies.TryGetValue("refreshToken", out string? tokenString);
            
            if (tokenString == null)
            {
                return Unauthorized();
            }

            RefreshToken? refreshToken = await _refreshTokenService.GetRefreshToken(tokenString);

            if (refreshToken is null
                || refreshToken.User is null
                || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized();
            }

            _authService.CreateAuthCookie(refreshToken.User, Response);
            await _authService.UpdateRefreshTokenCookie(refreshToken.User, Request, Response, refreshToken);

            return Ok(new
            {
                id = refreshToken.User.Id,
                username = refreshToken.User.Username,
                email = refreshToken.User.Email
            });
        }
    }
}
