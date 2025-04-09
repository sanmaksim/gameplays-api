using GameplaysApi.Converters;
using GameplaysApi.Data;
using GameplaysApi.DTOs;
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
        private readonly HttpClient _httpClient;
        
        public GamesController(ApplicationDbContext context, EntityTrackingService entityTrackingService, HttpClient httpClient)
        {
            _context = context;
            _entityTrackingService = entityTrackingService;
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
            // check whether the game can be retreived from the database
            if (int.TryParse(gameId, out int id))
            {
                var existingGame = await _context.Games
                    .Include(game => game.Developers)
                    .Include(game => game.Franchises)
                    .Include(game => game.Genres)
                    .Include(game => game.Platforms)
                    .Include(game => game.Publishers)
                    .Where(game => game.GameId == id)
                    .Select(game => new ResultsWrapperDto
                    {
                        Results = new GameResultDto
                        {
                            Name = game.Name,
                            Deck = game.Deck,
                            Developers = game.Developers!.Select(developer => new DeveloperDto { Name = developer.Name }).ToList(),
                            Franchises = game.Franchises!.Select(franchise => new FranchiseDto { Name = franchise.Name }).ToList(),
                            Genres = game.Genres!.Select(genre => new GenreDto { Name = genre.Name }).ToList(),
                            Image = game.Image,
                            OriginalReleaseDate = game.OriginalReleaseDate,
                            Platforms = game.Platforms!.Select(platform => new PlatformDto { Name = platform.Name }).ToList(),
                            Publishers = game.Publishers!.Select(publisher => new PublisherDto { Name = publisher.Name }).ToList()
                        }
                    })
                    .FirstOrDefaultAsync();

                var writeOptions = new JsonSerializerOptions
                {
                    // avoid infinite loops from models with many-to-many relationships by tracking object references
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    // increase max depth since object graph is deeper than the default 32
                    MaxDepth = 64
                };

                if (existingGame != null)
                {
                    var json = JsonSerializer.Serialize(existingGame, writeOptions);
                    return Ok(json);
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
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Developers, "DeveloperId");
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Franchises, "FranchiseId");
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Genres, "GenreId");
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Platforms, "PlatformId");
                await _entityTrackingService.AttachOrUpdateEntityAsync(game, game.Publishers, "PublisherId");

                _context.Games.Add(game);
                await _context.SaveChangesAsync();
            }

            return Ok(content);
        }
    }
}
