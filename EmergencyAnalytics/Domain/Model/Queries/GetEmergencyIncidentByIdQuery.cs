namespace foll_backend.EmergencyAnalytics.Domain.Model.Queries;

public record GetEmergencyIncidentByIdQuery(long ActorUserId, long IncidentId);
