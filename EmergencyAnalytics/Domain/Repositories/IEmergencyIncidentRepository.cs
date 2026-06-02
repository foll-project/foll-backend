using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.EmergencyAnalytics.Domain.Repositories;

public interface IEmergencyIncidentRepository : IBaseRepository<EmergencyIncident>
{
    Task<EmergencyIncident?> FindLatestOpenByDeviceIdAsync(long deviceId);
    Task<EmergencyIncident?> FindActiveByPatientIdAsync(long patientId);
    Task<EmergencyIncident?> FindByIdWithFallTypeAsync(long incidentId);
    Task<IReadOnlyCollection<EmergencyIncident>> ListByPatientIdAsync(long patientId);
}
