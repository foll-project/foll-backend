using foll_backend.Shared.Domain.Events;

namespace foll_backend.EmergencyAnalytics.Domain.Events;

public record EmergencyIncidentOpenedDomainEvent(
    Guid IncidentKey,
    long DeviceId,
    long PatientId,
    short FallTypeId,
    DateTime OpenedAtUtc,
    decimal? AiConfidenceScore,
    decimal? Latitude,
    decimal? Longitude) : IDomainEvent
{
    public DateTime OccurredOn => OpenedAtUtc;
}
