using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Repositories;

public class PatientInvitationRepository : BaseRepository<PatientInvitation>, IPatientInvitationRepository
{
    public PatientInvitationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PatientInvitation>> ListByPatientIdAsync(long patientId)
    {
        if (patientId <= 0) return Array.Empty<PatientInvitation>();

        return await Context.Set<PatientInvitation>()
            .Where(i => i.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PatientInvitation>> ListByInvitingUserIdAsync(long invitingUserId)
    {
        if (invitingUserId <= 0) return Array.Empty<PatientInvitation>();

        return await Context.Set<PatientInvitation>()
            .Where(i => i.InvitingUserId == invitingUserId)
            .OrderByDescending(i => i.PatientInvitationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PatientInvitation>> ListForOfficialGuardianAsync(long guardianUserId)
    {
        if (guardianUserId <= 0) return Array.Empty<PatientInvitation>();

        var patientIds = Context.Set<Patient>()
            .Where(p => p.OfficialGuardianUserId == guardianUserId)
            .Select(p => p.PatientId);

        return await Context.Set<PatientInvitation>()
            .Where(i => patientIds.Contains(i.PatientId))
            .OrderByDescending(i => i.PatientInvitationId)
            .ToListAsync();
    }
}
