using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaysController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGamesRepository _gamesRepository;
        private readonly IPlaysRepository _playsRepository;
        private readonly IUsersRepository _usersRepository;

        public PlaysController(
            IAuthService authService,
            IGamesRepository gamesRepository,
            IPlaysRepository playsRepository,
            IUsersRepository usersRepository)
        {
            _authService = authService;
            _gamesRepository = gamesRepository;
            _playsRepository = playsRepository;
            _usersRepository = usersRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPlay([FromBody] AddPlayDto playDto)
        {
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || playDto.UserId != jwtUserId)
            {
                return Forbid();
            }

            if (!int.TryParse(playDto.UserId, out int uId))
            {
                return BadRequest(new { message = "The user ID is invalid." });
            }

            var user = await _usersRepository.GetUserByIdAsync(uId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!int.TryParse(playDto.GameId, out int apiGameId))
            {
                return BadRequest(new { message = "The game ID is invalid." });
            }

            var game = await _gamesRepository.GetGameByExternalIdAsync(apiGameId);
            if (game == null)
            {
                return NotFound(new { message = "Game not found." });
            }

            var play = await _playsRepository.GetPlayByUserIdAndInternalGameIdAsync(uId, game.Id);
            if (play != null)
            {
                await _playsRepository.UpdatePlayStatusAsync(play, playDto.Status);
                return Ok(new { Message = $"Moved to {Enum.GetName(typeof(PlayStatus), playDto.Status)}." });
            }
            else
            {
                var newPlay = new Play
                {
                    UserId = uId,
                    GameId = game.Id, // this refers to our local database's game ID
                    ApiGameId = apiGameId, // this refers to the Giant Bomb API's game ID
                    RunId = 1,
                    Status = (PlayStatus)playDto.Status
                };

                await _playsRepository.AddPlayAsync(newPlay);
                return CreatedAtAction(
                    nameof(GetPlayByUserAndExternalGameId),
                    new
                    {
                        userId = newPlay.UserId,
                        apiGameId = newPlay.ApiGameId
                    },
                    new { Message = $"Added to {Enum.GetName(typeof(PlayStatus), playDto.Status)}." }
                );
            }
        }
        // TODO: refactor endpoint to filter based
        [Authorize]
        [HttpDelete("user/{userId}/play/{playId}")]
        public async Task<IActionResult> DeletePlay(string userId, string playId)
        {
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId))
            {
                return Forbid();
            }

            if (!int.TryParse(userId, out int uId))
            {
                return BadRequest(new { message = "The user ID is invalid." });
            }


            var user = await _usersRepository.GetUserByIdAsync(uId);
            if (user == null)
            {
                return NotFound();
            }

            if (!int.TryParse(playId, out int pId))
            {
                return BadRequest(new { message = "The play ID is invalid." });
            }

            var play = await _playsRepository.GetPlayByIdAsync(pId);
            if (play == null)
            {
                return NotFound(new { message = "Game not shelved." });
            }

            await _playsRepository.RemovePlayAsync(play);

            return Ok(new { message = $"Removed from {Enum.GetName(typeof(PlayStatus), play.Status)}." });
        }
        // TODO: refactor endpoint to filter based
        [Authorize]
        [HttpGet("user/{userId}/game/{apiGameId}")]
        public async Task<IActionResult> GetPlayByUserAndExternalGameId(string userId, string apiGameId)
        {
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || userId != jwtUserId)
            {
                return Forbid();
            }

            if (!int.TryParse(userId, out int uId))
            {
                return BadRequest(new { message = "The user ID is invalid." });
            }

            var user = await _usersRepository.GetUserByIdAsync(uId);
            if (user == null)
            {
                return NotFound();
            }

            if (!int.TryParse(apiGameId, out int extGameId))
            {
                return BadRequest(new { message = "The game ID is invalid." });
            }

            var play = await _playsRepository.GetPlayByUserIdAndExternalGameIdAsync(uId, extGameId);
            if (play == null)
            {
                return Ok(new { message = "Game not shelved." });
            }

            return Ok(play);
        }

        [Authorize]
        [HttpGet("user/{id}")]
        [SwaggerOperation(
            Summary = "Get all plays by user ID",
            Description = "Retrieve all plays for a user based on their ID",
            OperationId = "GetPlaysByUserId"
        )]
        public async Task<IActionResult> GetPlaysByUserId(int id)
        {
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || id.ToString() != jwtUserId)
            {
                return Forbid();
            }

            var user = await _usersRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var plays = await _playsRepository.GetPlaysByUserIdAsync(id);
            if (plays == null || !plays.Any())
            {
                return Ok(new { message = "No games shelved." });
            }

            return Ok(plays);
        }
    }
}
