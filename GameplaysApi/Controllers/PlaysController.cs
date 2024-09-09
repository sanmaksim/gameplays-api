using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;
using GameplaysApi.Data;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlaysController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlay(Play play)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the User exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == play.UserId);
            if (!userExists)
            {
                return BadRequest(new { message = "The specified User does not exist." });
            }

            // Check if the Game exists
            var gameExists = await _context.Games.AnyAsync(g => g.GameId == play.GameId);
            if (!gameExists)
            {
                return BadRequest(new { message = "The specified Game does not exist." });
            }

            var latestPlay = await _context.Plays
                .Where(p => p.UserId == play.UserId && p.GameId == play.GameId)
                .OrderByDescending(p => p.PlayId)
                .FirstOrDefaultAsync();

            if (latestPlay != null)
            {
                play.RunId = latestPlay.RunId + 1;
            }

            _context.Plays.Add(play);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPlay), new { id = play.PlayId }, play);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllPlays()
        {
            var plays = await _context.Plays.ToListAsync();
            return Ok(plays);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlay(int id)
        {
            var play = await _context.Plays.FindAsync(id);

            if (play == null)
            {
                return NotFound();
            }

            return Ok(play);
        }
        
    }
}
