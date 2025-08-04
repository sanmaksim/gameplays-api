using GameplaysApi.Config;
using GameplaysApi.Converters;
using GameplaysApi.Data;
using GameplaysApi.Models;
using GameplaysApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EntityTrackingService _entityTrackingService;
        private readonly GameConfig _gameConfig;
        private readonly GameService _gameService;
        private readonly HttpClient _httpClient;

        public GamesController(
            ApplicationDbContext context,
            EntityTrackingService entityTrackingService,
            IOptions<GameConfig> gameConfig,
            GameService gameService,
            HttpClient httpClient)
        {
            _context = context;
            _entityTrackingService = entityTrackingService;
            _gameConfig = gameConfig.Value;
            _gameService = gameService;
            _httpClient = httpClient;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery(Name = "q")] string query, 
            [FromQuery(Name = "page")] string? page, 
            [FromHeader(Name = "Result-Limit")] string? limit = "10")
        {
            var apiSearchUrl = $"{_gameConfig.GiantBombApiUrl}/search";

            // Assemble the search parameters
            var queryParams = new Dictionary<string, string?>
            {
                { "api_key", _gameConfig.GiantBombApiKey },
                { "format", "json" },
                { "limit", limit },
                { "page", page },
                { "query", query },
                { "resources", "game" },
            };

            // Build the search url
            var uriBuilder = new UriBuilder(apiSearchUrl)
            {
                Query = QueryHelpers.AddQueryString("", queryParams).TrimStart('?')
            };
            
            // Create a new HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());

            // Define a custom User-Agent as required by the Giant Bomb API
            var userAgent = $"{_gameConfig.AppName}/{_gameConfig.AppVersion} ({_gameConfig.AuthorEmail})";
            request.Headers.Add("User-Agent", userAgent);

            // Send the request to the Giant Bomb API
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            // Return the content from the request
            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }

        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetGame(string gameId)
        {
            bool updateEntity = false;

            // check whether the game can be retreived from the database
            if (int.TryParse(gameId, out int id))
            {
                var existingGame = await _gameService.GetExistingGame(id);

                var writeOptions = new JsonSerializerOptions
                {
                    // avoid infinite loops from models with many-to-many relationships by tracking object references
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    // increase max depth since object graph is deeper than the default 32
                    MaxDepth = 64
                };

                // refresh existing game entity after 30 days
                if (existingGame != null 
                    && existingGame.Results != null 
                    && (DateTime.Now - existingGame.Results.UpdatedAt).TotalDays < 30)
                {
                    var json = JsonSerializer.Serialize(existingGame, writeOptions);
                    return Ok(json);
                }
                else
                {
                    updateEntity = true;
                }
            }

            var apiGameUrl = $"{_gameConfig.GiantBombApiUrl}/game/{_gameConfig.GiantBombResourcePrefix}-{gameId}";

            // assemble search parameters
            var queryParams = new Dictionary<string, string?>
            {
                { "api_key", _gameConfig.GiantBombApiKey },
                { "format", "json" }
            };

            // build the search url
            var uriBuilder = new UriBuilder(apiGameUrl)
            {
                Query = QueryHelpers.AddQueryString("", queryParams).TrimStart('?')
            };
            
            // Create a new HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());

            // The Giant Bomb API requires a custom User-Agent
            var userAgent = $"{_gameConfig.AppName}/{_gameConfig.AppVersion} ({_gameConfig.AuthorEmail})";
            request.Headers.Add("User-Agent", userAgent);

            // send the request to the GB API
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            // get the content from the request
            var content = await response.Content.ReadAsStringAsync();

            // create JsonSerializerOptions and add the custom converter
            var readOptions = new JsonSerializerOptions
            {
                Converters = { new GameConverter() }
            };

            // deserialize the returned content using the custom converter
            var game = JsonSerializer.Deserialize<Game>(content, readOptions);

            // save the game to the database
            if (game != null)
            {
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Developers, "DeveloperId", updateEntity);
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Franchises, "FranchiseId", updateEntity);
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Genres, "GenreId", updateEntity);
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Platforms, "PlatformId", updateEntity);
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Publishers, "PublisherId", updateEntity);

                // Check whether the 'game' entity is already tracked in the db for update purposes
                var trackedGame = _context.ChangeTracker.Entries<Game>().FirstOrDefault(e => e.Entity.GameId == game.GameId);

                if (trackedGame != null)
                {
                    if (updateEntity == true)
                    {
                        // Copy relevant values from the main API entity to the main tracked entity
                        foreach (var property in _context.Entry(trackedGame.Entity).Properties)
                        {
                            if (!property.Metadata.IsPrimaryKey() && property.Metadata.Name != nameof(Game.CreatedAt))
                            {
                                property.CurrentValue = typeof(Game).GetProperty(property.Metadata.Name)?.GetValue(game);
                            }
                        }
                    }
                    else
                    {
                        _context.Entry(trackedGame.Entity).State = EntityState.Unchanged;
                    }
                }
                else
                {
                    _context.Games.Add(game);
                }
                
                await _context.SaveChangesAsync();
            }

            return Ok(content);
        }
    }
}
