namespace Application.DTOs
{
    public class VNPaySettings
    {
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
    }

    public class VNPayCallbackResultDTO
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
    }

    public class SubscriptionVNPayRequestDTO
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = "Subscription Payment";
        public string? Locale { get; set; } = "vn";
        public string? OrderType { get; set; } = "other";
        public string? IpAddress { get; set; }
        public string? BillingFullName { get; set; }
        public string? BillingEmail { get; set; }
        public string? BillingMobile { get; set; }
        public required Guid TierId { get; set; }
    }

}
