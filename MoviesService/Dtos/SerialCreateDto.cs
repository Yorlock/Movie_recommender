namespace MoviesService.Dtos
{
    public class SerialCreateDto
    {
        public required string? PrimaryTitle { get; set; }

        public required string OriginalTitle { get; set; }

        public required bool? IsAdult { get; set; }

        public required int? StartYear { get; set; }

        public required int? EndYear { get; set; }

        public required int? RuntimeMinutes { get; set; }

        public required IEnumerable<string>? Genres { get; set; }
    }
}
