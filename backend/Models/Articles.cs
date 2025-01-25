using System;
using System.ComponentModel.DataAnnotations;


namespace Backend.Models
{
    public class Article
    {
        [Key]
        public int id { get; set; }

        [Required]
        [MaxLength(255)]
        public string title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string author { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? annotation { get; set; }

        [Required]
        public string content { get; set; } = string.Empty;

        [Required]
        public DateTime published_date { get; set; }

    }
}