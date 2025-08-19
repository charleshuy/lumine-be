using Application.DTOs;
using Application.DTOs.SearchFilters;
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
            IImageService imageService)
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
            var query = _unitOfWork.GetRepository<ApplicationUser>().Entities.Include(u => u.District).ThenInclude(d => d.Province)
                .Where(u => !u.IsDeleted);

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Username))
                    query = query.Where(u => u.UserName!.Contains(filter.Username));

                if (!string.IsNullOrWhiteSpace(filter.Email))
                    query = query.Where(u => u.Email!.Contains(filter.Email));

                if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
                    query = query.Where(u => u.PhoneNumber!.Contains(filter.PhoneNumber));

                if (filter.DistrictId.HasValue)
                    query = query.Where(u => u.DistrictId == filter.DistrictId.Value);
                if (filter.ProvinceId.HasValue)
                    query = query.Where(u => u.District!.ProvinceId == filter.ProvinceId.Value);
            }

            query = query.OrderBy(u => u.Email);

            // Fetch all users to apply role filter if needed
            var users = await query.ToListAsync();

            if (filter != null && !string.IsNullOrWhiteSpace(filter.Role))
            {
                users = await FilterUsersByRoleAsync(users, filter.Role);
            }

            var totalCount = users.Count;

            // Paginate manually after role filtering
            var paginatedUsers = users
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var usersDto = _mapper.Map<List<ResponseUserDTO>>(paginatedUsers);

            for (int i = 0; i < paginatedUsers.Count; i++)
            {
                var roles = await _userManager.GetRolesAsync(paginatedUsers[i]);
                usersDto[i].Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();
            }

            return new PaginatedList<ResponseUserDTO>(
                usersDto,
                totalCount,
                pageIndex,
                pageSize
            );
        }

        public async Task<PaginatedList<ResponseUserDTO>> GetNearbyArtistsAsync(int pageIndex, int pageSize)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                throw new AuthenticationException("Vui lòng đăng nhập trước.");

            var currentUser = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Include(u => u.District)
                .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(u => u.Id == currentUserId && !u.IsDeleted);

            if (currentUser?.District == null || currentUser.District.Province == null)
                throw new InvalidOperationException("Không tìm thấy khu vực của người dùng hiện tại.");

            var districtId = currentUser.DistrictId;
            var provinceId = currentUser.District.ProvinceId;

            var usersQuery = _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Include(u => u.District)
                .ThenInclude(d => d.Province)
                .Where(u => !u.IsDeleted && u.isApproved && u.Id != currentUserId);

            // Same district first
            var nearbyUsers = await usersQuery
                .Where(u => u.DistrictId == districtId)
                .ToListAsync();

            // Fallback to same province
            if (!nearbyUsers.Any())
            {
                nearbyUsers = await usersQuery
                    .Where(u => u.District!.ProvinceId == provinceId)
                    .ToListAsync();
            }

            // ✅ Reuse your helper
            var nearbyArtists = await FilterUsersByRoleAsync(nearbyUsers, "Artist");

            var totalCount = nearbyArtists.Count;

            var paged = nearbyArtists
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtoList = _mapper.Map<List<ResponseUserDTO>>(paged);

            for (int i = 0; i < paged.Count; i++)
            {
                var roles = await _userManager.GetRolesAsync(paged[i]);
                dtoList[i].Roles = roles.Select(r => new RoleDTO { Name = r }).ToList();
            }

            return new PaginatedList<ResponseUserDTO>(dtoList, totalCount, pageIndex, pageSize);
        }

        private async Task<List<ApplicationUser>> FilterUsersByRoleAsync(List<ApplicationUser> users, string role)
        {
            var result = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(user);
                }
            }

            return result;
        }

        public async Task<List<ResponseUserDTO>> GetAllUsersAsync()
        {
            var users = _unitOfWork.GetRepository<ApplicationUser>().Entities.Include(u => u.District).ThenInclude(d => d.Province)
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
            // Use EF Core with Includes instead of _userManager.FindByIdAsync
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .NoTrackingEntities
                .Include(u => u.District)
                .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
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

            // Parse the userIdClaim (assuming it's a GUID)
            if (!Guid.TryParse(userIdClaim, out var userId))
                return null;

            // Use UoW + EF to fetch with includes
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .NoTrackingEntities
                .Include(u => u.District)
                .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
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
                user.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                user.Description = dto.Description;

            if (dto.DistrictId.HasValue)
                user.DistrictId = dto.DistrictId.Value;

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

        public async Task<bool> CheckAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                throw new NotFoundException("user_not_found", "Người dùng không tồn tại.");
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return true;
            else
                throw new BadRequestException("not_admin", "Người dùng không phải là Admin.");
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

        public async Task RateArtistAsync(RatingRequestDTO dto)
        {
            if (dto.Rating < 0 || dto.Rating > 5)
                throw new ArgumentException("Rating must be between 0 and 5");

            var customerId = GetCurrentUserId() ?? throw new AuthenticationException("Please log in");

            // Check if the customer has completed a booking with this artist
            var hasBooked = await _unitOfWork.GetRepository<Booking>().Entities
                .AnyAsync(b =>
                    b.Service!.ArtistID == dto.ArtistId &&
                    b.CustomerID == customerId &&
                    b.Status == BookingStatus.Completed);

            if (!hasBooked)
                throw new InvalidOperationException("You can only rate artists you've booked services with.");

            var userRatingRepo = _unitOfWork.GetRepository<UserRating>();

            var existingRating = await userRatingRepo.Entities
                .FirstOrDefaultAsync(r => r.ArtistId == dto.ArtistId && r.CustomerId == customerId);

            var artist = await _userManager.FindByIdAsync(dto.ArtistId.ToString());
            if (artist == null || artist.IsDeleted)
                throw new Exception("Artist not found");

            if (existingRating == null)
            {
                // First time rating
                var newRatingEntry = new UserRating
                {
                    Id = Guid.NewGuid(),
                    ArtistId = dto.ArtistId,
                    CustomerId = customerId,
                    Rating = dto.Rating,
                    Review = dto.Review,
                    RatedAt = DateTime.UtcNow
                };

                await userRatingRepo.InsertAsync(newRatingEntry);

                artist.Rating = ((artist.Rating * artist.RatingCount) + dto.Rating) / (artist.RatingCount + 1);
                artist.RatingCount += 1;
            }
            else
            {
                // Update existing rating
                var totalRating = (artist.Rating * artist.RatingCount) - existingRating.Rating + dto.Rating;

                existingRating.Rating = dto.Rating;
                existingRating.Review = dto.Review;
                existingRating.RatedAt = DateTime.UtcNow;

                artist.Rating = totalRating / artist.RatingCount;
            }

            artist.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
            await _userManager.UpdateAsync(artist);
        }

        public async Task<PaginatedList<RatingDTO>> GetArtistRatingsAsync(Guid artistId, int pageIndex, int pageSize)
        {
            var query = _unitOfWork.GetRepository<UserRating>().Entities
                .Where(r => r.ArtistId == artistId)
                .OrderByDescending(r => r.RatedAt);

            var totalCount = await query.CountAsync();

            var ratings = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RatingDTO
                {
                    Id = r.Id,
                    ArtistId = r.ArtistId,
                    CustomerId = r.CustomerId,
                    Review = r.Review,
                    Rating = r.Rating,
                    RatedAt = r.RatedAt
                })
                .ToListAsync();

            return new PaginatedList<RatingDTO>(ratings, totalCount, pageIndex, pageSize);
        }


        public async Task<IEnumerable<UserWeeklyStatsDto>> GetWeeklyUserStatsFromJuneAsync()
        {
            var query = from user in _userManager.Users
                        where user.CreatedAt.Month >= 6 && user.CreatedAt.Year == DateTime.UtcNow.Year
                        select new
                        {
                            user.Id,
                            user.CreatedAt
                        };

            var userList = await query.ToListAsync();

            // Get roles in bulk
            var result = new List<UserWeeklyStatsDto>();
            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(user.Id.ToString()));

                foreach (var role in roles)
                {
                    var year = user.CreatedAt.Year;
                    var month = user.CreatedAt.Month;
                    var weekNumber = System.Globalization.CultureInfo.InvariantCulture
                                     .Calendar.GetWeekOfYear(user.CreatedAt,
                                                              System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                                                              DayOfWeek.Monday);

                    result.Add(new UserWeeklyStatsDto
                    {
                        Year = year,
                        Month = month,
                        WeekNumber = weekNumber,
                        RoleName = role,
                        UserCount = 1
                    });
                }
            }

            // Group and sum counts
            return result
                .GroupBy(x => new { x.Year, x.Month, x.WeekNumber, x.RoleName })
                .Select(g => new UserWeeklyStatsDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    WeekNumber = g.Key.WeekNumber,
                    RoleName = g.Key.RoleName,
                    UserCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.WeekNumber)
                .ToList();
        }

    }
}
