using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        [MaxLength(255)]
        public string? Deck { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(255)]
        public string? Developers { get; set; }

        [MaxLength(255)]
        public string? Franchises { get; set; }

        [MaxLength(255)]
        public string? Genres { get; set; }

        [MaxLength(255)]
        public string? Images { get; set; }

        public DateOnly? OriginalReleaseDate { get; set; }

        [MaxLength(255)]
        public string? Platforms { get; set; }

        [MaxLength(255)]
        public string? Publishers { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
