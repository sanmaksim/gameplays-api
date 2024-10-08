using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;
using GameplaysApi.Data;
using GameplaysApi.DTOs;
using FluentValidation.Results;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
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

                // pwd will not be null here after form validation
                string password = registerDto.Password;
                User newUser = new User
                {
                    Username = registerDto.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Email = registerDto.Email
                };

                _context.Users.Add(newUser);
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

        
        // @desc Auth user/set token
        // route GET /api/users/auth
        // @access Public
        [HttpPost("auth")]
        public async Task<IActionResult> Login([FromBody] AuthDto authDto)
        {
            AuthDtoValidator validator = new AuthDtoValidator();
            ValidationResult result = validator.Validate(authDto);

            if (!result.IsValid)
            {
                foreach (ValidationFailure error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == authDto.Username || u.Email == authDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid login.");
            }
            else
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(authDto.Password, user.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized("Invalid password.");
                }
            }

            return Ok(user);
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
        // @access ???
        [HttpGet("{id}")]
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
        // route GET /api/users/:id
        // @access Protected
        [HttpPut("{id}")]
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
