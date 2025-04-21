using Domain.Base;

namespace Domain.Entities
{
    public class Review : BaseEntity
    {
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreateDate { get; set; }
        public ReviewStatus Status { get; set; }

        // Foreign Keys
        public Guid CustomerID { get; set; }
        public ApplicationUser? Customer { get; set; }  // Navigation Property

        public Guid ServiceID { get; set; }
        public Service? Service { get; set; }
    }

    public enum ReviewStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
