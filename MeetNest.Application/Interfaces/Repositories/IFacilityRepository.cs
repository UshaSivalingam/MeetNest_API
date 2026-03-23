using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IFacilityRepository
{
    Task AddAsync(Facility facility);
    Task<Facility?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    void Update(Facility facility);
    Task DeleteAsync(Facility facility);
    Task SaveChangesAsync();

    // ── Updated: paged + filtered ─────────────────────────────────────────────
    Task<PagedResult<Facility>> GetAllAsync(FacilityFilterDto filter);

    // ── Keep simple list for room-facility assignment dropdowns ────────────────
    Task<List<Facility>> GetAllSimpleAsync();
}
