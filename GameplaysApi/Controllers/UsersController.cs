using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;
using GameplaysApi.Data;
using GameplaysApi.DTOs;

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
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateUser), new { id = user.UserId }, user);
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
    }
}
