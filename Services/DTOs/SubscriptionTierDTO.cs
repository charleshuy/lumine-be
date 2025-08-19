namespace Application.DTOs
{
    public class SubscriptionTierDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DurationInDays { get; set; }
    }

    public class SubscribeRequestDTO
    {
        public Guid TierId { get; set; }
    }

    public class CreateSubscriptionTierDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
    }

    public class UserSubscriptionDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TierName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
    }

}
