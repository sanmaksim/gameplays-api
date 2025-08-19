using GameplaysApi.Interfaces;
using System.ComponentModel.DataAnnotations;
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

    public class Play : IHasCreatedAt, IHasUpdatedAt
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; } // will be null initially, EF will auto-populate when db is queried by ID

        public int GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game? Game { get; set; } // will be null initially, EF will auto-populate when db is queried by ID

        public int ApiGameId { get; set; }

        public int RunId { get; set; } = 1;

        public PlayStatus Status { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentageCompleted { get; set; } = 0.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursPlayed { get; set; } = 0.00m;

        public DateOnly? LastPlayedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
