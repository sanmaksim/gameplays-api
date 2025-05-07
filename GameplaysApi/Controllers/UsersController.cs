using GameplaysApi.Models;
using GameplaysApi.Data;
using GameplaysApi.DTOs;
using GameplaysApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace GameplaysApi.Controllers
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
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerDto.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Username already exists." });
                }

                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new { message = "Email already exists." });
                }

                var newUser = new User
                {
                    Username = registerDto.Username,
                    Password = registerDto.Password, // auto-hashed upon save in User model
                    Email = registerDto.Email
                };

                _context.Users.Add(newUser);

                // Save the new user first to generate the UserId
                await _context.SaveChangesAsync();

                // Now that the UserId is set, create the cookie
                _authService.CreateAuthCookie(newUser, Response);

                return CreatedAtAction(nameof(GetUser), 
                                        new { id = newUser.Id }, 
                                        new {
                                            id = newUser.Id,
                                            username = newUser.Username,
                                            email = newUser.Email
                                        });

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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == authDto.Username || u.Email == authDto.Email);

            if (user != null && authDto.Password != null)
            {
                if (user.VerifyPassword(authDto.Password, user.Password))
                {
                    _authService.CreateAuthCookie(user, Response);
                    return Ok(new { 
                        id = user.Id,
                        username = user.Username,
                        email = user.Email
                    });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }
            }
            else
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }
        }

        // @desc Delete auth cookie
        // route POST /api/users/logout
        // @access Private
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authService.DeleteAuthCookie(Response);
            return Ok( new { message = "Logged out successfully." });
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
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }

        // @desc Update user
        // route PUT /api/users/profile
        // @access Private
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto userDto)
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            if (int.TryParse(userId, out int id))
            {
                var existingUser = await _context.Users.FindAsync(id);

                if (existingUser == null)
                {
                    return NotFound();
                }
                if (id != userDto.UserId)
                {
                    return Forbid();
                }

                bool hasChanges = false;

                // Update only provided properties
                if (userDto.Username != null && userDto.Username != existingUser.Username)
                {
                    bool usernameExists = await _context.Users.AnyAsync(u => u.Username == userDto.Username);

                    if (!usernameExists)
                    {
                        existingUser.Username = userDto.Username;
                        hasChanges = true;
                    }
                    else
                    {
                        return BadRequest(new { message = "The username already exists." });
                    }
                }
                if (userDto.Email != null && userDto.Email != existingUser.Email)
                {
                    bool emailExists = await _context.Users.AnyAsync(u => u.Email == userDto.Email);

                    if (!emailExists)
                    {
                        existingUser.Email = userDto.Email;
                        hasChanges = true;
                    }
                    else
                    {
                        return BadRequest(new { message = "The email is already in use."});
                    }
                }
                if (!string.IsNullOrEmpty(userDto.Password))
                {
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
                        if (!_context.Users.Any(e => e.Id == id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                return Ok(new { 
                    id = existingUser.Id,
                    username = existingUser.Username,
                    email = existingUser.Email
                });
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }

        // @desc Delete user
        // route DELETE /api/users/profile
        // @access Private
        [Authorize]
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteUser()
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            if (int.TryParse(userId, out int id))
            {
                var existingUser = await _context.Users.FindAsync(id);

                if (existingUser == null)
                {
                    return NotFound(new { message = "User not found." });
                }
                
                _context.Users.Remove(existingUser);
                
                _authService.DeleteAuthCookie(Response);

                await _context.SaveChangesAsync();

                return Ok(new { message = "User deleted." });
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }
    }
}
