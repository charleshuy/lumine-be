using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Domain.Entities;
using Infrastructure.Identity;

namespace Infrastructure.Seeds
{
    public static class Seed
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedRoles(roleManager);
            var adminUser = await SeedAdminUser(userManager);
            await SeedServices(context);
            await SeedPayments(context);
            await SeedBookings(context, adminUser);
            await SeedReviews(context, adminUser);
            await SeedOrders(context, adminUser);
        }

        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }

        private static async Task<ApplicationUser> SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    //DateOfBirth = new DateTime(1990, 1, 1)
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            return adminUser;
        }

        private static async Task SeedServices(ApplicationDbContext context)
        {
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { ServiceName = "Haircut", Price = 20.00m },
                    new Service { ServiceName = "Massage", Price = 50.00m },
                    new Service { ServiceName = "Manicure", Price = 30.00m }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPayments(ApplicationDbContext context)
        {
            if (!context.Payments.Any())
            {
                context.Payments.AddRange(
                    new Payment { PaymentMethod = PaymentMethod.Cash, Status = PaymentStatus.Completed},
                    new Payment { PaymentMethod = PaymentMethod.PayPal, Status = PaymentStatus.Completed}
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBookings(ApplicationDbContext context, ApplicationUser adminUser)
        {
            if (!context.Bookings.Any())
            {
                var haircut = context.Services.First(s => s.ServiceName == "Haircut");
                var payment = context.Payments.First();

                context.Bookings.Add(new Booking
                {
                    ServiceID = haircut.Id,
                    CustomerID = adminUser.Id,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = 20.00m,
                    Deposit = 5.00m,
                    PaymentID = payment.Id
                });

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedReviews(ApplicationDbContext context, ApplicationUser adminUser)
        {
            if (!context.Reviews.Any())
            {
                var haircut = context.Services.First(s => s.ServiceName == "Haircut");

                context.Reviews.Add(new Review
                {
                    ServiceID = haircut.Id,
                    CustomerID = adminUser.Id,
                    Rating = 5,
                    Comment = "Excellent service!",
                    CreateDate = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedOrders(ApplicationDbContext context, ApplicationUser adminUser)
        {
            if (!context.Orders.Any())
            {
                var payment = context.Payments.First();

                context.Orders.Add(new Order
                {
                    CustomerID = adminUser.Id,
                    TotalPrice = 50.00m,
                    OrderDate = DateTime.UtcNow,
                    PaymentID = payment.Id
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
