using GameplaysApi.Models;
using System.Globalization;
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

                // The 'id' property from the incoming JSON payload corresponds to
                // the AK 'GameId' in Game so throw an exception if null or missing
                if (!root.GetProperty("results").TryGetProperty("id", out var idProp)
                    || idProp.ValueKind == JsonValueKind.Null)
                {
                    throw new JsonException("Required property 'id' is missing or null in the JSON payload.");
                }

                // Extract the cleaned overview text returned from the GB API
                var htmlSection = new HtmlSectionExtractor();
                var overview = htmlSection.ExtractOverviewText(
                    root.GetProperty("results").TryGetProperty("description", out var descProp)
                            && descProp.ValueKind != JsonValueKind.Null
                            ? descProp.GetString() ?? ""
                            : ""
                );

                var game = new Game
                {
                    
                    GameId = idProp.GetInt32(),

                    Name = root.GetProperty("results").TryGetProperty("name", out var nameProp)
                            && nameProp.ValueKind != JsonValueKind.Null
                            ? nameProp.GetString() ?? ""
                            : "",

                    DateLastUpdated = root.GetProperty("results").TryGetProperty("date_last_updated", out var dateLastUpdatedProp)
                                        && dateLastUpdatedProp.ValueKind != JsonValueKind.Null
                                        ? dateLastUpdatedProp.GetString() is string dateStr && dateStr != null
                                            ? DateTime.ParseExact(
                                                dateStr,
                                                "yyyy-MM-dd HH:mm:ss",
                                                CultureInfo.InvariantCulture
                                                )
                                            : null
                                        : null,

                    Description = overview,

                    Deck = root.GetProperty("results").TryGetProperty("deck", out var deckProp)
                            && deckProp.ValueKind != JsonValueKind.Null
                            ? deckProp.GetString() ?? ""
                            : "",

                    Developers = root.GetProperty("results").TryGetProperty("developers", out var devsProp)
                                    && devsProp.ValueKind != JsonValueKind.Null && devsProp.ValueKind == JsonValueKind.Array
                                    ? devsProp.EnumerateArray()
                                        .Select(d => new Developer 
                                        {
                                            DeveloperId = d.GetProperty("id").GetInt32(),
                                            Name = d.GetProperty("name").GetString()
                                        })
                                        .ToList()
                                    : new List<Developer>(),

                    Franchises = root.GetProperty("results").TryGetProperty("franchises", out var franchisesProp)
                                    && franchisesProp.ValueKind != JsonValueKind.Null && franchisesProp.ValueKind == JsonValueKind.Array
                                    ? franchisesProp.EnumerateArray()
                                        .Select(f => new Franchise
                                        {
                                            FranchiseId = f.GetProperty("id").GetInt32(),
                                            Name = f.GetProperty("name").GetString()
                                        })
                                        .ToList()
                                    : new List<Franchise>(),

                    Genres = root.GetProperty("results").TryGetProperty("genres", out var genresProp)
                                && genresProp.ValueKind != JsonValueKind.Null && genresProp.ValueKind == JsonValueKind.Array
                                ? genresProp.EnumerateArray()
                                    .Select(g => new Genre
                                    {
                                        GenreId = g.GetProperty("id").GetInt32(),
                                        Name = g.GetProperty("name").GetString()
                                    })
                                    .ToList()
                                : new List<Genre>(),

                    Image = new Image
                    {
                        // The incoming JSON 'image' property contains the nested properties below, 
                        // therefore we need to test whether the JsonValueKind is an Object prior to
                        // testing the nested properties for Null values
                        IconUrl = root.GetProperty("results").TryGetProperty("image", out var imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("icon_url", out var iconUrlProp)
                                        && iconUrlProp.ValueKind != JsonValueKind.Null
                                        ? iconUrlProp.GetString()
                                        : null
                                    : null,
                        MediumUrl = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("medium_url", out var medUrlProp)
                                        && medUrlProp.ValueKind != JsonValueKind.Null
                                        ? medUrlProp.GetString()
                                        : null
                                    : null,
                        ScreenUrl = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("screen_url", out var screenUrlProp)
                                        && screenUrlProp.ValueKind != JsonValueKind.Null
                                        ? screenUrlProp.GetString()
                                        : null
                                    : null,
                        SmallUrl = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("small_url", out var smallUrlProp)
                                        && smallUrlProp.ValueKind != JsonValueKind.Null
                                        ? smallUrlProp.GetString()
                                        : null
                                    : null,
                        SuperUrl = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("super_url", out var superUrlProp)
                                        && superUrlProp.ValueKind != JsonValueKind.Null
                                        ? superUrlProp.GetString()
                                        : null
                                    : null,
                        ThumbUrl = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("thumb_url", out var thumbUrlProp)
                                        && thumbUrlProp.ValueKind != JsonValueKind.Null
                                        ? thumbUrlProp.GetString()
                                        : null
                                    : null,
                        TinyUrl = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("tiny_url", out var tinyUrlProp)
                                        && tinyUrlProp.ValueKind != JsonValueKind.Null
                                        ? tinyUrlProp.GetString()
                                        : null
                                    : null,
                        OriginalUrl = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("original_url", out var originalUrlProp)
                                        && originalUrlProp.ValueKind != JsonValueKind.Null
                                        ? originalUrlProp.GetString()
                                        : null
                                    : null,
                        ImageTags = root.GetProperty("results").TryGetProperty("image", out imgProp)
                                    && imgProp.ValueKind != JsonValueKind.Null && imgProp.ValueKind == JsonValueKind.Object
                                    ? imgProp.TryGetProperty("image_tags", out var imgTagsProp)
                                        && imgTagsProp.ValueKind != JsonValueKind.Null
                                        ? imgTagsProp.GetString()
                                        : null
                                    : null
                    },

                    OriginalReleaseDate = root.GetProperty("results").TryGetProperty("original_release_date", out var originalReleaseDateProp)
                                            && originalReleaseDateProp.ValueKind != JsonValueKind.Null
                                            ? DateOnly.FromDateTime(originalReleaseDateProp.GetDateTime())
                                            : null,

                    Platforms = root.GetProperty("results").TryGetProperty("platforms", out var plaformsProp)
                                    && plaformsProp.ValueKind != JsonValueKind.Null && plaformsProp.ValueKind == JsonValueKind.Array
                                    ? plaformsProp.EnumerateArray()
                                        .Select(p => new Platform
                                        {
                                            PlatformId = p.GetProperty("id").GetInt32(),
                                            Name = p.GetProperty("name").GetString()
                                        })
                                        .ToList()
                                    : new List<Platform>(),

                    Publishers = root.GetProperty("results").TryGetProperty("publishers", out var pubsProp)
                                    && pubsProp.ValueKind != JsonValueKind.Null && pubsProp.ValueKind == JsonValueKind.Array
                                    ? pubsProp.EnumerateArray()
                                        .Select(p => new Publisher
                                        {
                                            PublisherId = p.GetProperty("id").GetInt32(),
                                            Name = p.GetProperty("name").GetString()
                                        })
                                        .ToList()
                                    : new List<Publisher>()
                };

                // Serialize Game's Image object to a JSON string
                // and store it in the Game's ImageJson property
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
