using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace GameplaysBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public GamesController(HttpClient httpClient)
        {
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

            apiUrl = $"{apiUrl}/game/3030-{gameId}";

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
            return Ok(content);
        }
    }
}
