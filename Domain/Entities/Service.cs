using Domain.Base;

namespace Domain.Entities
{
    public class Service : BaseEntity
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }  // TIME in SQL maps to TimeSpan in C#
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; }

        // Foreign Key
        public Guid? ArtistID { get; set; }
        public Guid? ServiceTypeID { get; set; }



        // Navigation Properties
        public ServiceType? ServiceType { get; set; }
        public ApplicationUser? Artist { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public enum ServiceStatus
    {
        Unavailable,
        Available,
        Discontinued
    }
}
