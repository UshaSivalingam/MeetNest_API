namespace MeetNest.Domain.Enums;

public enum NotificationType
{
    // Admin approved a booking → schedule reminder at EndTime
    MeetingEndReminder = 1,

    // Employee cancelled a booking → notify admin immediately
    BookingCancelled = 2,

    // Room blocked from delete/maintenance due to active booking
    RoomBlocked = 3,

    // General info
    General = 4
}