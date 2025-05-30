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
            if (int.TryParse(userId, out int uId) && int.TryParse(playDto.GameId, out int gId))
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
                var game = await _context.Games.FirstOrDefaultAsync(g => g.GameId == gId);
                if (game == null)
                {
                    return NotFound(new { message = "Game not found." });
                }

                var newPlay = new Play
                {
                    UserId = playDto.UserId,
                    GameId = game.Id, // local game ID, not the Giant Bomb API game ID
                    ApiGameId = gId // this is the Giant Bomb API game ID
                };

                // Get the count of plays for this user and game
                var playCount = await _context.Plays
                    .CountAsync(p => p.UserId == playDto.UserId && p.GameId == game.Id);

                if (playCount > 0)
                {
                    var existingPlays = await _context.Plays
                        .Where(p => p.UserId == playDto.UserId && p.GameId == game.Id)
                        .ToListAsync();

                    if (existingPlays != null)
                    {
                        foreach (var play in existingPlays)
                        {
                            if (play.Status == PlayStatus.Playing
                                && (PlayStatus)playDto.Status == PlayStatus.Playing)
                            {
                                return Ok(new { message = "Item already shelved as playing." });
                            }
                            else if (play.Status == PlayStatus.Played
                                && (PlayStatus)playDto.Status == PlayStatus.Played)
                            {
                                return Ok(new { message = "Item already shelved as played." });
                            }
                            else if (play.Status == PlayStatus.Wishlist
                                && (PlayStatus)playDto.Status == PlayStatus.Wishlist)
                            {
                                return Ok(new { message = "Item already wishlisted." });
                            }
                            else if (play.Status == PlayStatus.Backlog
                                && (PlayStatus)playDto.Status == PlayStatus.Backlog)
                            {
                                return Ok(new { message = "Item already backlogged." });
                            }
                            else
                            {
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
                    nameof(GetPlaysByGameId), 
                    new { GameId = gId }, 
                    new { Message = "Play item created." }
                );
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }

        [Authorize]
        [HttpDelete("{playId}")]
        public async Task<IActionResult> DeletePlay(string playId)
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
                    var existingPlay = await _context.Plays.FindAsync(pId);

                    if (existingPlay == null)
                    {
                        return NotFound(new { message = "Play not found." });
                    }

                    _context.Plays.Remove(existingPlay);

                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Play deleted." });
                }
                else
                {
                    return BadRequest(new { message = "The play ID is not valid." });
                }
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }
        
        [Authorize]
        [HttpGet("game/{gameId}")]
        public async Task<IActionResult> GetPlaysByGameId(string gameId)
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

                if (int.TryParse(gameId, out int gId))
                {
                    var plays = await _context.Plays
                        .Where(p => p.UserId == uId && p.ApiGameId == gId)
                        .ToListAsync();

                    if (plays != null)
                    {
                        var statusList = new List<PlayStatusDto>();
                        foreach (Play play in plays)
                        {
                            statusList.Add(new PlayStatusDto { 
                                PlayId = play.Id, 
                                Status = (int)play.Status 
                            });
                        }
                        return Ok(statusList);
                    }
                    else
                    {
                        return Ok(new { message = "No plays found." });
                    }
                }
                else
                {
                    return NotFound(new { message = "The game ID is not valid." });
                }
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPlaysByUserId(string userId)
        {
            // Retrieve the user ID string from the JWT 'sub' claim
            var jwtUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(jwtUserId) || userId != jwtUserId)
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
                
                var plays = await _context.Plays
                    .Include(p => p.Game)
                    .Where(p => p.UserId == uId)
                    .Select(p => new UserPlaysDto
                    {
                        Id = p.Id,
                        Name = p.Game!.Name,
                        Developers = p.Game.Developers!.Select(
                            dev => new DevDto
                            {
                                DevId = dev.Id,
                                Name = dev.Name
                            }).ToList(),
                        OriginalReleaseDate = p.Game.OriginalReleaseDate,
                        CreatedAt = DateOnly.FromDateTime(p.CreatedAt),
                        HoursPlayed = p.HoursPlayed,
                        PercentageCompleted = p.PercentageCompleted,
                        LastPlayedAt = p.LastPlayedAt,
                        Status = (int)p.Status,
                        ApiGameId = p.ApiGameId
                    })
                    .ToListAsync();

                if (plays != null)
                {
                    return Ok(plays);
                }
                else
                {
                    return Ok(new { message = "No plays found." });
                }
            }
            else
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }
        }
    }
}
