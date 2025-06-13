using GameplaysApi.Data;
using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
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
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly IPlaysRepository _playsRepository;

        public PlaysController(
            IAuthService authService,
            ApplicationDbContext context,
            IPlaysRepository playsRepository)
        {
            _authService = authService;
            _context = context;
            _playsRepository = playsRepository;
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
                                return Ok(new { message = $"Already {Enum.GetName(typeof(PlayStatus), play.Status)}." });
                            }
                            else if (play.Status == PlayStatus.Played
                                && (PlayStatus)playDto.Status == PlayStatus.Played)
                            {
                                return Ok(new { message = $"Already {Enum.GetName(typeof(PlayStatus), play.Status)}." });
                            }
                            else if (play.Status == PlayStatus.Wishlist
                                && (PlayStatus)playDto.Status == PlayStatus.Wishlist)
                            {
                                return Ok(new { message = $"Already {Enum.GetName(typeof(PlayStatus), play.Status)}ed." });
                            }
                            else if (play.Status == PlayStatus.Backlog
                                && (PlayStatus)playDto.Status == PlayStatus.Backlog)
                            {
                                return Ok(new { message = $"Already {Enum.GetName(typeof(PlayStatus), play.Status)}ged." });
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
                    new { Message = $"Added to {Enum.GetName(typeof(PlayStatus), playDto.Status)}." }
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
                        return NotFound(new { message = "Game not shelved." });
                    }

                    _context.Plays.Remove(existingPlay);

                    await _context.SaveChangesAsync();

                    return Ok(new { message = $"Removed from {Enum.GetName(typeof(PlayStatus), existingPlay.Status)}." });
                }
                else
                {
                    return BadRequest(new { message = "The playId is not valid." });
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
                    var play = await _context.Plays.FirstOrDefaultAsync(p => p.UserId == uId && p.ApiGameId == gId);

                    if (play != null)
                    {
                        return Ok(new
                        {
                            PlayId = play.Id,
                            Status = (int)play.Status
                        });
                    }
                    else
                    {
                        return Ok(new { message = "Game not shelved." });
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
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || userId != jwtUserId)
            {
                return Forbid();
            }

            int uId;
            if (!int.TryParse(userId, out uId))
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }

            var user = await _playsRepository.GetUserByIdAsync(uId);
            if (user == null)
            {
                return NotFound();
            }
            
            var plays = await _playsRepository.GetPlaysByUserIdAsync(uId);
            if (plays == null || !plays.Any())
            {
                return Ok(new { message = "No games shelved." });
            }

            return Ok(plays);
        }
    }
}
