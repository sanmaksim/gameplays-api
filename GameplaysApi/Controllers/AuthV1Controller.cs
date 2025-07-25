﻿using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // [Authorize] omitted since no auth middleware
        // configured for refresh token cookie
        [HttpPost("refresh")]
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
