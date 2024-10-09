using System.ComponentModel.DataAnnotations;

namespace GameplaysBackend.Models
{
    public class User
    {
        private string _password = string.Empty;

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
        public required string Password
        { 
            get { return _password; } 
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _password = BCrypt.Net.BCrypt.HashPassword(value);
                }
                else
                {
                    throw new ArgumentException("Password cannot be null or whitespace.");
                }
            }
        }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
