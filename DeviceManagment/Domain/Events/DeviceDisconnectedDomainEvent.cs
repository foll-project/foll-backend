using foll_backend.DeviceManagment.Domain.Model.Enums;
using foll_backend.Shared.Domain.Events;

namespace foll_backend.DeviceManagment.Domain.Events;

public record DeviceDisconnectedDomainEvent(
    long DeviceId,
    long AssignedPatientId,
    DateTime DetectedAtUtc,
    DateTime LastSeenAtUtc,
    ConnectivityChangeCause Cause) : IDomainEvent
{
    public DateTime OccurredOn => DetectedAtUtc;
}
