using Application.DTOs.UserDTO;
using Application.Paggings;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<PaginatedList<ResponseUserDTO>> GetPaginatedUsers(int pageIndex, int pageSize);
    }
}
