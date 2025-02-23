using GameplaysApi.Data;
using GameplaysApi.DTOs;
using GameplaysApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPlay([FromBody] AddPlayDto playDto)
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            if (int.TryParse(userId, out int uId))
            {
                if (uId != playDto.UserId)
                {
                    return BadRequest(new { message = "The user is not authorized." });
                }

                var user = await _context.Users.FindAsync(playDto.UserId);

                if (user == null)
                {
                    return NotFound();
                }

                var newPlay = new Play
                {
                    UserId = playDto.UserId,
                    GameId = playDto.GameId
                };

                // Get the count of plays for this user and game
                var playCount = await _context.Plays
                    .CountAsync(p => p.UserId == playDto.UserId && p.GameId == playDto.GameId);

                // Set RunId based on the count
                newPlay.RunId = playCount > 0 ? playCount + 1 : 1;

                // Set the play status
                newPlay.Status = (PlayStatus)playDto.Status;

                _context.Plays.Add(newPlay);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetPlay), new { playId = newPlay.PlayId }, newPlay);
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }
        
        [Authorize]
        [HttpGet("{playId}")]
        public async Task<IActionResult> GetPlay(string playId)
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            if (int.TryParse(userId, out int uId))
            {
                var user = await _context.Users.FindAsync(uId);

                if (user == null)
                {
                    return NotFound();
                }

                if (int.TryParse(playId, out int pId))
                {
                    var play = await _context.Plays
                        .Include(p => p.User)
                        .FirstOrDefaultAsync(p => p.UserId == uId && p.PlayId == pId);

                    return Ok(play);
                }
                else
                {
                    return NotFound(new { message = "Play not found." });
                }
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }
    }
}
