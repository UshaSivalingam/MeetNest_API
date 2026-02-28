using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role
                {
                    Name = UserRole.Admin,
                    Description = "Manages users and rooms",
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = UserRole.Employee,
                    Description = "Can book rooms and view schedule",
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
        }
    }
}