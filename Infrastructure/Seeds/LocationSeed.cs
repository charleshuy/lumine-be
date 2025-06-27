using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Infrastructure.Seeds
{
    public static class LocationSeed
    {
        public static async Task SeedLocation(IServiceProvider serviceProvider)
        {
            var ctx = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var http = serviceProvider.GetRequiredService<IHttpClientFactory>();

            await SeedAsync(ctx, http);
        }


        public static async Task SeedAsync(ApplicationDbContext context, IHttpClientFactory httpFactory)
        {
            if (await context.Provinces.AnyAsync()) return;

            var client = httpFactory.CreateClient("locations");
            var response = await client.GetAsync("?depth=2");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<ProvinceDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new List<ProvinceDto>();

            foreach (var p in data)
            {
                var province = new Province { Name = p.Name };
                foreach (var d in p.Districts ?? new List<DistrictDto>())
                {
                    province.Districts.Add(new District { Name = d.Name });
                }
                context.Provinces.Add(province);
            }

            await context.SaveChangesAsync();
        }

        private class ProvinceDto
        {
            public string Name { get; set; } = "";
            public List<DistrictDto>? Districts { get; set; }
        }

        private class DistrictDto
        {
            public string Name { get; set; } = "";
        }
    }
}
