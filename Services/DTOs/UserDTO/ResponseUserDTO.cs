

namespace Application.DTOs.UserDTO
{
    public class ResponseUserDTO
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<RoleDTO>? Roles { get; set; }
    }


    public class RoleDTO
    {
        public string? Name { get; set; }
        //public string? Description { get; set; }
    }
}
