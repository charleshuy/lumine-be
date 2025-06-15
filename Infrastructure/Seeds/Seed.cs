using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Domain.Entities;

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
            await SeedServices(context, adminUser);
            //await SeedPayments(context);
            await SeedBookings(context, adminUser);
            await SeedReviews(context, adminUser);
            //await SeedOrders(context, adminUser);
        }

        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "User", "Artist" };
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
            var adminEmail = "admin@lumine.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (!result.Succeeded)
                    throw new Exception("Failed to create admin user");

                await userManager.AddToRoleAsync(adminUser, "Admin");

                // 🚨 Fetch the user again to get the DB-generated ID (ensures tracking is updated)
                adminUser = await userManager.FindByEmailAsync(adminEmail);
            }

            return adminUser!;
        }

        private static async Task<List<ServiceType>> SeedServiceTypes(ApplicationDbContext context)
        {
            if (!context.ServiceTypes.Any())
            {
                var types = new List<ServiceType>
        {
            new ServiceType { Name = "Hair", Description = "Hair-related services" },
            new ServiceType { Name = "Spa", Description = "Relaxation and massage services" },
            new ServiceType { Name = "Nails", Description = "Manicure and pedicure services" }
        };

                context.ServiceTypes.AddRange(types);
                await context.SaveChangesAsync();
            }

            return context.ServiceTypes.ToList(); // Ensure we return the tracked entities
        }


        private static async Task SeedServices(ApplicationDbContext context, ApplicationUser adminUser)
        {
            if (!context.Services.Any())
            {
                var serviceTypes = await SeedServiceTypes(context);

                var hairType = serviceTypes.First(st => st.Name == "Hair");
                var spaType = serviceTypes.First(st => st.Name == "Spa");
                var nailType = serviceTypes.First(st => st.Name == "Nails");

                context.Services.AddRange(
                    new Service
                    {
                        ServiceName = "Haircut",
                        Price = 20.00m,
                        ArtistID = adminUser.Id,
                        Duration = TimeSpan.FromMinutes(30),
                        Status = ServiceStatus.Available,
                        ServiceTypeID = hairType.Id
                    },
                    new Service
                    {
                        ServiceName = "Massage",
                        Price = 50.00m,
                        ArtistID = adminUser.Id,
                        Duration = TimeSpan.FromMinutes(60),
                        Status = ServiceStatus.Available,
                        ServiceTypeID = spaType.Id
                    },
                    new Service
                    {
                        ServiceName = "Manicure",
                        Price = 30.00m,
                        ArtistID = adminUser.Id,
                        Duration = TimeSpan.FromMinutes(45),
                        Status = ServiceStatus.Available,
                        ServiceTypeID = nailType.Id
                    }
                );

                await context.SaveChangesAsync();
            }
        }



        //private static async Task SeedPayments(ApplicationDbContext context)
        //{
        //    if (!context.Payments.Any())
        //    {
        //        context.Payments.AddRange(
        //            new Payment { PaymentMethod = PaymentMethod.Cash, Status = PaymentStatus.Completed},
        //            new Payment { PaymentMethod = PaymentMethod.PayPal, Status = PaymentStatus.Completed}
        //        );
        //        await context.SaveChangesAsync();
        //    }
        //}

        private static async Task SeedBookings(ApplicationDbContext context, ApplicationUser adminUser)
        {
            if (!context.Bookings.Any())
            {
                var haircut = context.Services.First(s => s.ServiceName == "Haircut");

                var booking = new Booking
                {
                    ServiceID = haircut.Id,
                    CustomerID = adminUser.Id,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = 20.00m,
                    Deposit = 5.00m,
                    Status = BookingStatus.Confirmed,
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(1.5)
                };

                context.Bookings.Add(booking);
                await context.SaveChangesAsync();

                var payment = new Payment
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed,
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice
                };

                context.Payments.Add(payment);
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

        //private static async Task SeedOrders(ApplicationDbContext context, ApplicationUser adminUser)
        //{
        //    if (!context.Orders.Any())
        //    {
        //        var order = new Order
        //        {
        //            CustomerID = adminUser.Id,
        //            TotalPrice = 50.00m,
        //            OrderDate = DateTime.UtcNow,
        //            OrderStatus = OrderStatus.Pending
        //        };

        //        context.Orders.Add(order);
        //        await context.SaveChangesAsync();

        //        var payment = new Payment
        //        {
        //            PaymentMethod = PaymentMethod.PayPal,
        //            Status = PaymentStatus.Completed,
        //            OrderId = order.Id,
        //            Amount = order.TotalPrice
        //        };

        //        context.Payments.Add(payment);
        //        await context.SaveChangesAsync();
        //    }
        //}

    }
}
