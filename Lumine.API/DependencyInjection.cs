using Application;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Lumine.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration configuration)
        {


            // Register Layer
            services
                .AddApplicationLayer()
                .AddInfrastructureLayer(configuration)
                .AddCustomAuthentication(configuration);

            // Register Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
