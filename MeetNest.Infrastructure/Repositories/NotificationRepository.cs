// MeetNest.Infrastructure/Repositories/NotificationRepository.cs
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Domain.Entities;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;
    public NotificationRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<Notification?> GetByIdAsync(int id)
        => await _context.Notifications
            .Include(n => n.Room)
            .FirstOrDefaultAsync(n => n.Id == id);

    // Only return notifications that are:
    // 1. For this user
    // 2. Unread
    // 3. ScheduledFor is null (immediate) OR ScheduledFor <= now (due)
    public async Task<List<Notification>> GetDueForUserAsync(int userId)
        => await _context.Notifications
            .Include(n => n.Room)
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead &&
                (n.ScheduledFor == null || n.ScheduledFor <= DateTime.UtcNow))
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)  // cap at 20 for performance
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _context.Notifications.CountAsync(n =>
            n.UserId == userId &&
            !n.IsRead &&
            (n.ScheduledFor == null || n.ScheduledFor <= DateTime.UtcNow));

    public async Task MarkReadAsync(int id)
    {
        var n = await _context.Notifications.FindAsync(id);
        if (n is null) return;
        n.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var n in notifications) n.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }
}