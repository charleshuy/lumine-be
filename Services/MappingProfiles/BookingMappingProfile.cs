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
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                .ForPath(dest => dest.Service.ArtistName, opt => opt.MapFrom(src => src.Service != null && src.Service.Artist != null ? src.Service.Artist.UserName : null));

            CreateMap<Service, BookingServiceDTO>();

            CreateMap<BookingCreateDTO, Booking>()
                .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => BookingStatus.Pending));


            // 👇 Add this mapping to fix the PaginatedList problem  
            CreateMap(typeof(PaginatedList<>), typeof(PaginatedList<>))
                .ConvertUsing(typeof(PaginatedListConverter<,>));
        }
    }
}
