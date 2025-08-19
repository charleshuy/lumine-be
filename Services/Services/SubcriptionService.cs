using Application.DTOs;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Security.Claims;

namespace Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public SubscriptionService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                throw new AuthenticationException("Please log in");

            if (!Guid.TryParse(userIdStr, out var userId))
                throw new AuthenticationException("Invalid user identifier");

            return userId;
        }

        public async Task<List<SubscriptionTierDTO>> GetAllTiersAsync()
        {
            var tiers = await _unitOfWork.GetRepository<SubscriptionTier>().Entities.ToListAsync();
            return _mapper.Map<List<SubscriptionTierDTO>>(tiers);
        }

        public async Task<SubscriptionTierDTO?> GetTierByIdOrNameAsync(Guid? id, string? name)
        {
            var query = _unitOfWork.GetRepository<SubscriptionTier>().Entities.AsQueryable();

            if (id.HasValue)
                query = query.Where(t => t.Id == id.Value);
            else if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(t => t.Name.ToLower() == name.Trim().ToLower());
            else
                throw new ArgumentException("Either ID or Name must be provided.");

            var tier = await query.FirstOrDefaultAsync();

            return tier != null ? _mapper.Map<SubscriptionTierDTO>(tier) : null;
        }


        public async Task<bool> SubscribeAsync(SubscribeRequestDTO request)
        {
            var userId = GetCurrentUserId();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
                throw new Exception("User not found");

            var tier = await _unitOfWork.GetRepository<SubscriptionTier>().GetByIdAsync(request.TierId);
            if (tier == null)
                throw new Exception("Subscription tier not found");

            var now = DateTime.UtcNow;

            var subscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubscriptionTierId = tier.Id,
                StartDate = now,
                EndDate = now.AddDays(tier.DurationInDays)
            };

            await _unitOfWork.GetRepository<UserSubscription>().InsertAsync(subscription);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<UserSubscription?> GetCurrentUserActiveSubscriptionAsync()
        {
            var userId = GetCurrentUserId();

            var activeSub = await _unitOfWork.GetRepository<UserSubscription>().Entities
                .Include(s => s.SubscriptionTier)
                .Where(s => s.UserId == userId && s.EndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            return activeSub;
        }

        public async Task<bool> HasActiveSubscriptionAsync()
        {
            var userId = GetCurrentUserId();

            return await _unitOfWork.GetRepository<UserSubscription>().Entities
                .AnyAsync(s => s.UserId == userId && s.EndDate > DateTime.UtcNow);
        }

        public async Task<bool> ActivateSubscriptionFromVNPayAsync(string orderId, decimal amount)
        {
            var subscriptionRepo = _unitOfWork.GetRepository<UserSubscription>();

            // Parse the vnp_TxnRef back to long ticks, then Guid (if you store it that way)
            if (!long.TryParse(orderId, out var ticks))
                return false;

            var subscription = await subscriptionRepo.Entities
                .Where(s => s.StartDate.Ticks == ticks)
                .FirstOrDefaultAsync();

            if (subscription == null)
                return false;

            // Optional: validate amount matches tier
            var tier = await _unitOfWork.GetRepository<SubscriptionTier>()
                .GetByIdAsync(subscription.SubscriptionTierId);

            if (tier == null || tier.Price * 100 != amount)
                return false;

            // Save or update logic if needed
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<SubscriptionTierDTO> AddSubscriptionTierAsync(CreateSubscriptionTierDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required");

            if (dto.Price < 0)
                throw new ArgumentException("Price must be non-negative");

            if (dto.DurationInDays <= 0)
                throw new ArgumentException("Duration must be at least 1 day");

            var tier = _mapper.Map<SubscriptionTier>(dto);
            tier.Id = Guid.NewGuid();

            await _unitOfWork.GetRepository<SubscriptionTier>().InsertAsync(tier);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<SubscriptionTierDTO>(tier);
        }
    }
}
