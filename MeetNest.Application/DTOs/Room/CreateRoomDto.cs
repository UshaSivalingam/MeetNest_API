namespace MeetNest.Application.DTOs.Room;

public class CreateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int BranchId { get; set; }
    public bool ApprovalRequired { get; set; } = true;
    public bool UnderMaintenance { get; set; } = false;
}