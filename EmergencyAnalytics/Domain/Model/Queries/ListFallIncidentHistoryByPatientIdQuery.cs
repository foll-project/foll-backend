namespace foll_backend.EmergencyAnalytics.Domain.Model.Queries;

public record ListFallIncidentHistoryByPatientIdQuery(long ActorUserId, long PatientId);
