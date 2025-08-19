

using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.Payment
{
    public interface ISubscriptionVNPayService
    {
        Task<string> CreatePaymentUrl(SubscriptionVNPayRequestDTO request);

        Task<VNPayCallbackResultDTO> ProcessVNPayCallback(IQueryCollection query);
    }
}
