using Microsoft.EntityFrameworkCore;
using MoviesService.Models;

namespace MoviesService.Data
{
    public class MovieRepo : IMovieRepo
    {
        private readonly AppDbContext _context;

        public MovieRepo(AppDbContext context)
        {
            _context = context;
        }

        public void CreateMovie(Movie movie)
        {
            ArgumentNullException.ThrowIfNull(movie);

            _context.Movies.Add(movie);
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            return _context.Movies.ToList();
        }

        public Movie? GetMovieById(int id)
        {
            return _context.Movies.FirstOrDefault(p => p.Id == id);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}
