using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        //public string? FcmToken { get; set; }
        //public DateTime DateOfBirth { get; set; }
    }
}
