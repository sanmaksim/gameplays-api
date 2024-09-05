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
    }
}
