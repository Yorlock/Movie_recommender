using MoviesService.Models;

namespace MoviesService.Data
{
    public interface IMovieRepo
    {
        bool SaveChanges();

        IEnumerable<Movie> GetAllMovies();

        Movie? GetMovieById(int id);

        void CreateMovie(Movie movie);
    }
}
