using System.ComponentModel.DataAnnotations;

namespace GameplaysBackend.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }

        [MaxLength(255)]
        public string? Username { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Password { get; set; }
    }
}
