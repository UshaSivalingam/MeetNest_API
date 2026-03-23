public class UpdateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool ApprovalRequired { get; set; } = true;
    public bool UnderMaintenance { get; set; } = false;
}