using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysApi.Models;
using GameplaysApi.Data;
using GameplaysApi.DTOs;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateGame), new { id = game.GameId }, game);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _context.Games.ToListAsync();
            return Ok(games);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            return Ok(game);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(int id, Game game)
        {
            var existingGame = await _context.Games.FindAsync(id);
            if (existingGame == null)
            {
                return NotFound();
            }

            // Update only provided properties
            if (!string.IsNullOrEmpty(game.Title))
                existingGame.Title = game.Title;
            
            if (!string.IsNullOrEmpty(game.Genre))
                existingGame.Genre = game.Genre;
            
            if (game.ReleaseDate != default(DateOnly))
                existingGame.ReleaseDate = game.ReleaseDate;

            if (!string.IsNullOrEmpty(game.Developer))
                existingGame.Developer = game.Developer;

            existingGame.UpdateTimestamp();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Games.Any(e => e.GameId == id))
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
        public async Task<IActionResult> DeleteGame(int id)
        {
            var existingGame = await _context.Games.FindAsync(id);
            if (existingGame == null)
            {
                return NotFound();
            }

            _context.Games.Remove(existingGame);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
