// Runs every 60 seconds, fires email when a MeetingEndReminder becomes due

using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeetNest.Infrastructure.Services;

public class NotificationReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationReminderBackgroundService> _logger;

    // Runs every 60 seconds
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    public NotificationReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationReminderBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NotificationReminderBackgroundService.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessDueRemindersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;

        // Find MeetingEndReminder notifications that:
        // 1. Are scheduled (ScheduledFor is set)
        // 2. Are now due (ScheduledFor <= now)
        // 3. Have not had their email sent yet (EmailSentAt is null)
        var dueReminders = await db.Notifications
            .Include(n => n.Room)
                .ThenInclude(r => r.Branch)
            .Where(n =>
                n.Type == NotificationType.MeetingEndReminder &&
                n.ScheduledFor != null &&
                n.ScheduledFor <= now &&
                n.EmailSentAt == null)   // ← tracks whether email already fired
            .ToListAsync();

        foreach (var reminder in dueReminders)
        {
            try
            {
                var roomName = reminder.Room?.Name ?? "Unknown Room";
                var branchName = reminder.Room?.Branch?.Name ?? string.Empty;
                var endTime = reminder.ScheduledFor!.Value;

                await emailService.SendMeetingEndReminderAsync(roomName, branchName, endTime);

                // Mark email as sent so we never fire it twice
                reminder.EmailSentAt = now;
                await db.SaveChangesAsync();

                _logger.LogInformation(
                    "Meeting end reminder email sent for room {Room} at {Time}.",
                    roomName, endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send meeting end reminder email for notification {Id}.",
                    reminder.Id);
                // Don't mark EmailSentAt — will retry next tick
            }
        }
    }
}