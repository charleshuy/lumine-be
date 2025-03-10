using Infrastructure.Seeds;
using Lumine.API;
using Microsoft.OpenApi.Models;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // ✅ Add Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Lumine API",
                Version = "v1",
                Description = "API documentation for Lumine",
            });
        });

        // Add custom services (if any)
        builder.Services.AddServices(builder.Configuration);

        var app = builder.Build();

        // Seed data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await Seed.Initialize(services);
        }

        // ✅ Enable Swagger in Development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lumine API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
