namespace MeetNest.Application.DTOs.Room;

public class UpdateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }

    // ── NEW: Admin can flip approval mode on existing rooms ────────
    public bool ApprovalRequired { get; set; } = true;
}
