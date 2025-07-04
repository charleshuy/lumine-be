﻿using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FcmToken { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool isApproved { get; set; } = true;
        public string? ProfilePicture { get; set; }


        // ✅ Location
        public Guid? DistrictId { get; set; }
        public District? District { get; set; }

        // ✅ Rating fields
        public double Rating { get; set; } = 0.0;          // Average rating (0.0 - 5.0)
        public int RatingCount { get; set; } = 0;          // Total number of ratings
    }

}
