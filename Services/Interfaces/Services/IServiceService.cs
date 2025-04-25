using Application.DTOs.ServiceDTO;
using Application.Paggings;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IServiceService
    {
        Task<PaginatedList<ResponseServiceDTO>> GetServicesAsync(
            int pageIndex,
            int pageSize,
            string? serviceName = null,
            decimal? price = null,
            ServiceStatus? status = null,
            Guid? artistId = null);

        Task<PaginatedList<ResponseServiceDTO>> GetServicesForArtistAsync(
            int pageIndex,
            int pageSize,
            string? serviceName = null,
            decimal? price = null,
            ServiceStatus? status = null);

        Task<PaginatedList<ResponseServiceDTO>> GetAllServicesByArtistIdAsync(int pageIndex, int pageSize, Guid artistId);
        Task<ResponseServiceDTO> CreateAsync(CreateServiceDTO dto);
        Task UpdateAsync(UpdateServiceDTO dto);
        Task DeleteAsync(Guid id);
        Task<ResponseServiceDTO> CreateServiceForArtistAsync(CreateServiceForArtistDTO dto);
        Task UpdateServiceForArtistAsync(UpdateServiceDTO dto);
    }
}
