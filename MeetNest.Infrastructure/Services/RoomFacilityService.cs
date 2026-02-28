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

    // ✅ Assign Facility to Room
    public async Task AssignFacilityAsync(CreateRoomFacilityDto dto)
    {
        // 🔎 Validate Room exists
        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new Exception("Room not found.");

        // 🔎 Validate Facility exists & active
        var facility = await _facilityRepo.GetByIdAsync(dto.FacilityId);
        if (facility == null || !facility.IsActive)
            throw new Exception("Facility not found or inactive.");

        // 🔒 Prevent duplicate assignment
        var exists = await _repo
            .ExistsAsync(dto.RoomId, dto.FacilityId);

        if (exists)
            throw new Exception("Facility already assigned to this room.");

        var entity = new RoomFacility
        {
            RoomId = dto.RoomId,
            FacilityId = dto.FacilityId
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
    }

    // ✅ Get Facilities By Room
    public async Task<List<string>> GetFacilitiesByRoomAsync(int roomId)
    {
        var list = await _repo.GetByRoomIdAsync(roomId);

        return list
            .Where(x => x.Facility.IsActive)
            .Select(x => x.Facility.Name)
            .ToList();
    }
    public async Task<bool> RemoveFacilityAsync(int roomId, int facilityId)
    {
        var entity = await _repo.GetAsync(roomId, facilityId);

        if (entity == null)
            return false;

        await _repo.RemoveAsync(entity);
        await _repo.SaveChangesAsync();

        return true;
    }
}
