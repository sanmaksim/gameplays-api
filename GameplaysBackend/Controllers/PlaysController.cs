using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysBackend.Filters;
using GameplaysBackend.Models;
using GameplaysBackend.Data;

namespace GameplaysBackend.Controllers
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
                .Include(p => p.User)
                .Include(p => p.Game)
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
                .Include(p => p.User)
                .Include(p => p.Game)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PlayId == playId);

            if (play == null)
            {
                return NotFound();
            }

            return Ok(play);
        }
        
        [HttpPut("{playId}")]
        public async Task<IActionResult> UpdatePlay(int userId, int playId, Play play)
        {
            var existingPlay = await _context.Plays
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PlayId == playId);
            
            if (existingPlay == null)
            {
                return NotFound(new { message = "Play not found." });
            }

            if (play.GameId != 0 && existingPlay.GameId != play.GameId)
            {
                // Verify the new game exists
                var newGameExists = await _context.Games.AnyAsync(g => g.GameId == play.GameId);
                if (!newGameExists)
                {
                    return NotFound(new { message = "Game not found." });
                }
                existingPlay.GameId = play.GameId;
            }

            if (play.Status != PlayStatus.Unplayed) // Unplayed is the default
            {
                existingPlay.Status = play.Status;
            }

            if (play.PercentageCompleted >= 0 && play.PercentageCompleted <= 100)
            {
                existingPlay.PercentageCompleted = play.PercentageCompleted;
            }
            else if (play.PercentageCompleted != 0)
            {
                return BadRequest(new { message = "PercentageCompleted must be between 0 and 100." });
            }

            if (play.HoursPlayed >= 0)
            {
                existingPlay.HoursPlayed = play.HoursPlayed;
            }
            else if (play.HoursPlayed != 0)
            {
                return BadRequest(new { message = "HoursPlayed must be non-negative." });
            }

            if (play.LastPlayedAt.HasValue)
            {
                if (play.LastPlayedAt.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    existingPlay.LastPlayedAt = play.LastPlayedAt;
                }
                else
                {
                    return BadRequest(new { message = "LastPlayedAt cannot be in the future." });
                }
            }

            existingPlay.UpdateTimestamp();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if (!_context.Plays.Any(p => p.PlayId == playId))
                {
                    return NotFound(new { message = "Play not found." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{playId}")]
        public async Task<IActionResult> DeletePlay(int userId, int playId)
        {
            var existingPlay = await _context.Plays
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PlayId == playId);
            
            if (existingPlay == null)
            {
                return NotFound(new { message = "Play not found for this user." });
            }

            _context.Plays.Remove(existingPlay);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
