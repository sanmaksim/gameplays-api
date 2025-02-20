using System.ComponentModel.DataAnnotations.Schema;

namespace GameplaysApi.Models
{
    public enum PlayStatus
    {
        Playing,
        Played,
        Wishlist,
        Backlog
    }

    public class Play
    {
        public int PlayId { get; set; }

        public required int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; } // will be null initially, EF will auto-populate when db is queried by ID

        public required int GameId { get; set; }

        public int RunId { get; set; } = 1;

        public PlayStatus Status { get; set; } = PlayStatus.Backlog;

        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentageCompleted { get; set; } = 0.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursPlayed { get; set; } = 0.00m;

        public DateOnly? LastPlayedAt { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
