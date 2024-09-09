using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;
using GameplaysApi.Data;
using GameplaysApi.DTOs;
using MySqlConnector;

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

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mySqlEx && mySqlEx.Number == 1062)
                {
                    return Conflict(new { message = "A user with this email already exists." });
                }
                
                // For other database-related errors, return a generic error message
                return StatusCode(500, new { message = "An error occurred while creating the user." });
            }
            catch (Exception)
            {
                // For any other unexpected errors, return a generic error message
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

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
