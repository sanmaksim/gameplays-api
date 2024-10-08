using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysApi.Data;
using GameplaysApi.DTOs;
using GameplaysApi.Models;

namespace GameplaysApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            //User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.Password == loginDto.Password);
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username || u.Email == loginDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid username or email.");
            }
            else
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized("Invalid password.");
                }
            }

            return Ok(user);
        }
    }
}
