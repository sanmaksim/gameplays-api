using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class Platform
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int PlatformId {  get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public List<Game>? Games { get; set; }
    }
}
