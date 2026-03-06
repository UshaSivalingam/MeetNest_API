namespace MeetNest.Application.DTOs.Room;

public class CreateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int BranchId { get; set; }

    // ── NEW: Admin chooses approval mode at creation time ─────────
    // Defaults to true (require admin approval) if not supplied
    public bool ApprovalRequired { get; set; } = true;
}