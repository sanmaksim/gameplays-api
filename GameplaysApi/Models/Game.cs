using GameplaysApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GameplaysApi.Models
{
    public class Game : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int GameId { get; set; } // Alternate Key

        [MaxLength(255)]
        [Required]
        public required string Name { get; set; }

        [Precision(0)]
        public DateTime? DateLastUpdated { get; set; }

        [MaxLength(255)]
        public string? Deck { get; set; }

        //[MaxLength(255)]
        //public string? Description { get; set; }

        public List<Developer>? Developers { get; set; }

        public List<Franchise>? Franchises { get; set; }

        public List<Genre>? Genres { get; set; }

        [MaxLength(255)]
        public string? ImageJson { get; set; } // Column Type = "json"

        public DateOnly? OriginalReleaseDate { get; set; }

        public List<Platform>? Platforms { get; set; }

        public List<Publisher>? Publishers { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

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
