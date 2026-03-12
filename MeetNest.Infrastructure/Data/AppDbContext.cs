using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<RoomFacility> RoomFacilities => Set<RoomFacility>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── NEW ──────────────────────────────────────────────────────
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("meetnest");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = 1,
                Name = UserRole.Admin,
                Description = "Manages users and rooms",
                CreatedAt = new DateTime(2026, 2, 18, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = 2,
                Name = UserRole.Employee,
                Description = "Can book rooms and view schedule",
                CreatedAt = new DateTime(2026, 2, 18, 10, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<RoomFacility>()
            .HasKey(rf => new { rf.RoomId, rf.FacilityId });

        // ── Notification configuration ────────────────────────────
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);

            // UserId → User (required)
            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // BookingId → Booking (optional, set null on delete)
            entity.HasOne(n => n.Booking)
                  .WithMany()
                  .HasForeignKey(n => n.BookingId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            // RoomId → Room (optional, set null on delete)
            entity.HasOne(n => n.Room)
                  .WithMany()
                  .HasForeignKey(n => n.RoomId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(n => n.Message).HasMaxLength(1000);
            entity.Property(n => n.Type).HasConversion<int>();

            // Index for fast unread-count queries (most common access pattern)
            entity.HasIndex(n => new { n.UserId, n.IsRead, n.ScheduledFor });
        });

        base.OnModelCreating(modelBuilder);
    }
}