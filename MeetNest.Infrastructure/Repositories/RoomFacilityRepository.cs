
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Domain.Entities;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class RoomFacilityRepository : IRoomFacilityRepository
{
    private readonly AppDbContext _context;

    public RoomFacilityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RoomFacility entity)
        => await _context.RoomFacilities.AddAsync(entity);

    public async Task<bool> ExistsAsync(int roomId, int facilityId)
        => await _context.RoomFacilities
            .AnyAsync(x => x.RoomId == roomId && x.FacilityId == facilityId);

    public async Task<bool> FacilityHasRoomsAsync(int facilityId)
        => await _context.RoomFacilities
            .AnyAsync(x => x.FacilityId == facilityId);

    public async Task<List<RoomFacility>> GetByRoomIdAsync(int roomId)
        => await _context.RoomFacilities
            .Include(x => x.Facility)
            .Where(x => x.RoomId == roomId && x.Facility.IsActive)
            .ToListAsync();
    public async Task<RoomFacility?> GetAsync(int roomId, int facilityId)
    {
        return await _context.RoomFacilities
            .FirstOrDefaultAsync(x =>
                x.RoomId == roomId && x.FacilityId == facilityId);
    }

    public async Task RemoveAsync(RoomFacility entity)
    {
        _context.RoomFacilities.Remove(entity);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
