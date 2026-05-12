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
}
