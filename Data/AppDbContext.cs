using ESGFiap.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ESGFiap.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<WaterSensor> WaterSensors => Set<WaterSensor>();
    public DbSet<WaterReading> WaterReadings => Set<WaterReading>();
    public DbSet<LeakAlert> LeakAlerts => Set<LeakAlert>();
    public DbSet<WaterImpactReport> WaterImpactReports => Set<WaterImpactReport>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<WaterSensor>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.AlertThresholdLitersPerHour).HasPrecision(10, 3);
            e.HasOne(s => s.User).WithMany(u => u.Sensors)
             .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WaterReading>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.LitersPerHour).HasPrecision(10, 3);
            e.Property(r => r.PressureBar).HasPrecision(6, 3);
            e.HasOne(r => r.Sensor).WithMany(s => s.Readings)
             .HasForeignKey(r => r.SensorId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LeakAlert>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.DetectedLitersPerHour).HasPrecision(10, 3);
            e.HasOne(a => a.Sensor).WithMany(s => s.Alerts)
             .HasForeignKey(a => a.SensorId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WaterImpactReport>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.TotalConsumedLiters).HasPrecision(15, 3);
            e.Property(r => r.RegionalAverageLiters).HasPrecision(15, 3);
            e.HasOne(r => r.User).WithMany(u => u.Reports)
             .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
