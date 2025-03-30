using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int GenreId { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public ICollection<Game>? Games { get; set; }
    }
}
