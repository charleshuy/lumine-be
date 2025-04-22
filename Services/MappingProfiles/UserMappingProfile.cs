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
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // roles will be added after
        }
    }

}
