using foll_backend.Shared.Domain.Events;

namespace foll_backend.EmergencyAnalytics.Domain.Events;

public record EmergencyIncidentResolvedDomainEvent(
    Guid IncidentKey,
    long DeviceId,
    long PatientId,
    short FallTypeId,
    DateTime ResolvedAtUtc,
    long ActorUserId,
    string? Observation) : IDomainEvent
{
    public DateTime OccurredOn => ResolvedAtUtc;
}
