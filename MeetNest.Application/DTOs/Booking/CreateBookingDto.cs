using MeetNest.Domain.Enums;
using System.Text.Json.Serialization;

namespace MeetNest.Application.DTOs.Booking;

public class CreateBookingDto
{
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Frontend sends "Low" / "Medium" / "High" as strings.
    // STJ deserializes enums from integers by default — so "Medium" and "High"
    // both silently failed to bind and became 0 (Low).
    // JsonStringEnumConverter makes STJ accept string names correctly.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingPriority Priority { get; set; }
    public string? Notes { get; set; }
}