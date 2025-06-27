using Application.DTOs;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ✅ Return all provinces as DTOs, ordered by name
        public async Task<IEnumerable<ProvinceDto>> GetAllProvincesAsync()
        {
            var provinces = await _unitOfWork.GetRepository<Province>()
                .NoTrackingEntities
                .OrderBy(p => p.Name)
                .ToListAsync();

            return provinces.Select(p => new ProvinceDto
            {
                Id = p.Id,
                Name = p.Name
            });
        }

        // ✅ Return province with districts as DTOs, ordered by district name
        public async Task<LocationDto?> GetDistrictsByProvinceIdAsync(Guid provinceId)
        {
            var province = await _unitOfWork.GetRepository<Province>()
                .NoTrackingEntities
                .Include(p => p.Districts.OrderBy(d => d.Name)) // sort districts at the query level
                .FirstOrDefaultAsync(p => p.Id == provinceId);

            if (province == null)
                return null;

            return new LocationDto
            {
                Id = province.Id,
                Name = province.Name,
                Districts = province.Districts
                    .Select(d => new DistrictDto
                    {
                        Id = d.Id,
                        Name = d.Name
                    })
                    .ToList()
            };
        }
    }
}
