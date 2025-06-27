using Application.DTOs;
using Application.DTOs.UserDTO;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<ApplicationUser, ResponseUserDTO>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()) // roles handled manually
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.District));

            CreateMap<ApplicationUser, CustomerDTO>();

            // Nested mapping
            CreateMap<District, UserDistrictDto>()
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province));

            CreateMap<Province, ProvinceDto>();
        }
    }


}
