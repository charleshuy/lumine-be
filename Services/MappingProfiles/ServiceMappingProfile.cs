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
                .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src =>
                    src.Artist != null ? $"{src.Artist.FirstName} {src.Artist.LastName}" : null))
                .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.ServiceType))
                .ReverseMap();


            CreateMap<CreateServiceDTO, Service>();
            CreateMap<UpdateServiceDTO, Service>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.DurationInMinutes)));
    

            CreateMap<CreateServiceForArtistDTO, Service>()
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.DurationInMinutes)));

        }
    }
}
