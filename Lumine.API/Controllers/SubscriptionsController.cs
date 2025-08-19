using Application.DTOs;
using Application.Helper;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Payment;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>  
    /// Controller for managing subscription-related operations.  
    /// </summary>  
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionVNPayService _vnPayService;
        private readonly ISubscriptionService _subscriptionService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="SubscriptionsController"/> class.  
        /// </summary>  
        /// <param name="vnPayService">The VNPay service for handling subscription payments.</param>
        /// <param name="subscriptionService"></param>  
        public SubscriptionsController(ISubscriptionVNPayService vnPayService, ISubscriptionService subscriptionService)
        {
            _vnPayService = vnPayService;
            _subscriptionService = subscriptionService;
        }

        /// <summary>  
        /// Creates a VNPay payment URL for a subscription.  
        /// </summary>  
        /// <param name="request">The subscription payment request details.</param>  
        /// <returns>A response containing the payment URL.</returns>  
        [HttpPost("pay")]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] SubscriptionVNPayRequestDTO request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            var url = await _vnPayService.CreatePaymentUrl(request);
            return Ok(new { paymentUrl = url });
        }

        /// <summary>  
        /// Handles the VNPay callback for subscription payments.  
        /// </summary>  
        /// <returns>A response indicating the result of the payment process.</returns>  
        [HttpGet("vnpay-callback")]
        public async Task<IActionResult> VNPayCallback()
        {
            var result = await _vnPayService.ProcessVNPayCallback(Request.Query);

            if (!result.Success)
                return BadRequest($"❌ Payment failed or invalid signature. Code: {result.ResponseCode}");

            return Content($"✅ Subscription activated! Order: {result.OrderId}, Amount: {result.Amount}");
        }

        /// <summary>  
        /// Retrieves all available subscription tiers.  
        /// </summary>  
        /// <returns>A list of subscription tiers.</returns>  
        [HttpGet("tiers")]
        public async Task<IActionResult> GetAllTiers()
        {
            var tiers = await _subscriptionService.GetAllTiersAsync();
            return Ok(tiers);
        }

        /// <summary>  
        /// Retrieves a subscription tier by its ID or name.  
        /// </summary>  
        /// <param name="id">The unique identifier of the subscription tier (optional).</param>  
        /// <param name="name">The name of the subscription tier (optional).</param>  
        /// <returns>A response containing the subscription tier details or a not found result.</returns>  
        [HttpGet("tier")]
        public async Task<IActionResult> GetTier([FromQuery] Guid? id, [FromQuery] string? name)
        {
            var tier = await _subscriptionService.GetTierByIdOrNameAsync(id, name);
            if (tier == null)
                return NotFound("Subscription tier not found");

            return Ok(tier);
        }


        /// <summary>  
        /// Creates a new subscription tier.  
        /// </summary>  
        /// <param name="tier">The details of the subscription tier to create.</param>  
        /// <returns>A response containing the result of the creation process.</returns>  
        [HttpPost("create-subtier")]
        public async Task<IActionResult> CreateSubscriptionTier([FromBody] CreateSubscriptionTierDTO tier)
        {
            var created = await _subscriptionService.AddSubscriptionTierAsync(tier);
            return Ok(created);
        }
    }

}
