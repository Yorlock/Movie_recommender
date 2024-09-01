using AutoMapper;
using MoviesService.Dtos;
using MoviesService.Models;

namespace MoviesService.Profiles
{
    public class MoviesService : Profile
    {
        public MoviesService()
        {
            // Source -> Target
            CreateMap<Movie, MovieReadDto>();
            CreateMap<MovieCreateDto, Movie>();
        }
    }
}
