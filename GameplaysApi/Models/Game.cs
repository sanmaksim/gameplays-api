using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GameplaysApi.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int GameId { get; set; } // Alternate Key

        [MaxLength(255)]
        [Required]
        public required string Name { get; set; }

        [MaxLength(255)]
        public string? Deck { get; set; }

        //[MaxLength(255)]
        //public string? Description { get; set; }

        public ICollection<Developer>? Developers { get; set; }

        public ICollection<Franchise>? Franchises { get; set; }

        public ICollection<Genre>? Genres { get; set; }

        [MaxLength(255)]
        public string? ImageJson { get; set; } // Column Type = "json"

        public DateOnly? OriginalReleaseDate { get; set; }

        public ICollection<Platform>? Platforms { get; set; }

        public ICollection<Publisher>? Publishers { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        // This is the non-mapped property that holds the Image object
        [NotMapped]
        public Image? Image
        {
            get => string.IsNullOrEmpty(ImageJson) ? null : JsonSerializer.Deserialize<Image>(ImageJson);
            set => ImageJson = JsonSerializer.Serialize(value);
        }
    }
}
