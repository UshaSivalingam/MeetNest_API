namespace MeetNest.Application.DTOs.Room;

public class RoomResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RoomFacilityDto> Facilities { get; set; } = new();
}

public class RoomFacilityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}