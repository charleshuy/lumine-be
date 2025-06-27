

namespace Application.DTOs.SearchFilters
{
    public class UserSearchFilterDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? ProvinceId { get; set; }
    }

}
