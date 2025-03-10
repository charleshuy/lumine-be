using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }

    }
}
