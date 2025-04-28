using Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Booking : BaseEntity
    {
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Deposit { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Foreign Keys
        public Guid ServiceID { get; set; }
        public Service? Service { get; set; }

        public Guid CustomerID { get; set; }

        [ForeignKey("CustomerID")]
        public ApplicationUser? Customer { get; set; }  // Navigation Property

        public Guid PaymentID { get; set; }
        public Payment? Payment { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Canceled
    }
}
