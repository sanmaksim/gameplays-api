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

        public PlaysController(
            IAuthService authService,
            IGamesRepository gamesRepository,
            IPlaysRepository playsRepository,
            IUsersRepository usersRepository)
        {
            _authService = authService;
            _gamesRepository = gamesRepository;
            _playsRepository = playsRepository;
        }

        [Authorize]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create play",
            Description = "Adds a play for a user ID based on a Giant Bomb game ID, " +
            "or updates an existing play when also provided a status ID",
            OperationId = "CreatePlay"
        )]
        public async Task<IActionResult> CreatePlay([FromQuery] PlayRequestDto playRequestDto)
        {
            // Validate incoming query parameters
            var validator = new PlayRequestDtoValidator();
            var result = validator.Validate(playRequestDto);
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
            else if (playRequestDto.UserId.ToString() != jwtUserId)
            {
                return Unauthorized();
            }

            // Add play for the current user
            if (playRequestDto.UserId != null
                && playRequestDto.ApiGameId != null
                && playRequestDto.StatusId != null)
            {
                var game = await _gamesRepository.GetGameByExternalIdAsync((int)playRequestDto.ApiGameId);
                if (game == null)
                {
                    return NotFound(new { message = "Game not found." });
                }

                var newPlay = new Play
                {
                    UserId = (int)playRequestDto.UserId,
                    GameId = game.Id, // this refers to our local database's game ID
                    ApiGameId = (int)playRequestDto.ApiGameId, // this refers to the Giant Bomb API's game ID
                    RunId = 1,
                    Status = (PlayStatus)playRequestDto.StatusId
                };

                await _playsRepository.AddPlayAsync(newPlay);
                return CreatedAtAction(
                    nameof(GetPlays),
                    new
                    {
                        userId = newPlay.UserId,
                        apiGameId = newPlay.ApiGameId
                    },
                    new { Message = $"Added to {Enum.GetName(typeof(PlayStatus), playRequestDto.StatusId)}." }
                );
            }

            return BadRequest(new { message = "One or more missing query parameters." });
        }

        [Authorize]
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get filtered plays",
            Description = "Retrieves plays for a user ID filtered by " +
            "Giant Bomb Game ID or status ID",
            OperationId = "GetPlays"
        )]
        public async Task<IActionResult> GetPlays([FromQuery] PlayRequestDto playDto)
        {
            // Validate incoming query parameters
            var validator = new PlayRequestDtoValidator();
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
                var plays = await _playsRepository.GetPlaysByUserAndOptionalIdAsync((int)playDto.UserId, null, (int)playDto.StatusId);
                if (plays == null || plays.Count == 0)
                {
                    return Ok(new { message = "No games shelved." });
                }

                return Ok(plays);
            }

            // Return the user's play ID and status for a certain Giant Bomb game ID
            if (playDto.UserId != null && playDto.ApiGameId != null)
            {
                var plays = await _playsRepository.GetPlaysByUserAndOptionalIdAsync((int)playDto.UserId, (int)playDto.ApiGameId, null);
                if (plays == null || plays.Count == 0)
                {
                    return Ok(new { message = "Game not shelved." });
                }
                else if (plays.Count == 1)
                {
                    // For now, there is only support for a single play per user per game
                    // This will be updated in future to support multiple playthroughs per user per game
                    var playStatus = plays.Select(p => new {
                        PlayId = p.Id,
                        Status = p.Status!
                    })
                    .Single();
                    return Ok(playStatus);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    { 
                        message = "Unexpected number of plays."
                    });
                }                
            }

            return BadRequest(new { message = "One or more missing query parameters." });
        }

        [Authorize]
        [HttpPut]
        [SwaggerOperation(
            Summary = "Update play",
            Description = "Updates a play status provided a user ID, " +
            "Giant Bomb game ID, and status ID",
            OperationId = "UpdatePlay"
        )]
        public async Task<IActionResult> UpdatePlay([FromQuery] PlayRequestDto playRequestDto)
        {
            // Validate incoming query parameters
            var validator = new PlayRequestDtoValidator();
            var result = validator.Validate(playRequestDto);
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
            else if (playRequestDto.UserId.ToString() != jwtUserId)
            {
                return Unauthorized();
            }

            // Update play
            if (playRequestDto.UserId != null
                && playRequestDto.ApiGameId != null
                && playRequestDto.StatusId != null)
            {
                // If play exists then update status
                var play = await _playsRepository.GetPlayByUserAndApiGameIdAsync((int)playRequestDto.UserId, (int)playRequestDto.ApiGameId);
                if (play != null)
                {
                    await _playsRepository.UpdatePlayStatusAsync(play, (int)playRequestDto.StatusId);
                    return Ok(new { Message = $"Moved to {Enum.GetName(typeof(PlayStatus), playRequestDto.StatusId)}." });
                }
                else
                {
                    return NotFound(new { message = "Play not found." });
                }
            }

            return BadRequest(new { message = "One or more missing query parameters." });
        }

        [Authorize]
        [HttpDelete]
        [SwaggerOperation(
            Summary = "Delete play",
            Description = "Removes a play ID for a user ID",
            OperationId = "DeletePlay"
        )]
        public async Task<IActionResult> DeletePlay([FromQuery] PlayRequestDto playRequestDto)
        {
            // Validate incoming query parameters
            var validator = new PlayRequestDtoValidator();
            var result = validator.Validate(playRequestDto);
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
            else if (playRequestDto.UserId.ToString() != jwtUserId)
            {
                return Unauthorized();
            }

            // Remove play for the current user
            if (playRequestDto.UserId != null && playRequestDto.PlayId != null)
            {
                var play = await _playsRepository.GetPlayAsync((int)playRequestDto.PlayId);
                if (play == null)
                {
                    return NotFound(new { message = "Game not shelved." });
                }

                await _playsRepository.RemovePlayAsync(play);

                return Ok(new { message = $"Removed from {Enum.GetName(typeof(PlayStatus), play.Status)}." });
            }
            
            return BadRequest(new { message = "One or more missing query parameters." });
        }
    }
}
