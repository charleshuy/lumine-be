﻿using Domain.Base;

namespace Domain.Entities
{
    public class ServiceType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation Property
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
