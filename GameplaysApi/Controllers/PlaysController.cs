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

            // Parse the retrieved token ID string to an int
            if (int.TryParse(userId, out int uId))
            {
                // Check if the token ID matches the FromBody ID
                if (uId != playDto.UserId)
                {
                    return BadRequest(new { message = "The user is not authorized." });
                }

                // Make sure the user exists in the database
                var user = await _context.Users.FindAsync(playDto.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Make sure the game exists in the database
                var game = await _context.Games.FindAsync(playDto.GameId);
                if (game == null)
                {
                    return NotFound(new { message = "Game not found." });
                }

                var newPlay = new Play
                {
                    UserId = playDto.UserId,
                    GameId = playDto.GameId
                };

                // Get the count of plays for this user and game
                var playCount = await _context.Plays
                    .CountAsync(p => p.UserId == playDto.UserId && p.GameId == playDto.GameId);

                if (playCount > 0)
                {
                    var existingPlays = await _context.Plays
                        .Where(p => p.UserId == playDto.UserId && p.GameId == playDto.GameId)
                        .ToListAsync();

                    if (existingPlays != null)
                    {
                        foreach (var play in existingPlays)
                        {
                            // Only add additional play for Played and Playing statuses
                            if (play.Status == PlayStatus.Playing || play.Status == PlayStatus.Played 
                                && (PlayStatus)playDto.Status == PlayStatus.Playing || (PlayStatus)playDto.Status == PlayStatus.Played)
                            {
                                // Increment the run ID based on the count
                                newPlay.RunId = playCount > 0 ? playCount + 1 : 1;

                                // Set the play status
                                newPlay.Status = (PlayStatus)playDto.Status;
                            }
                            else if (play.Status == PlayStatus.Wishlist
                                && (PlayStatus)playDto.Status == PlayStatus.Wishlist)
                            {
                                return Ok(new { message = "Wishlist item already exists." });
                            }
                            else if (play.Status == PlayStatus.Backlog 
                                && (PlayStatus)playDto.Status == PlayStatus.Backlog)
                            {
                                return Ok(new { message = "Backlog item already exists." });
                            }
                            else
                            {
                                // Set the play status
                                newPlay.Status = (PlayStatus)playDto.Status;
                            }
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "Plays not found." });
                    }
                }
                else
                {
                    // Add a playthrough
                    newPlay.RunId = 1;

                    // Set the play status
                    newPlay.Status = (PlayStatus)playDto.Status;
                }

                _context.Plays.Add(newPlay);
                await _context.SaveChangesAsync();
                return CreatedAtAction(
                    nameof(GetPlay), 
                    new { playId = newPlay.Id }, 
                    new {
                        Message = "Play item created.",
                        Play = newPlay 
                    }
                );
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
                        .Include(p => p.Game)
                        .FirstOrDefaultAsync(p => p.UserId == uId && p.Id == pId);

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
