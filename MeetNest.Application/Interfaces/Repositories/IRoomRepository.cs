using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IRoomRepository
{
    Task AddAsync(Room room);
    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetByIdWithFacilitiesAsync(int id);
    Task UpdateAsync(Room room);
    Task DeleteAsync(Room room);

    Task<PagedResult<Room>> GetAllAsync(RoomFilterDto filter);
    Task<PagedResult<Room>> GetByBranchIdAsync(int branchId, RoomFilterDto filter);
    Task<List<Room>> GetByBranchIdSimpleAsync(int branchId);

    // ── returns future approved+pending bookings for a room ──
    // Used by RoomService before allowing delete or maintenance mode.
    // "Active" means: Status is Approved or Pending AND EndTime > UtcNow
    Task<List<Booking>> GetActiveBookingsForRoomAsync(int roomId);
}