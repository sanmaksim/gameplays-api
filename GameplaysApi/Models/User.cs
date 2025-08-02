using GameplaysApi.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class User : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public required string Username { get; set; }

        [MaxLength(255)]
        [Required]
        public required string Email { get; set; } // Alternate Key

        [MaxLength(255)]
        [Required]
        public required string Password { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
