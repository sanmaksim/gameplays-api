using GameplaysBackend.Models;
using GameplaysBackend.Data;
using GameplaysBackend.DTOs;
using GameplaysBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GameplaysBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public UsersController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // @desc Register user/set token
        // route POST /api/users/register
        // @access Public
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                User? existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerDto.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Username already exists." });
                }

                User? existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new { message = "Email already exists." });
                }

                User newUser = new User
                {
                    Username = registerDto.Username,
                    Password = registerDto.Password, // auto-hashed upon save in User model
                    Email = registerDto.Email
                };

                _context.Users.Add(newUser);

                _authService.CreateAuthCookie(newUser, Response);

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = newUser.UserId }, newUser);

            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        
        // @desc Auth user/create auth cookie
        // route GET /api/users/login
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

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == authDto.Username || u.Email == authDto.Email);

            if (user != null && authDto.Password != null)
            {
                if (user.VerifyPassword(authDto.Password, user.Password))
                {
                    _authService.CreateAuthCookie(user, Response);
                    return Ok(user);
                }
                else
                {
                    return Unauthorized("Invalid username or password.");
                }
            }
            else
            {
                return NotFound("User not found.");
            }
        }

        // @desc Delete auth cookie
        // route GET /api/users/logout
        // @access Private
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authService.DeleteAuthCookie(Response);
            return Ok( new { message = "Logged out successfully." });
        }

        // @desc Get all users
        // route GET /api/users
        // @access Private
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // @desc Get user
        // route GET /api/users/profile
        // @access Private
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUser()
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            if (int.TryParse(userId, out int id))
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            else
            {
                return BadRequest("The token string is not a valid integer.");
            }
        }

        // @desc Update user
        // route PUT /api/users/settings
        // @access Private
        [Authorize]
        [HttpPut("settings")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto userDto)
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (userId == null || userId != userDto.UserId.ToString())
            {
                return Forbid();
            }

            var existingUser = await _context.Users.FindAsync(userDto.UserId);

            if (existingUser == null)
            {
                return NotFound();
            }

            bool hasChanges = false;

            // Update only provided properties
            if (userDto.Username != null && userDto.Username != existingUser.Username)
            {
                existingUser.Username = userDto.Username;
                hasChanges = true;
            }
            if (userDto.Email != null && userDto.Email != existingUser.Email)
            {
                existingUser.Email = userDto.Email;
                hasChanges = true;
            }
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                // TODO: to check if the password has indeed changed?
                existingUser.Password = userDto.Password;
                hasChanges = true;
            }

            if (hasChanges)
            {
                existingUser.UpdateTimestamp();

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == userDto.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return NoContent();
        }

        // @desc Delete user
        // route DELETE /api/users/settings
        // @access Private
        [Authorize]
        [HttpDelete("settings")]
        public async Task<IActionResult> DeleteUser([FromBody] UserDto userDto)
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (userId == null || userId != userDto.UserId.ToString())
            {
                return Forbid();
            }

            var existingUser = await _context.Users.FindAsync(userDto.UserId);

            if (existingUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(existingUser);
            
            _authService.DeleteAuthCookie(Response);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
