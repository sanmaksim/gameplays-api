﻿using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Models
{
    public class Developer
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public required string Name { get; set; }

        public required ICollection<Game> Games { get; set; }
    }
}
