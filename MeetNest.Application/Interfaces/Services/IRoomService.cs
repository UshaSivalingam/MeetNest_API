using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;

namespace MeetNest.Application.Interfaces.Services;

public interface IRoomService
{
    // ── Existing (unchanged) ──────────────────────────────────────
    Task<PagedResult<RoomResponseDto>> GetAllAsync(RoomFilterDto filter);
    Task<PagedResult<RoomResponseDto>> GetByBranchAsync(int branchId, RoomFilterDto filter);
    Task<RoomResponseDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateRoomDto dto);
    Task UpdateAsync(int id, UpdateRoomDto dto);
    Task DeleteAsync(int id);
    Task<List<RoomResponseDto>> GetByBranchIdSimpleAsync(int branchId);

    // ── NEW: check active bookings before delete/maintenance ──────
    // Returns a summary list so the frontend can show the warning modal.
    // "Active" = Approved or Pending bookings with EndTime > now.
    Task<List<ActiveBookingDto>> GetActiveBookingsAsync(int roomId);
}