using Application.DTOs.ServiceDTO;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class ServiceMappingProfile : Profile
    {
        public ServiceMappingProfile() 
        {
            CreateMap<Service, ResponseServiceDTO>()
            .ForMember(dest => dest.ServiceDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src =>
                src.Artist != null ? $"{src.Artist.FirstName} {src.Artist.LastName}" : null));
        }
    }
}
