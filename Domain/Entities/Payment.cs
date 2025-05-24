using Domain.Base;

namespace Domain.Entities
{
    public class Payment : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentType PaymentType { get; set; }
        public string? TransactionId { get; set; }
        public string? Gateway { get; set; } // e.g., Stripe, PayPal
        public string? FailureReason { get; set; }

        // Foreign Key
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public Guid? OrderId { get; set; } // nullable to support shared Payment table with Booking
        public Order? Order { get; set; }

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
        Failed,
        Refunded
    }

    public enum PaymentType
    {
        Deposit,
        Balance,
        Refund
    }


}
