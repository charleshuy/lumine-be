using Domain.Base;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public Guid CustomerID { get; set; }
        public ApplicationUser? Customer { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        //public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal TotalPaid => Payments?
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount) ?? 0;

        public bool IsPaid => TotalPaid >= TotalPrice;
    }


    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Canceled
    }
}
