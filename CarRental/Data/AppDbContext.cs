using CarRental.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CarRental.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<VehiclePricingRule> VehiclePricingRules { get; set; }
        public DbSet<VehicleStatusLogs> VehicleStatusLogs { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("car_rental_official3");

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Company>().ToTable("companies");
            modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");
            modelBuilder.Entity<Vehicle>().ToTable("vehicles");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<VehiclePricingRule>().ToTable("vehicle_pricing_rules");
            modelBuilder.Entity<VehicleStatusLogs>().ToTable("vehicle_status_logs");
            modelBuilder.Entity<Maintenance>().ToTable("maintenance");
            modelBuilder.Entity<MaintenanceLog>().ToTable("maintenance_logs");
            
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<MaintenanceLog>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.MaintenanceLogs)
                .HasForeignKey(rt => rt.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<MaintenanceLog>()
                .HasOne(rt => rt.Maintenance)
                .WithMany(u => u.MaintenanceLogs)
                .HasForeignKey(rt => rt.MaintenanceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<VehiclePricingRule>()
                .HasOne(rt => rt.Vehicle)
                .WithMany(u => u.VehiclePricingRules)
                .HasForeignKey(rt => rt.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<VehicleStatusLogs>()
                .HasOne(rt => rt.Vehicle)
                .WithMany(u => u.VehicleStatusLogs)
                .HasForeignKey(rt => rt.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Maintenance>()
                .HasOne(rt => rt.Vehicle)
                .WithMany(u => u.Maintenances)
                .HasForeignKey(rt => rt.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<VehicleStatusLogs>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.VehicleStatusLogs)
                .HasForeignKey(rt => rt.ChangedBy)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<User>()
                .HasOne(rt => rt.Role)
                .WithMany(u => u.Users)
                .HasForeignKey(rt => rt.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<User>()
                .HasOne(rt => rt.Company)
                .WithMany(u => u.Users)
                .HasForeignKey(rt => rt.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Company)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany()
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
