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
                    DateLastUpdated = DateTime.ParseExact(
                        root.GetProperty("results").GetProperty("date_last_updated").GetString()!, 
                        "yyyy-MM-dd HH:mm:ss", 
                        System.Globalization.CultureInfo.InvariantCulture),
                    Deck = root.GetProperty("results").GetProperty("deck").GetString(),
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
                    Image = new Image
                    {
                        IconUrl = root.GetProperty("results").GetProperty("image").GetProperty("icon_url").GetString(),
                        MediumUrl = root.GetProperty("results").GetProperty("image").GetProperty("medium_url").GetString(),
                        ScreenUrl = root.GetProperty("results").GetProperty("image").GetProperty("screen_url").GetString(),
                        SmallUrl = root.GetProperty("results").GetProperty("image").GetProperty("small_url").GetString(),
                        SuperUrl = root.GetProperty("results").GetProperty("image").GetProperty("super_url").GetString(),
                        ThumbUrl = root.GetProperty("results").GetProperty("image").GetProperty("thumb_url").GetString(),
                        TinyUrl = root.GetProperty("results").GetProperty("image").GetProperty("tiny_url").GetString(),
                        OriginalUrl = root.GetProperty("results").GetProperty("image").GetProperty("original_url").GetString(),
                        ImageTags = root.GetProperty("results").GetProperty("image").GetProperty("image_tags").GetString()
                    },
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

                // Serialize the Image object to a JSON string and store it in the ImageJson property
                game.ImageJson = JsonSerializer.Serialize(game.Image);

                return game;
            }
        }

        public override void Write(Utf8JsonWriter writer, Game game, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
