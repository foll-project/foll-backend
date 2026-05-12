using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Repositories;

public class PatientRepository : BaseRepository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext context) : base(context)
    {
    }

    public new async Task<Patient?> FindByIdAsync(long id)
    {
        if (id <= 0) return null;

        return await Context.Set<Patient>()
            .Include(p => p.Invitations)
            .Include(p => p.EmergencyContacts)
            .Include(p => p.Caregivers)
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }

    public async Task<Patient?> FindByDniAsync(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return null;
        var normalized = dni.Trim();

        return await Context.Set<Patient>()
            .Include(p => p.Invitations)
            .Include(p => p.EmergencyContacts)
            .Include(p => p.Caregivers)
            .FirstOrDefaultAsync(p => p.Dni == normalized);
    }

    public async Task<IEnumerable<Patient>> ListForUserAsync(long userId)
    {
        if (userId <= 0) return Array.Empty<Patient>();

        return await Context.Set<Patient>()
            .Include(p => p.Invitations)
            .Include(p => p.EmergencyContacts)
            .Include(p => p.Caregivers)
            .Where(p => p.OfficialGuardianUserId == userId || p.Caregivers.Any(c => c.UserId == userId))
            .ToListAsync();
    }
}
