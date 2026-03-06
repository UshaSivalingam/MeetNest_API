using MeetNest.Application.DTOs.RoomFacility;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;

namespace MeetNest.Infrastructure.Services;

public class RoomFacilityService : IRoomFacilityService
{
    private readonly IRoomFacilityRepository _repo;
    private readonly IFacilityRepository _facilityRepo;
    private readonly IRoomRepository _roomRepo;

    public RoomFacilityService(
        IRoomFacilityRepository repo,
        IFacilityRepository facilityRepo,
        IRoomRepository roomRepo)
    {
        _repo = repo;
        _facilityRepo = facilityRepo;
        _roomRepo = roomRepo;
    }

    public async Task AssignFacilityAsync(CreateRoomFacilityDto dto)
    {
        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null) throw new Exception("Room not found.");

        var facility = await _facilityRepo.GetByIdAsync(dto.FacilityId);
        if (facility == null || !facility.IsActive) throw new Exception("Facility not found or inactive.");

        var exists = await _repo.ExistsAsync(dto.RoomId, dto.FacilityId);
        if (exists) throw new Exception("Facility already assigned to this room.");

        await _repo.AddAsync(new RoomFacility { RoomId = dto.RoomId, FacilityId = dto.FacilityId });
        await _repo.SaveChangesAsync();
    }

    // ← FIXED: was returning List<string> (names only — no IDs!)
    //          now returns List<RoomFacilityItemDto> with FacilityId
    public async Task<List<RoomFacilityItemDto>> GetFacilitiesByRoomAsync(int roomId)
    {
        var list = await _repo.GetByRoomIdAsync(roomId);

        return list
            .Where(x => x.Facility.IsActive)
            .Select(x => new RoomFacilityItemDto
            {
                FacilityId = x.FacilityId,
                Name = x.Facility.Name,
                Icon = x.Facility.Icon ?? "🔧"
            })
            .ToList();
    }

    public async Task<bool> RemoveFacilityAsync(int roomId, int facilityId)
    {
        var entity = await _repo.GetAsync(roomId, facilityId);
        if (entity == null) return false;
        await _repo.RemoveAsync(entity);
        await _repo.SaveChangesAsync();
        return true;
    }
}