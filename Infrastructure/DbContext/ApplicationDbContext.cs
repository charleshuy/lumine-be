using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<ServiceType> ServiceTypes { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Province> Provinces { get; set; } = null!;
        public DbSet<District> Districts { get; set; } = null!;
        public DbSet<UserRating> UserRatings { get; set; }
        public DbSet<SubscriptionTier> SubscriptionTiers { get; set; } = null!;
        public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Rename Identity tables
            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens");

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceID);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Booking>()
            //    .HasOne(b => b.Payment)
            //    .WithMany(p => p.Bookings)
            //    .HasForeignKey(b => b.PaymentID);

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.Deposit)
                .HasColumnType("decimal(18,2)");

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Service)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.ServiceID);

            // Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Order>()
            //    .HasOne(o => o.Payment)
            //    .WithMany(p => p.Orders)
            //    .HasForeignKey(o => o.PaymentID);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // Service
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Artist)
                .WithMany()
                .HasForeignKey(s => s.ArtistID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Service>()
                .HasOne(s => s.ServiceType)
                .WithMany(st => st.Services)
                .HasForeignKey(s => s.ServiceTypeID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Province - District: One-to-Many
            modelBuilder.Entity<District>()
                .HasOne(d => d.Province)
                .WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            // District - User: One-to-Many
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.District)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DistrictId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserRating>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.Artist)
                    .WithMany()
                    .HasForeignKey(r => r.ArtistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => new { r.ArtistId, r.CustomerId })
                    .IsUnique();

                entity.Property(r => r.Rating)
                    .HasColumnType("float");
            });

            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.HasKey(us => us.Id);

                entity.HasOne(us => us.User)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(us => us.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(us => us.SubscriptionTier)
                    .WithMany()
                    .HasForeignKey(us => us.SubscriptionTierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(us => us.StartDate)
                    .IsRequired();

                entity.Property(us => us.EndDate)
                    .IsRequired();
            });
        }
    }
}
