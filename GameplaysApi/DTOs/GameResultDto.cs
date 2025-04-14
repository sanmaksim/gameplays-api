using GameplaysApi.Models;
using System.Text.Json.Serialization;

namespace GameplaysApi.DTOs
{
    public class ResultsWrapperDto
    {
        [JsonPropertyName("results")]
        public GameResultDto? Results { get; set; }
    }

    public class GameResultDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("deck")]
        public string? Deck { get; set; }

        //[JsonPropertyName("description")]
        //public string? Description { get; set; }

        [JsonPropertyName("developers")]
        public List<DeveloperDto>? Developers { get; set; }

        [JsonPropertyName("franchises")]
        public List<FranchiseDto>? Franchises { get; set; }

        [JsonPropertyName("genres")]
        public List<GenreDto>? Genres { get; set; }

        [JsonPropertyName("image")]
        public Image? Image { get; set; }

        [JsonPropertyName("original_release_date")]
        public DateOnly? OriginalReleaseDate { get; set; }

        [JsonPropertyName("platforms")]
        public List<PlatformDto>? Platforms { get; set; }

        [JsonPropertyName("publishers")]
        public List<PublisherDto>? Publishers { get; set; }
    }

    public class DeveloperDto
    {
        [JsonPropertyName("id")]
        public int? DeveloperId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class FranchiseDto
    {
        [JsonPropertyName("id")]
        public int? FranchiseId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class GenreDto
    {
        [JsonPropertyName("id")]
        public int? GenreId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class PlatformDto
    {
        [JsonPropertyName("id")]
        public int? PlatformId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class PublisherDto
    {
        [JsonPropertyName("id")]
        public int? PublisherId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
