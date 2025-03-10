using Domain.Base;

namespace Domain.Entities
{
    public class Booking : BaseEntity
    {
        public DateTime BookingDate { get; set; }  // DECIMAL should be DateTime
        public decimal TotalPrice { get; set; }
        public decimal Deposit { get; set; }
        public BookingStatus Status { get; set; }

        // Foreign Keys
        public Guid ServiceID { get; set; }
        public Service? Service { get; set; }  // Navigation Property

        public Guid CustomerID { get; set; }

        public Guid PaymentID { get; set; }
        public Payment? Payment { get; set; }  // Navigation Property
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Canceled
    }
}
