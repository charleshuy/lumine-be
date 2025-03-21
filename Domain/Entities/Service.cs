﻿using Domain.Base;

namespace Domain.Entities
{
    public class Service : BaseEntity
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }  // TIME in SQL maps to TimeSpan in C#
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; }  // Enum for Status

        // Foreign Key
        public Guid ArtistID { get; set; }

        // Navigation Property for Reviews
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public enum ServiceStatus
    {
        Available,
        Unavailable,
        Discontinued
    }
}
