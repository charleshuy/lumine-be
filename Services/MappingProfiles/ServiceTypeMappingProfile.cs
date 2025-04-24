using Application.DTOs.ServiceDTO;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles
{
    public class ServiceTypeMappingProfile : Profile
    {
        public ServiceTypeMappingProfile()
        {
            CreateMap<ServiceType, ServiceTypeDTO>().ReverseMap();
            CreateMap<CreateServiceTypeDTO, ServiceType>();
            CreateMap<UpdateServiceTypeDTO, ServiceType>();
        }
    }
}
