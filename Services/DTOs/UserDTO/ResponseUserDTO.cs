﻿

namespace Application.DTOs.UserDTO
{
    public class ResponseUserDTO
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ProfilePicture { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public double Rating { get; set; } = 0.0;
        public string Description { get; set; } = "N/A";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<RoleDTO>? Roles { get; set; }
        public UserDistrictDto? District { get; set; }
    }

    public class UserDistrictDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public ProvinceDto? Province { get; set; }
    }

    public class CreatedSummaryDTO
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class RoleDTO
    {
        public string? Name { get; set; }
        //public string? Description { get; set; }
    }
}
