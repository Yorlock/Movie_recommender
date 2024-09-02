using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesService.Data;
using MoviesService.Dtos;
using MoviesService.Models;
using Serilog;

namespace SerialsService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SerialsController : ControllerBase
    {
        private readonly ISerialRepo _repository;
        private IMapper _mapper;

        public SerialsController(ISerialRepo repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SerialReadDto>> GetSerials()
        {
            Log.Information("Getting all Serials");

            var serialItems = _repository.GetAllSerials().OrderBy(serial => serial.Id);

            return Ok(_mapper.Map<IEnumerable<SerialReadDto>>(serialItems));
        }

        [HttpGet("{id}", Name = "GetSerialById")]
        public ActionResult<SerialReadDto> GetSerialById(int id)
        {
            Log.Information("Getting Serial by id: {@id}", id);

            var serialItem = _repository.GetSerialById(id);

            if (serialItem != null) return Ok(_mapper.Map<SerialReadDto>(serialItem));

            return NotFound();
        }

        [HttpPost]
        public ActionResult<SerialReadDto> PostSerial(SerialCreateDto serialCreateDto)
        {
            Log.Information("Posting Serial: {@serialCreateDto}", serialCreateDto);

            var serialModel = _mapper.Map<Serial>(serialCreateDto);
            _repository.CreateSerial(serialModel);
            _repository.SaveChanges();

            var serialReadDto = _mapper.Map<SerialReadDto>(serialModel);

            return CreatedAtRoute(nameof(GetSerialById),
                new
                {
                    serialReadDto.Id,
                    serialReadDto.PrimaryTitle,
                    serialReadDto.OriginalTitle,
                    serialReadDto.IsAdult,
                    serialReadDto.StartYear,
                    serialReadDto.EndYear,
                    serialReadDto.RuntimeMinutes,
                    serialReadDto.Genres
                },
                serialReadDto);
        }

    }
}
