using System.ComponentModel.DataAnnotations;

namespace MoviesService.Dtos
{
    public class MovieCreateDto
    {
        public required string? PrimaryTitle { get; set; }

        public required string OriginalTitle { get; set; }

        public required bool IsAdult { get; set; }

        public required int Year { get; set; }

        public required int? RuntimeMinutes { get; set; }

        public required IEnumerable<string> Genres { get; set; }
    }
}
