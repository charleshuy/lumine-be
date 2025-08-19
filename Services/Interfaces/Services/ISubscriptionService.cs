using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionTierDTO>> GetAllTiersAsync();
        Task<bool> SubscribeAsync(SubscribeRequestDTO request);
        Task<UserSubscription?> GetCurrentUserActiveSubscriptionAsync();
        Task<bool> HasActiveSubscriptionAsync();
        Task<bool> ActivateSubscriptionFromVNPayAsync(string orderId, decimal amount);
        Task<SubscriptionTierDTO> AddSubscriptionTierAsync(CreateSubscriptionTierDTO dto);
        Task<SubscriptionTierDTO?> GetTierByIdOrNameAsync(Guid? id, string? name);

    }

}
