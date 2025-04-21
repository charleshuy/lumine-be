using Domain.Base;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public Guid PaymentID { get; set; }
        public Payment? Payment { get; set; }

        public Guid CustomerID { get; set; }
        public ApplicationUser? Customer { get; set; }  // Navigation Property
    }

    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Canceled
    }
}
