using Application.DTOs;
using Application.DTOs.SearchFilters;
using Application.DTOs.UserDTO;
using Application.Paggings;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<PaginatedList<ResponseUserDTO>> GetPaginatedUsers(
            int pageIndex,
            int pageSize,
            UserSearchFilterDTO? filter = null);
        Task<PaginatedList<ResponseUserDTO>> GetNearbyArtistsAsync(int pageIndex, int pageSize);
        Task<List<ResponseUserDTO>> GetAllUsersAsync();
        Task<List<CreatedSummaryDTO>> GetUsersCreatedSummaryAsync();
        Task<ResponseUserDTO?> GetUserByIdAsync(Guid userId);
        Task<ResponseUserDTO?> GetCurrentUserAsync();
        Task<bool> UpdateUserAsync(Guid userId, UpdateUserDTO dto);
        Task<bool> UpdateCurrentUserProfileAsync(UpdateProfileDTO dto);
        Task<bool> DeleteUserAsync(Guid userId);
        Guid? GetCurrentUserId();
        Task<PaginatedList<ResponseUserDTO>> GetUnapprovedArtistsAsync(int pageIndex, int pageSize);
        Task<bool> ApproveArtistAsync(Guid userId);
        Task<string> UploadCurrentUserAvatarAsync(IFormFile avatarFile);
        Task RateArtistAsync(RatingRequestDTO dto);
        Task<PaginatedList<RatingDTO>> GetArtistRatingsAsync(Guid artistId, int pageIndex, int pageSize);
        Task<bool> CheckAdminRole(string userId);
        Task<IEnumerable<UserWeeklyStatsDto>> GetWeeklyUserStatsFromJuneAsync();
    }
}
