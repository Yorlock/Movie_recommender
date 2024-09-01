using AutoMapper;
using MoviesService.Dtos;
using MoviesService.Models;

namespace MoviesService.Profiles
{
    public class SerialsProfile : Profile
    {
        public SerialsProfile()
        {
            // Source -> Target
            CreateMap<Serial, SerialReadDto>();
            CreateMap<SerialCreateDto, Serial>();
        }
    }
}
