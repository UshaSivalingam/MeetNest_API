// MeetNest.Infrastructure/Services/RoomService.cs
// ── REPLACE your existing file entirely ──

using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;

namespace MeetNest.Infrastructure.Services;

public class RoomService(IRoomRepository repo) : IRoomService
{
    // ── Queries ───────────────────────────────────────────────────
    public async Task<PagedResult<RoomResponseDto>> GetAllAsync(RoomFilterDto filter)
        => MapPaged(await repo.GetAllAsync(filter));

    public async Task<PagedResult<RoomResponseDto>> GetByBranchAsync(int branchId, RoomFilterDto filter)
        => MapPaged(await repo.GetByBranchIdAsync(branchId, filter));

    public async Task<List<RoomResponseDto>> GetByBranchIdSimpleAsync(int branchId)
    {
        var rooms = await repo.GetByBranchIdSimpleAsync(branchId);
        return rooms.Select(MapToDto).ToList();
    }

    public async Task<RoomResponseDto?> GetByIdAsync(int id)
    {
        var room = await repo.GetByIdWithFacilitiesAsync(id);
        return room is null ? null : MapToDto(room);
    }

    // ── Active bookings check (used by delete guard + edit guard) ─
    public async Task<List<ActiveBookingDto>> GetActiveBookingsAsync(int roomId)
    {
        var bookings = await repo.GetActiveBookingsForRoomAsync(roomId);
        return bookings.Select(b => new ActiveBookingDto
        {
            Id = b.Id,
            EmployeeName = b.User.FullName,
            Status = b.Status.ToString(),
            Priority = b.Priority.ToString(),
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            Notes = b.Notes,
        }).ToList();
    }

    // ── Create ────────────────────────────────────────────────────
    public async Task<int> CreateAsync(CreateRoomDto dto)
    {
        var room = new Room
        {
            Name = dto.Name.Trim(),
            Capacity = dto.Capacity,
            BranchId = dto.BranchId,
            ApprovalRequired = dto.ApprovalRequired,
            UnderMaintenance = dto.UnderMaintenance,
        };
        await repo.AddAsync(room);
        return room.Id;
    }

    // ── Update — GUARDED on maintenance flip ──────────────────────
    // If the admin is turning ON maintenance (false → true) while the room
    // has active future bookings, we block it with the same error prefix
    // the frontend already knows how to handle (ROOM_HAS_ACTIVE_BOOKINGS).
    public async Task UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await repo.GetByIdAsync(id)
            ?? throw new Exception("Room not found.");

        // ── Maintenance-flip guard ─────────────────────────────────
        // Only check when admin is ENABLING maintenance (not disabling it)
        bool turningMaintenanceOn = !room.UnderMaintenance && dto.UnderMaintenance;

        if (turningMaintenanceOn)
        {
            var activeBookings = await repo.GetActiveBookingsForRoomAsync(id);
            if (activeBookings.Count > 0)
            {
                // Same prefix as delete guard — frontend ProtectedRoomModal catches this
                throw new Exception(
                    $"ROOM_HAS_ACTIVE_BOOKINGS:{activeBookings.Count} active booking(s) prevent maintenance mode. " +
                    $"The latest booking ends at {activeBookings.Max(b => b.EndTime):yyyy-MM-dd HH:mm} UTC.");
            }
        }

        // ── Apply changes ─────────────────────────────────────────
        room.Name = dto.Name.Trim();
        room.Capacity = dto.Capacity;
        room.ApprovalRequired = dto.ApprovalRequired;
        room.UnderMaintenance = dto.UnderMaintenance;
        room.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(room);
    }

    // ── Delete — GUARDED against active bookings ──────────────────
    public async Task DeleteAsync(int id)
    {
        var room = await repo.GetByIdAsync(id)
            ?? throw new Exception("Room not found.");

        var activeBookings = await repo.GetActiveBookingsForRoomAsync(id);
        if (activeBookings.Count > 0)
        {
            throw new Exception(
                $"ROOM_HAS_ACTIVE_BOOKINGS:{activeBookings.Count} active booking(s) prevent deletion. " +
                $"The latest booking ends at {activeBookings.Max(b => b.EndTime):yyyy-MM-dd HH:mm} UTC.");
        }

        await repo.DeleteAsync(room);
    }

    // ── Mapping ───────────────────────────────────────────────────
    private static PagedResult<RoomResponseDto> MapPaged(PagedResult<Room> paged) => new()
    {
        Items = paged.Items.Select(MapToDto).ToList(),
        TotalCount = paged.TotalCount,
        Page = paged.Page,
        PageSize = paged.PageSize,
    };

    private static RoomResponseDto MapToDto(Room r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Capacity = r.Capacity,
        BranchId = r.BranchId,
        BranchName = r.Branch?.Name ?? string.Empty,
        IsActive = r.IsActive,
        CreatedAt = r.CreatedAt,
        ApprovalRequired = r.ApprovalRequired,
        UnderMaintenance = r.UnderMaintenance,
        Facilities = r.RoomFacilities?
            .Where(rf => rf.Facility.IsActive)
            .Select(rf => new RoomFacilityDto
            {
                Id = rf.Facility.Id,
                Name = rf.Facility.Name,
                Description = rf.Facility.Description,
            })
            .ToList() ?? new(),
    };
}