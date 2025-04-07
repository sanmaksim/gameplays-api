using System.Text.Json.Serialization;

namespace GameplaysApi.Models
{
    public class Image
    {
        [JsonPropertyName("icon_url")]
        public string? IconUrl { get; set; }

        [JsonPropertyName("medium_url")]
        public string? MediumUrl { get; set; }

        [JsonPropertyName("screen_url")]
        public string? ScreenUrl { get; set; }

        [JsonPropertyName("small_url")]
        public string? SmallUrl { get; set; }

        [JsonPropertyName("super_url")]
        public string? SuperUrl { get; set; }

        [JsonPropertyName("thumb_url")]
        public string? ThumbUrl { get; set; }

        [JsonPropertyName("tiny_url")]
        public string? TinyUrl { get; set; }

        [JsonPropertyName("original_url")]
        public string? OriginalUrl { get; set; }

        [JsonPropertyName("image_tags")]
        public string? ImageTags { get; set; }
    }
}
