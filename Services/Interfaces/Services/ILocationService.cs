using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<ProvinceDto>> GetAllProvincesAsync();
        Task<LocationDto?> GetDistrictsByProvinceIdAsync(Guid provinceId);
    }
}
