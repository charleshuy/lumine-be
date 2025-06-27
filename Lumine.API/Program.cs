using Infrastructure.Seeds;
using Lumine.API;
using Lumine.API.Middlewares;

/// <summary>
/// The entry point of the Lumine API application.
/// </summary>
public class Program
{
    /// <summary>
    /// The main method that starts the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // ✅ Add Swagger
        builder.Services.AddEndpointsApiExplorer();

        // Add custom services
        builder.Services.AddConfig(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy => policy.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader());
        });

        builder.Services.AddHttpClient("locations", client =>
        {
            client.BaseAddress = new Uri("https://provinces.open-api.vn/api/");
        });

        var app = builder.Build();

        // Seed data
        //using (var scope = app.Services.CreateScope())
        //{
        //    var services = scope.ServiceProvider;
        //    await Seed.Initialize(services);
        //}

        // Seed dLocation data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await LocationSeed.SeedLocation(services);
        }



        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lumine API v1");
            });
        }


        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
