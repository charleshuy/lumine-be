using Infrastructure;
using Infrastructure.Identities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Lumine.API
{
    public static class DependencyInjection
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructureLayer(configuration);
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }
    }
}
