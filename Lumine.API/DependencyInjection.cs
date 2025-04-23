using Application;
using Domain.Entities;
using FirebaseAdmin.Auth;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Lumine.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            // Register Layer
            services
                .AddApplicationLayer(configuration)
                .AddInfrastructureLayer(configuration)
                .AddCustomAuthentication(configuration);

            // Register Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add Swagger Configuration
            services.AddSwaggerDocumentation();

            return services;
        }

        /// <summary>
        /// Configures Swagger documentation for the API.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        private static void AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "The Coffee Hand API",
                    Version = "v1",
                    Description = "API for The Coffee Hand application"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter '{your Firebase ID token}'"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });

                // Enable XML Comments (for Swagger API Documentation)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                options.IncludeXmlComments(xmlPath);
            });
        }
    }
}
