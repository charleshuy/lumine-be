using Application.DTOs;
using Application.DTOs.ServiceDTO;
using Application.Paggings;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service));

            CreateMap<Service, BookingServiceDTO>();

            // 👇 Add this mapping to fix the PaginatedList problem
            CreateMap(typeof(PaginatedList<>), typeof(PaginatedList<>))
                .ConvertUsing(typeof(PaginatedListConverter<,>));
        }
    }
}
