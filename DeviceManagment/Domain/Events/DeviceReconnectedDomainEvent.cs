using foll_backend.DeviceManagment.Domain.Model.Enums;
using foll_backend.Shared.Domain.Events;

namespace foll_backend.DeviceManagment.Domain.Events;

public record DeviceReconnectedDomainEvent(
    long DeviceId,
    long AssignedPatientId,
    DateTime ReportedAtUtc,
    ConnectivityChangeCause Cause) : IDomainEvent
{
    public DateTime OccurredOn => ReportedAtUtc;
}
