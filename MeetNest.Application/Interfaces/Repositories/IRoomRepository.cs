using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IRoomRepository
{
    // ── Existing (unchanged) ──────────────────────────────────────────────────
    Task AddAsync(Room room);
    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetByIdWithFacilitiesAsync(int id);
    Task UpdateAsync(Room room);
    Task DeleteAsync(Room room);

    // ── Updated: paged + filtered ─────────────────────────────────────────────
    Task<PagedResult<Room>> GetAllAsync(RoomFilterDto filter);
    Task<PagedResult<Room>> GetByBranchIdAsync(int branchId, RoomFilterDto filter);

    // ── Keep simple list for employee room selection (no pagination) ───────────
    Task<List<Room>> GetByBranchIdSimpleAsync(int branchId);
}
