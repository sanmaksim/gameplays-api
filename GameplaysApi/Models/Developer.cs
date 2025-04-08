using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class Developer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int DeveloperId { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public List<Game>? Games { get; set; }
    }
}
