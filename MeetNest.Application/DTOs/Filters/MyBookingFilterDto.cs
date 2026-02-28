namespace MeetNest.Application.DTOs.Filters;
public class MyBookingFilterDto
{
    public string? Status { get; set; }   // "Pending" | "Approved" | "Rejected" | "Cancelled"
    public string? Search { get; set; }   // matches RoomName
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}