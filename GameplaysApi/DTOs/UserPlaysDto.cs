using System.Text.Json.Serialization;

namespace GameplaysApi.DTOs
{
    public class UserPlaysDto
    {
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("developers")]
        public List<DevDto>? Developers { get; set; }

        [JsonPropertyName("original_release_date")]
        public DateOnly? OriginalReleaseDate { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateOnly CreatedAt { get; set; }

        [JsonPropertyName("hours_played")]
        public decimal? HoursPlayed { get; set; }

        [JsonPropertyName("percentage_completed")]
        public decimal? PercentageCompleted { get; set; }

        [JsonPropertyName("last_played_at")]
        public DateOnly? LastPlayedAt { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("api_game_id")]
        public int ApiGameId { get; set; }
    }

    public class DevDto
    {
        [JsonPropertyName("id")]
        public int? DevId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
