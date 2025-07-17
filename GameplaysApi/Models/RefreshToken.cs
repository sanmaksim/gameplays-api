using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameplaysApi.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [MaxLength(255)]
        public required string Token { get; set; }
        
        public int UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public string? UserAgent { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }
    }
}
