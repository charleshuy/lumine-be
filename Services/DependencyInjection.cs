using Application.Interfaces.Auth;
using Application.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {

            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
            return services;
        }
    }
}
