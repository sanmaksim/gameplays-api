using FluentValidation.Results;
using GameplaysBackend.Models;
using GameplaysBackend.Data;
using GameplaysBackend.DTOs;
using GameplaysBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    Password = registerDto.Password, // auto-hashed upon save in User class
                    Email = registerDto.Email
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _authService.CreateAuthCookie(newUser, Response);
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

        
        // @desc Auth user/set token
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
                return Unauthorized("Please enter your credentials.");
            }
        }

        // @desc Auth user/set token
        // route GET /api/users/auth
        // @access Public
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authService.DeleteAuthCookie(Response);
            return Ok( new { message = "Logged out successfully." });
        }

        // @desc Get all users
        // route GET /api/users
        // @access ???
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // @desc Get user
        // route GET /api/users/:id
        // route GET /api/users/profile ???
        // @access Private
        [Authorize]
        [HttpGet("{id}")]
        //[HttpGet("profile")] ???
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // @desc Update user
        // route PUT /api/users/:id
        // route PUT /api/users/profile ???
        // @access Private
        [Authorize]
        [HttpPut("{id}")]
        //[HttpPut("profile")] ???
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto updateDto)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Update only provided properties
            if (!string.IsNullOrEmpty(updateDto.Username))
                existingUser.Username = updateDto.Username;
            
            if (!string.IsNullOrEmpty(updateDto.Email))
                existingUser.Email = updateDto.Email;
            
            if (!string.IsNullOrEmpty(updateDto.Password))
                existingUser.Password = updateDto.Password;

            existingUser.UpdateTimestamp();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.UserId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // @desc Delete user
        // route DELETE /api/users/:id
        // @access Protected
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(existingUser);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
