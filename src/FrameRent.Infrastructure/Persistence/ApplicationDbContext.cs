using FrameRent.Domain.Entities;
using FrameRent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FrameRent.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<Rental> Rentals => Set<Rental>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Equipment configuration
        builder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DailyRate).HasPrecision(18, 2);
            entity.Property(e => e.TotalUnits).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        // Rental configuration
        builder.Entity<Rental>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ClientId).IsRequired();
            entity.Property(r => r.DailyRateAtBooking).HasPrecision(18, 2);
            entity.Property(r => r.TotalCost).HasPrecision(18, 2);
            entity.Property(r => r.LateFee).HasPrecision(18, 2);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(r => r.Equipment)
                  .WithMany(e => e.Rentals)
                  .HasForeignKey(r => r.EquipmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
