using GameplaysApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameplaysApi.Converters
{
    public class GameConverter : JsonConverter<Game>
    {
        public override Game Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var game = new Game
                {
                    GameId = root.GetProperty("results").GetProperty("id").GetInt32(),
                    Name = root.GetProperty("results").GetProperty("name").GetString()!,
                    Deck = root.GetProperty("results").GetProperty("deck").GetString(),
                    //Description = root.GetProperty("results").GetProperty("description").GetString(),
                    Developers = root.GetProperty("results").GetProperty("developers")
                                    .EnumerateArray()
                                    .Select(d => new Developer 
                                    {
                                        DeveloperId = d.GetProperty("id").GetInt32(),
                                        Name = d.GetProperty("name").GetString()
                                    })
                                    .ToList(),
                    Franchises = root.GetProperty("results").GetProperty("franchises")
                                    .EnumerateArray()
                                    .Select(f => new Franchise
                                    {
                                        FranchiseId = f.GetProperty("id").GetInt32(),
                                        Name = f.GetProperty("name").GetString()
                                    })
                                    .ToList(),
                    Genres = root.GetProperty("results").GetProperty("genres")
                                    .EnumerateArray()
                                    .Select(g => new Genre
                                    {
                                        GenreId = g.GetProperty("id").GetInt32(),
                                        Name = g.GetProperty("name").GetString()
                                    })
                                    .ToList(),
                    //Image = root.GetProperty("results").GetProperty("image").GetString(),
                    OriginalReleaseDate = DateOnly.FromDateTime(root.GetProperty("results").GetProperty("original_release_date").GetDateTime()),
                    Platforms = root.GetProperty("results").GetProperty("platforms")
                                    .EnumerateArray()
                                    .Select(p => new Platform
                                    {
                                        PlatformId = p.GetProperty("id").GetInt32(),
                                        Name = p.GetProperty("name").GetString()
                                    })
                                    .ToList(),
                    Publishers = root.GetProperty("results").GetProperty("publishers")
                                    .EnumerateArray()
                                    .Select(p => new Publisher
                                    {
                                        PublisherId = p.GetProperty("id").GetInt32(),
                                        Name = p.GetProperty("name").GetString()
                                    })
                                    .ToList()
                };

                // deserialize the Image field from the JSON string into an ImageDetails object
                if (root.GetProperty("results").TryGetProperty("image", out JsonElement imageElement) && imageElement.ValueKind == JsonValueKind.String)
                {
                    var imageJson = imageElement.GetString();
                    if (!string.IsNullOrEmpty(imageJson))
                    {
                        game.Image = imageJson;
                    }
                }

                return game;
            }
        }

        public override void Write(Utf8JsonWriter writer, Game game, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageDetails
    {
        public string? IconUrl { get; set; }
        public string? MediumUrl { get; set; }
        public string? ScreenUrl { get; set; }
        public string? SmallUrl { get; set; }
        public string? SuperUrl { get; set; }
        public string? ThumbUrl { get; set; }
        public string? TinyUrl { get; set; }
        public string? OriginalUrl { get; set; }
        public string? ImageTags { get; set; }
    }
}
