using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameplaysApi.Models
{
    public enum PlayStatus
    {
        Unplayed,
        InProgress,
        Completed
    }

    public class Play
    {
        public int PlayId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public required User User { get; set; }

        public int GameId { get; set; }

        [ForeignKey("GameId")]
        public required Game Game { get; set; }

        public required int RunId { get; set; } = 1;

        public required PlayStatus Status { get; set; } = PlayStatus.Unplayed;

        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentageCompleted { get; set; } = 0.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursPlayed { get; set; } = 0.00m;

        public DateTime LastPlayedAt { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
