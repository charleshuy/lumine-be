namespace Domain.Entities
{
    public class SubscriptionTier
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Free", "Premium", "Pro"
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInDays { get; set; } // e.g., 30 for monthly
        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
