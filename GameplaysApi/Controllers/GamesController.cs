using GameplaysApi.Converters;
using GameplaysApi.Data;
using GameplaysApi.Models;
using GameplaysApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
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
        private readonly GameHelperService _gameHelperService;
        private readonly HttpClient _httpClient;
        
        public GamesController(
            ApplicationDbContext context, 
            EntityTrackingService entityTrackingService, 
            GameHelperService gameHelperService,
            HttpClient httpClient)
        {
            _context = context;
            _entityTrackingService = entityTrackingService;
            _gameHelperService = gameHelperService;
            _httpClient = httpClient;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery(Name = "q")] string query, [FromQuery(Name = "page")] string? page, [FromHeader(Name = "Result-Limit")] string? limit = "10")
        {
            string? apiUrl = Environment.GetEnvironmentVariable("GIANT_BOMB_API_URL");
            string? apiKey = Environment.GetEnvironmentVariable("GIANT_BOMB_API_KEY");
            string? appName = Environment.GetEnvironmentVariable("GAMEPLAYS_APP_NAME");
            string? appVersion = Environment.GetEnvironmentVariable("GAMEPLAYS_APP_VERSION");
            string? authorEmail = Environment.GetEnvironmentVariable("GIANT_BOMB_USER_AGENT_EMAIL");

            // verify api config is present
            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, "Giant Bomb API configuration is missing.");
            }

            apiUrl = $"{apiUrl}/search";

            // assemble search parameters
            var queryParams = new Dictionary<string, string?>
            {
                { "api_key", apiKey },
                { "format", "json" },
                { "limit", limit },
                { "page", page },
                { "query", query },
                { "resources", "game" },
            };

            // build the search url
            var uriBuilder = new UriBuilder(apiUrl)
            {
                Query = QueryHelpers.AddQueryString("", queryParams).TrimStart('?')
            };
            
            // Create a new HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());

            // The Giant Bomb API requires a custom User-Agent
            var userAgent = $"{appName}/{appVersion} ({authorEmail})";
            request.Headers.Add("User-Agent", userAgent);

            // send the request to the GB API
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                StatusCode((int)response.StatusCode);
            }

            // get the content from the request
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
                var existingGame = await _gameHelperService.GetExistingGame(id);

                var writeOptions = new JsonSerializerOptions
                {
                    // avoid infinite loops from models with many-to-many relationships by tracking object references
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    // increase max depth since object graph is deeper than the default 32
                    MaxDepth = 64
                };

                if (existingGame != null 
                    && existingGame.Results != null 
                    && (DateTime.Now - existingGame.Results.UpdatedAt).TotalDays < 1)
                {
                    var json = JsonSerializer.Serialize(existingGame, writeOptions);
                    return Ok(json);
                }
                else
                {
                    updateEntity = true;
                }
            }

            string? apiUrl = Environment.GetEnvironmentVariable("GIANT_BOMB_API_URL");
            string? apiKey = Environment.GetEnvironmentVariable("GIANT_BOMB_API_KEY");
            string? appName = Environment.GetEnvironmentVariable("GAMEPLAYS_APP_NAME");
            string? appVersion = Environment.GetEnvironmentVariable("GAMEPLAYS_APP_VERSION");
            string? authorEmail = Environment.GetEnvironmentVariable("GIANT_BOMB_USER_AGENT_EMAIL");
            string? resourcePrefix = Environment.GetEnvironmentVariable("GAMEPLAYS_RESOURCE_PREFIX");

            // verify api config is present
            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, "Giant Bomb API configuration is missing.");
            }

            apiUrl = $"{apiUrl}/game/{resourcePrefix}-{gameId}";

            // assemble search parameters
            var queryParams = new Dictionary<string, string?>
            {
                { "api_key", apiKey },
                { "format", "json" }
            };

            // build the search url
            var uriBuilder = new UriBuilder(apiUrl)
            {
                Query = QueryHelpers.AddQueryString("", queryParams).TrimStart('?')
            };
            
            // Create a new HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());

            // The Giant Bomb API requires a custom User-Agent
            var userAgent = $"{appName}/{appVersion} ({authorEmail})";
            request.Headers.Add("User-Agent", userAgent);

            // send the request to the GB API
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                StatusCode((int)response.StatusCode);
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
                        // Copy non-PK values from API object to the tracked entity
                        foreach (var property in _context.Entry(trackedGame.Entity).Properties)
                        {
                            if (!property.Metadata.IsPrimaryKey())
                            {
                                var newValue = typeof(Game).GetProperty(property.Metadata.Name)?.GetValue(game);
                                property.CurrentValue = newValue;
                            }
                        }
                        trackedGame.Entity.UpdateTimestamp();
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
