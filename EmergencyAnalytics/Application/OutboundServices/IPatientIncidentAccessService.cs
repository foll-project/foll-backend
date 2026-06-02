namespace foll_backend.EmergencyAnalytics.Application.OutboundServices;

public interface IPatientIncidentAccessService
{
    Task<bool> CanAccessIncidentAsync(long actorUserId, long patientId);
}
