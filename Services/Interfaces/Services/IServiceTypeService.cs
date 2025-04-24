using Application.DTOs.ServiceDTO;

namespace Application.Interfaces.Services
{
    public interface IServiceTypeService
    {
        Task<IEnumerable<ServiceTypeDTO>> GetAllAsync();
        Task<ServiceTypeDTO> GetByIdAsync(Guid id);
        Task<ServiceTypeDTO> CreateAsync(CreateServiceTypeDTO dto);
        Task UpdateAsync(UpdateServiceTypeDTO dto);
        Task DeleteAsync(Guid id);
    }
}
