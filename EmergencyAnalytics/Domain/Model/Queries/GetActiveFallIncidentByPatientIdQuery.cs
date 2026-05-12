namespace foll_backend.EmergencyAnalytics.Domain.Model.Queries;

public record GetActiveFallIncidentByPatientIdQuery(long ActorUserId, long PatientId);
