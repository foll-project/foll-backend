using foll_backend.Shared.Domain.Events;

namespace foll_backend.DeviceManagment.Domain.Events;

public record LowBatteryResolvedDomainEvent(
    long DeviceId,
    long AssignedPatientId,
    short BatteryLevel,
    DateTime ReportedAtUtc) : IDomainEvent
{
    public DateTime OccurredOn => ReportedAtUtc;
}
