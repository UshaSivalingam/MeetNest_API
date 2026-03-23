using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;

namespace MeetNest.Application.Interfaces.Services;

public interface IRoomService
{
    Task<PagedResult<RoomResponseDto>> GetAllAsync(RoomFilterDto filter);
    Task<PagedResult<RoomResponseDto>> GetByBranchAsync(int branchId, RoomFilterDto filter);
    Task<RoomResponseDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateRoomDto dto);
    Task UpdateAsync(int id, UpdateRoomDto dto);
    Task DeleteAsync(int id);
    Task<List<RoomResponseDto>> GetByBranchIdSimpleAsync(int branchId);

    // Check active bookings before delete/maintenance
    Task<List<ActiveBookingDto>> GetActiveBookingsAsync(int roomId);

    // ── Block scheduling ──────────────────────────────────────
    Task SetBlockDateAsync(int id, DateTime blockFromDate, string reason);  // ← NEW
    Task RemoveBlockDateAsync(int id);                                       // ← NEW
}
