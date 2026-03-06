using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Facility;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;

namespace MeetNest.Infrastructure.Services;

public class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _repo;
    private readonly IRoomFacilityRepository _roomFacilityRepo;

    public FacilityService(IFacilityRepository repo, IRoomFacilityRepository roomFacilityRepo)
    {
        _repo = repo;
        _roomFacilityRepo = roomFacilityRepo;
    }

    public async Task<PagedResult<FacilityResponseDto>> GetAllAsync(FacilityFilterDto filter)
    {
        var paged = await _repo.GetAllAsync(filter);
        return new PagedResult<FacilityResponseDto>
        {
            Items = paged.Items.Select(MapToDto).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<List<FacilityResponseDto>> GetAllSimpleAsync()
    {
        var list = await _repo.GetAllSimpleAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<FacilityResponseDto?> GetByIdAsync(int id)
    {
        var f = await _repo.GetByIdAsync(id);
        return f is null || !f.IsActive ? null : MapToDto(f);
    }

    public async Task<FacilityResponseDto> CreateAsync(CreateFacilityDto dto)
    {
        if (await _repo.ExistsByNameAsync(dto.Name))
            throw new Exception("Facility already exists.");

        var facility = new Facility
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "🔧" : dto.Icon   // ← NEW
        };
        await _repo.AddAsync(facility);
        await _repo.SaveChangesAsync();
        return MapToDto(facility);
    }

    public async Task UpdateAsync(int id, UpdateFacilityDto dto)
    {
        var f = await _repo.GetByIdAsync(id);
        if (f is null || !f.IsActive) throw new Exception("Facility not found.");
        if (f.Name != dto.Name && await _repo.ExistsByNameAsync(dto.Name))
            throw new Exception("Facility name already exists.");

        f.Name = dto.Name;
        f.Description = dto.Description;
        f.Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "🔧" : dto.Icon;   // ← NEW
        f.UpdatedAt = DateTime.UtcNow;

        _repo.Update(f);
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var f = await _repo.GetByIdAsync(id);
        if (f is null || !f.IsActive) throw new Exception("Facility not found.");
        if (await _roomFacilityRepo.FacilityHasRoomsAsync(id))
            throw new Exception("Cannot delete: facility is assigned to rooms.");
        await _repo.DeleteAsync(f);
        await _repo.SaveChangesAsync();
    }

    // ← UPDATED: includes Icon
    private static FacilityResponseDto MapToDto(Facility f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        Description = f.Description,
        Icon = f.Icon ?? "🔧"
    };
}