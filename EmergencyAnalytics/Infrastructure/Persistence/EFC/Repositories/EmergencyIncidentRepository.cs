using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Persistence.EFC.Repositories;

public class EmergencyIncidentRepository : BaseRepository<EmergencyIncident>, IEmergencyIncidentRepository
{
    public EmergencyIncidentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<EmergencyIncident?> FindLatestOpenByDeviceIdAsync(long deviceId)
    {
        if (deviceId <= 0) return null;

        return await Context.Set<EmergencyIncident>()
            .Include(i => i.FallType)
            .Where(i => i.DeviceId == deviceId && i.Status == EmergencyIncidentStatus.Open)
            .OrderByDescending(i => i.OpenedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<EmergencyIncident?> FindActiveByPatientIdAsync(long patientId)
    {
        if (patientId <= 0) return null;

        return await Context.Set<EmergencyIncident>()
            .Include(i => i.FallType)
            .Where(i => i.PatientId == patientId && i.Status == EmergencyIncidentStatus.Open)
            .OrderByDescending(i => i.OpenedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<EmergencyIncident?> FindByIdWithFallTypeAsync(long incidentId)
    {
        if (incidentId <= 0) return null;

        return await Context.Set<EmergencyIncident>()
            .Include(i => i.FallType)
            .FirstOrDefaultAsync(i => i.EmergencyIncidentId == incidentId);
    }

    public async Task<IReadOnlyCollection<EmergencyIncident>> ListByPatientIdAsync(long patientId)
    {
        if (patientId <= 0) return Array.Empty<EmergencyIncident>();

        return await Context.Set<EmergencyIncident>()
            .Include(i => i.FallType)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.OpenedAt)
            .ToListAsync();
    }
}
