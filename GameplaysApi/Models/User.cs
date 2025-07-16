using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class User
    {
        private string _password = string.Empty;

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPassword);
        }

        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
