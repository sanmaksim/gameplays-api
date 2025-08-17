using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using GameplaysApi.Validators;
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
                    nameof(GetPlays),
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
        
        [Authorize]
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get filtered plays",
            Description = "Retrieve plays for a user filtered by status",
            OperationId = "GetPlays"
        )]
        public async Task<IActionResult> GetPlays([FromQuery] PlayDto playDto)
        {
            // Validate incoming query parameters
            var validator = new PlayDtoValidator();
            var result = validator.Validate(playDto);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            // Verify user access
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId))
            {
                return Forbid();
            }
            else if (playDto.UserId.ToString() != jwtUserId)
            {
                return Unauthorized();
            }

            // Return all plays for a user filtered by status
            if (playDto.UserId != null && playDto.StatusId != null)
            {
                var plays = await _playsRepository.GetPlaysAsync((int)playDto.UserId, null, (int)playDto.StatusId);
                if (plays == null || plays.Count == 0)
                {
                    return Ok(new { message = "No games shelved." });
                }

                return Ok(plays);
            }

            // Return only the user single play ID and status for a certain Giant Bomb game ID
            if (playDto.UserId != null && playDto.ApiGameId != null)
            {
                var plays = await _playsRepository.GetPlaysAsync((int)playDto.UserId, (int)playDto.ApiGameId, null);
                if (plays == null || plays.Count == 0)
                {
                    return Ok(new { message = "Game not shelved." });
                }

                var play = plays.Select(p => new {
                        PlayId = p.Id,
                        Status = p.Status!
                    })
                    .Single();
                
                return Ok(play);
            }

            return BadRequest(new { message = "One or more missing query parameters." });
        }
    }
}
