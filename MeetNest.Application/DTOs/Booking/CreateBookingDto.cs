namespace MeetNest.Application.DTOs.Booking;

public class CreateBookingDto
{
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}