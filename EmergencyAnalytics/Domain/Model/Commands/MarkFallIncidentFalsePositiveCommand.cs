namespace foll_backend.EmergencyAnalytics.Domain.Model.Commands;

public record MarkFallIncidentFalsePositiveCommand(
    long IncidentId,
    long ActorUserId,
    string? Observation,
    DateTime MarkedAtUtc);
