namespace Domain.Entities
{
    public class UserSubscription
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        public Guid SubscriptionTierId { get; set; }
        public SubscriptionTier SubscriptionTier { get; set; } = default!;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }                   // Calculated from StartDate + Duration
        public bool IsActive => EndDate > DateTime.UtcNow;
    }
}
