namespace MeetNest.Application.DTOs.Filters;
public class RoomFilterDto
{
    public string? Search { get; set; }   // matches Name
    public int? BranchId { get; set; }
    public int? MinCap { get; set; }   
    public int? MaxCap { get; set; }  
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}