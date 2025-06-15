using Application.Interfaces.Auth;
using Application.Interfaces.Services;
using Application.Services;
using Application.Services.Auth;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AutoMapper
            services.AddAutoMapper(typeof(DependencyInjection));

            // Add Redis
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisHost = configuration["Redis:Host"];
                var redisPort = configuration["Redis:Port"];
                var redisPassword = configuration["Redis:Password"];

                var options = new ConfigurationOptions
                {
                    EndPoints = { $"{redisHost}:{redisPort}" },
                    Password = redisPassword,
                    AbortOnConnectFail = false
                };

                return ConnectionMultiplexer.Connect(options);
            });

            // Add Cloundinary
            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var cloudinarySettings = config.GetSection("CloudinarySettings").Get<CloudinarySettings>();

                return new Cloudinary(new Account(
                    cloudinarySettings!.CloudName,
                    cloudinarySettings.ApiKey,
                    cloudinarySettings.ApiSecret
                ));
            });

            // Register services
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IServiceTypeService, ServiceTypeService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IAppOverviewService, AppOverviewService>();
            return services;
        }

        public class CloudinarySettings
        {
            public string? CloudName { get; set; }
            public string? ApiKey { get; set; }
            public string? ApiSecret { get; set; }
        }
    }
}
