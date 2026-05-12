using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using foll_backend.Shared.Domain.Events;

namespace foll_backend.EmergencyAnalytics.Domain.Events;

public record EmergencyIncidentCancelledDomainEvent(
    Guid IncidentKey,
    long DeviceId,
    long PatientId,
    short FallTypeId,
    DateTime CancelledAtUtc,
    EmergencyCancellationReason Reason,
    long? ActorUserId,
    string? Observation) : IDomainEvent
{
    public DateTime OccurredOn => CancelledAtUtc;
}
