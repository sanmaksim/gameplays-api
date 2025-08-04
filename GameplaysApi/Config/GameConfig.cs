using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Config
{
    public class GameConfig
    {
        [Required]
        public string AppName { get; set; } = default!;

        [Required]
        public string AppVersion { get; set; } = default!;

        [Required]
        public string AuthorEmail { get; set; } = default!;

        [Required]
        public string GiantBombApiKey { get; set; } = default!;

        [Required]
        public string GiantBombApiUrl { get; set; } = default!;

        [Required]
        public string GiantBombResourcePrefix { get; set; } = default!;
    }
}
