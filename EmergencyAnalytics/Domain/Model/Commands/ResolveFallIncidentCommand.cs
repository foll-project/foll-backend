namespace foll_backend.EmergencyAnalytics.Domain.Model.Commands;

public record ResolveFallIncidentCommand(
    long IncidentId,
    long ActorUserId,
    string? Observation,
    DateTime ResolvedAtUtc);
