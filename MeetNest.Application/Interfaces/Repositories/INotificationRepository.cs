using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(int id);

    // Returns notifications for a user that are due (ScheduledFor <= now OR null) and unread
    Task<List<Notification>> GetDueForUserAsync(int userId);

    // Unread count for bell badge — only due notifications
    Task<int> GetUnreadCountAsync(int userId);

    Task MarkReadAsync(int id);
    Task MarkAllReadAsync(int userId);
    Task UpdateAsync(Notification notification);
}