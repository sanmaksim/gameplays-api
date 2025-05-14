using System.Text.Json.Serialization;

namespace GameplaysApi.DTOs
{
    public class PlayStatusDto
    {
        [JsonPropertyName("playId")]
        public int PlayId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }
}
