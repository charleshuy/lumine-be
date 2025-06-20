﻿using Application.DTOs.SearchFilters;
using Application.DTOs.UserDTO;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using Application.Paggings;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Security.Claims;
using static Domain.Base.BaseException;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageService _imageService;

        public UserService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IImageService imageService) // inject this
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
        }

        public async Task<PaginatedList<ResponseUserDTO>> GetPaginatedUsers(
            int pageIndex,
            int pageSize,
            UserSearchFilterDTO? filter = null)
        {
            var query = _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Where(u => !u.IsDeleted);

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Username))
                    query = query.Where(u => u.UserName!.Contains(filter.Username));

                if (!string.IsNullOrWhiteSpace(filter.Email))
                    query = query.Where(u => u.Email!.Contains(filter.Email));

                if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
                    query = query.Where(u => u.PhoneNumber!.Contains(filter.PhoneNumber));
            }

            query = query.OrderBy(u => u.Email);

            var pagedUsers = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetPagging(query, pageIndex, pageSize);

            var filteredUsers = new List<ApplicationUser>();

            foreach (var user in pagedUsers.Items)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (filter == null || string.IsNullOrWhiteSpace(filter.Role) ||
                    roles.Any(r => r.Equals(filter.Role, StringComparison.OrdinalIgnoreCase)))
                {
                    filteredUsers.Add(user);
                }
            }


            var usersDto = _mapper.Map<List<ResponseUserDTO>>(filteredUsers);

            for (int i = 0; i < filteredUsers.Count; i++)
            {
                var roles = await _userManager.GetRolesAsync(filteredUsers[i]);
                usersDto[i].Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();
            }

            return new PaginatedList<ResponseUserDTO>(
                usersDto,
                filteredUsers.Count,
                pageIndex,
                pageSize
            );
        }



        public async Task<List<ResponseUserDTO>> GetAllUsersAsync()
        {
            var users = _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Email)
                .ToList();

            var usersDto = _mapper.Map<List<ResponseUserDTO>>(users);

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var roles = await _userManager.GetRolesAsync(user);
                usersDto[i].Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();
            }

            return usersDto;
        }

        public async Task<List<CreatedSummaryDTO>> GetUsersCreatedSummaryAsync()
        {
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            var summary = users
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new CreatedSummaryDTO
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToList();

            return summary;
        }



        public async Task<ResponseUserDTO?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                return null;

            var userDto = _mapper.Map<ResponseUserDTO>(user);

            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();

            return userDto;
        }

        public Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
            throw new AuthenticationException("Loggin first pls!!!");
        }

        public async Task<ResponseUserDTO?> GetCurrentUserAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null || user.IsDeleted)
                return null;

            var userDto = _mapper.Map<ResponseUserDTO>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();

            return userDto;
        }
        public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                return false;

            // Skip null fields
            if (!string.IsNullOrWhiteSpace(dto.UserName))
                user.UserName = dto.UserName;

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            // Update roles if provided
            if (dto.Roles is not null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var toRemove = currentRoles.Except(dto.Roles).ToList();
                var toAdd = dto.Roles.Except(currentRoles).ToList();

                if (toRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
                    if (!removeResult.Succeeded)
                        throw new Exception(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
                }

                if (toAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, toAdd);
                    if (!addResult.Succeeded)
                        throw new Exception(string.Join("; ", addResult.Errors.Select(e => e.Description)));
                }
            }

            return true;
        }
        public async Task<bool> UpdateCurrentUserProfileAsync(UpdateProfileDTO dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return false;

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null || user.IsDeleted)
                return false;

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                user.Address = dto.Address; // ✅ Add this

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                return false;

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            return true;
        }

        public async Task<PaginatedList<ResponseUserDTO>> GetUnapprovedArtistsAsync(int pageIndex, int pageSize)
        {
            var artistUsers = await _userManager.Users
                .Where(u => !u.IsDeleted && !u.isApproved)
                .ToListAsync();

            // Filter users in memory to only those in "Artist" role
            var unapprovedArtists = new List<ApplicationUser>();

            foreach (var user in artistUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Artist"))
                {
                    unapprovedArtists.Add(user);
                }
            }

            var totalCount = unapprovedArtists.Count;
            var pagedUsers = unapprovedArtists
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var usersDto = _mapper.Map<List<ResponseUserDTO>>(pagedUsers);

            for (int i = 0; i < pagedUsers.Count; i++)
            {
                var user = pagedUsers[i];
                var roles = await _userManager.GetRolesAsync(user);
                usersDto[i].Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();
            }

            return new PaginatedList<ResponseUserDTO>(usersDto, totalCount, pageIndex, pageSize);
        }
        public async Task<bool> ApproveArtistAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null || user.IsDeleted)
                throw new NotFoundException("user_not_found", "Người dùng không tồn tại.");

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Artist"))
                throw new BadRequestException("not_artist", "Người dùng không phải là đối tác (Artist).");

            if (user.isApproved)
                throw new BadRequestException("already_approved", "Người dùng đã được duyệt.");

            user.isApproved = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            return true;
        }

        public async Task<string> UploadCurrentUserAvatarAsync(IFormFile avatarFile)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                throw new AuthenticationException("Vui lòng đăng nhập trước.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                throw new ArgumentException("Không tìm thấy người dùng.");

            // Optionally delete old avatar
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var publicId = Path.GetFileNameWithoutExtension(new Uri(user.ProfilePicture).AbsolutePath);
                await _imageService.DeleteImageAsync(publicId);
            }

            using var stream = avatarFile.OpenReadStream();
            var imageUrl = await _imageService.UploadImageAsync(stream, avatarFile.FileName);

            user.ProfilePicture = imageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            return imageUrl;
        }

    }
}
