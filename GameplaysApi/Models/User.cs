using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Password { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
