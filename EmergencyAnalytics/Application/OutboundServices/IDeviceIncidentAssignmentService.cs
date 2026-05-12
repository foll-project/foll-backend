namespace foll_backend.EmergencyAnalytics.Application.OutboundServices;

public interface IDeviceIncidentAssignmentService
{
    Task<long?> FindAssignedPatientIdAsync(long deviceId);
}
