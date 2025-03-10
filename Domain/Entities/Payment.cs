using Domain.Base;

namespace Domain.Entities
{
    public class Payment : BaseEntity
    {
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }

        // Navigation Properties
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public enum PaymentMethod
    {
        CreditCard,
        PayPal,
        BankTransfer,
        Cash
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }
}
