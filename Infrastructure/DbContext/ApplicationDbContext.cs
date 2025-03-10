using Domain.Entities;
using Infrastructure.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets for domain entities
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensures Identity tables are configured

            // Rename Identity tables if needed
            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens");

            // Booking relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceID);

            modelBuilder.Entity<Booking>()
                .HasOne<ApplicationUser>()  // ✅ Correct FK to Identity User
                .WithMany()
                .HasForeignKey(b => b.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PaymentID);

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.Deposit)
                .HasColumnType("decimal(18,2)");


            // Review relationships
            modelBuilder.Entity<Review>()
                .HasOne<ApplicationUser>()  // ✅ Correct FK to Identity User
                .WithMany()
                .HasForeignKey(r => r.CustomerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Service)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.ServiceID);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasOne<ApplicationUser>()  // ✅ Correct FK to Identity User
                .WithMany()
                .HasForeignKey(o => o.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PaymentID);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");


            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");
        }

    }
}
