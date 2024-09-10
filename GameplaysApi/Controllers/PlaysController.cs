using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysApi.Filters;
using GameplaysApi.Models;
using GameplaysApi.Data;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    [ValidateUserExists]
    public class PlaysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlaysController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlay(int userId, Play play)
        {
            if (userId != play.UserId)
            {
                return BadRequest(new { message = "UserId in the route does not match the UserId in the play object." });
            }
            
            // Check if the Game exists
            var gameExists = await _context.Games.AnyAsync(g => g.GameId == play.GameId);
            if (!gameExists)
            {
                return BadRequest(new { message = "The specified Game does not exist." });
            }

            // Get the count of plays for this user and game
            var playCount = await _context.Plays
                .CountAsync(p => p.UserId == userId && p.GameId == play.GameId);

            // Set RunId based on the count
            play.RunId = playCount > 0 ? playCount + 1 : 1;

            _context.Plays.Add(play);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPlay), new { userId = play.UserId, playId = play.PlayId }, play);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllPlays(int userId)
        {
            var plays = await _context.Plays
                .Where(p => p.UserId == userId)
                .ToListAsync();
            
            if (plays == null || !plays.Any())
            {
                return NotFound(new { message = "No plays found for this user." });
            }

            return Ok(plays);
        }
        
        [HttpGet("{playId}")]
        public async Task<IActionResult> GetPlay(int userId, int playId)
        {
            var play = await _context.Plays
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PlayId == playId);

            if (play == null)
            {
                return NotFound();
            }

            return Ok(play);
        }
        
    }
}
