﻿using System.ComponentModel.DataAnnotations;

namespace MoviesService.Models
{
    public class Serial
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public required string? PrimaryTitle { get; set; }

        public required string OriginalTitle { get; set; }

        public required bool? IsAdult { get; set; }

        public required int? StartYear { get; set; }

        public required int? EndYear { get; set; }

        public required int? RuntimeMinutes { get; set; }

        public required List<string>? Genres { get; set; }
    }
}
