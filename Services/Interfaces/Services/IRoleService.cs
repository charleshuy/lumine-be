using Application.DTOs.UserDTO;

namespace Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<List<RoleDTO>> GetAllRolesAsync();
        Task<RoleDTO?> GetRoleByNameAsync(string roleName);
        Task<bool> CreateRoleAsync(RoleDTO dto);
        Task<bool> UpdateRoleNameAsync(string oldRoleName, string newRoleName);
        Task<bool> DeleteRoleAsync(string roleName);
    }
}
