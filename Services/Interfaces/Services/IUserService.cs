using Application.DTOs.UserDTO;
using Application.Paggings;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<PaginatedList<ResponseUserDTO>> GetPaginatedUsers(
            int pageIndex,
            int pageSize,
            string? username = null,
            string? email = null,
            string? phoneNumber = null);
        Task<ResponseUserDTO?> GetUserByIdAsync(Guid userId);
        Task<ResponseUserDTO?> GetCurrentUserAsync();
        Task<bool> UpdateUserAsync(Guid userId, UpdateUserDTO dto);
        Task<bool> UpdateCurrentUserProfileAsync(UpdateProfileDTO dto);
        Task<bool> DeleteUserAsync(Guid userId);
        Guid? GetCurrentUserId();
    }
}
