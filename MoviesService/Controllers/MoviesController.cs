using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesService.Data;
using MoviesService.Dtos;
using MoviesService.Models;

namespace MoviesService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepo _repository;
        private IMapper _mapper;

        public MoviesController(IMovieRepo repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<MovieReadDto>> GetMovies()
        {
            var movieItems = _repository.GetAllMovies().OrderBy(movie => movie.Id);

            return Ok(_mapper.Map<IEnumerable<MovieReadDto>>(movieItems));
        }

        [HttpGet("{id}", Name = "GetMovieById")]
        public ActionResult<MovieReadDto> GetMovieById(int id)
        {
            var movieItem = _repository.GetMovieById(id);

            if (movieItem != null) return Ok(_mapper.Map<MovieReadDto>(movieItem));

            return NotFound();
        }

        [HttpPost]
        public ActionResult<MovieReadDto> PostMovie(MovieCreateDto movieCreateDto)
        {
            var movieModel = _mapper.Map<Movie>(movieCreateDto);
            _repository.CreateMovie(movieModel);
            _repository.SaveChanges();

            var movieReadDto = _mapper.Map<MovieReadDto>(movieModel);

            return CreatedAtRoute(nameof(GetMovieById), 
                new {
                    movieReadDto.Id,
                    movieReadDto.PrimaryTitle,
                    movieReadDto.OriginalTitle,
                    movieReadDto.IsAdult,
                    movieReadDto.Year,
                    movieReadDto.RuntimeMinutes,
                    movieReadDto.Genres
                },
                movieReadDto);
        }
    }
}
