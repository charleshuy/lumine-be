using Application.DTOs;
using Application.Helper;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UOW;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Services.Payment
{
    public class SubscriptionVNPayService : ISubscriptionVNPayService
    {
        private readonly VNPaySettings _settings;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionVNPayService(IOptions<VNPaySettings> options, IUserService userService, IUnitOfWork unitOfWork)
        {
            _settings = options.Value;
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreatePaymentUrl(SubscriptionVNPayRequestDTO request)
        {
            var userId = _userService.GetCurrentUserId();

            if (userId == null)
                throw new UnauthorizedAccessException("User is not authenticated");

            var tick = DateTime.Now.Ticks.ToString();
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVN = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

            var vnp_Params = new Dictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _settings.TmnCode },
            { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
            { "vnp_CurrCode", "VND" },
            { "vnp_TxnRef", tick },
            { "vnp_OrderInfo", request.Description },
            { "vnp_OrderType", request.OrderType ?? "other" },
            { "vnp_Locale", request.Locale ?? "vn" },
            { "vnp_ReturnUrl", _settings.ReturnUrl },
            { "vnp_IpAddr", request.IpAddress ?? "127.0.0.1" },
            { "vnp_CreateDate", nowVN.ToString("yyyyMMddHHmmss") },
            { "vnp_ExpireDate", nowVN.AddMinutes(15).ToString("yyyyMMddHHmmss") }
        };

            // Optional Billing Info
            if (!string.IsNullOrWhiteSpace(request.BillingEmail))
                vnp_Params["vnp_Bill_Email"] = request.BillingEmail;
            if (!string.IsNullOrWhiteSpace(request.BillingMobile))
                vnp_Params["vnp_Bill_Mobile"] = request.BillingMobile;
            if (!string.IsNullOrWhiteSpace(request.BillingFullName))
            {
                var names = request.BillingFullName.Trim().Split(' ');
                vnp_Params["vnp_Bill_FirstName"] = names[0];
                if (names.Length > 1)
                    vnp_Params["vnp_Bill_LastName"] = string.Join(" ", names.Skip(1));
            }

            // 👇 Log everything you’re sending to VNPay
            //Console.WriteLine("==== VNPay Parameters ====");
            //foreach (var param in vnp_Params)
            //{
            //    Console.WriteLine($"{param.Key} = {param.Value}");
            //}

            var url = VNPayHelper.BuildPaymentUrl(vnp_Params, _settings.BaseUrl, _settings.HashSecret);

            //Console.WriteLine("==== VNPay Payment URL ====");
            //Console.WriteLine(url);

            var tier = _unitOfWork.GetRepository<SubscriptionTier>().GetById(request.TierId);

            var subscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = (Guid)userId,
                SubscriptionTierId = request.TierId,
                StartDate = new DateTime(long.Parse(tick)),
                EndDate = new DateTime(long.Parse(tick)).AddDays(tier.DurationInDays)
            };

            await _unitOfWork.GetRepository<UserSubscription>().InsertAsync(subscription);
            await _unitOfWork.SaveAsync();

            return url;
        }

        public async Task<VNPayCallbackResultDTO> ProcessVNPayCallback(IQueryCollection query)
        {
            var vnpay = new VNPayHelper();

            foreach (var pair in query)
            {
                if (!string.IsNullOrEmpty(pair.Key) && pair.Key.StartsWith("vnp_"))
                    vnpay.AddResponseData(pair.Key, pair.Value.ToString());
            }

            var secureHash = query["vnp_SecureHash"];
            var isValid = vnpay.ValidateSignature(secureHash, _settings.HashSecret);

            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var orderId = vnpay.GetResponseData("vnp_TxnRef");
            var amount = vnpay.GetResponseData("vnp_Amount");

            if (!isValid)
            {
                return new VNPayCallbackResultDTO
                {
                    Success = false,
                    OrderId = orderId,
                    Amount = amount,
                    ResponseCode = responseCode
                };
            }

            if (responseCode == "00")
            {
                // TODO: Activate the subscription, store payment info, etc.
                // await _unitOfWork.SubscriptionRepository.ActivateByOrderId(orderId);

                return new VNPayCallbackResultDTO
                {
                    Success = true,
                    OrderId = orderId,
                    Amount = amount,
                    ResponseCode = responseCode
                };
            }

            return new VNPayCallbackResultDTO
            {
                Success = false,
                OrderId = orderId,
                Amount = amount,
                ResponseCode = responseCode
            };
        }
    }

}
