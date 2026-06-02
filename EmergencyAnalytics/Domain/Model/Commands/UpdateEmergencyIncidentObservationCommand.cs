namespace foll_backend.EmergencyAnalytics.Domain.Model.Commands;

public record UpdateEmergencyIncidentObservationCommand(
    long IncidentId,
    long ActorUserId,
    string Observation);
