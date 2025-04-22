using Application.DTOs.UserDTO;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using Application.Paggings;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<PaginatedList<ResponseUserDTO>> GetPaginatedUsers(int pageIndex, int pageSize)
        {
            var query = _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.FirstName);

            var pagedUsers = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetPagging(query, pageIndex, pageSize);

            // Automap users
            var usersDto = _mapper.Map<List<ResponseUserDTO>>(pagedUsers.Items);

            // Manually inject roles (can't be auto-mapped easily)
            for (int i = 0; i < pagedUsers.Items.Count; i++)
            {
                var user = pagedUsers.Items[i];
                var roles = await _userManager.GetRolesAsync(user);
                usersDto[i].Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();
            }

            return new PaginatedList<ResponseUserDTO>(usersDto, pagedUsers.TotalCount, pageIndex, pageSize);
        }
    }
}
