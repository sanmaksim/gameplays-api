using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameplaysBackend.Models;
using GameplaysBackend.Data;

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
        public async Task<IActionResult> Search([FromQuery(Name = "q")] string query)
        {
            string? apiKey = Environment.GetEnvironmentVariable("GIANT_BOMB_API_KEY");
            string? appName = Environment.GetEnvironmentVariable("GAMEPLAYS_APP_NAME");
            string? appVersion = Environment.GetEnvironmentVariable("GAMEPLAYS_APP_VERSION");
            string? authorEmail = Environment.GetEnvironmentVariable("GIANT_BOMB_USER_AGENT_EMAIL");

            var url = $"http://www.giantbomb.com/api/search/?api_key={apiKey}&query={query}&format=json&resources=game";

            // Create a new HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // The Giant Bomb API requires a custom User-Agent
            var userAgent = $"{appName}/{appVersion} ({authorEmail})";
            request.Headers.Add("User-Agent", userAgent);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                StatusCode((int)response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
    }
}
