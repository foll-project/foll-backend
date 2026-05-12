using MediatR;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public record LowBatteryResolvedIntegrationEvent(
    long DeviceId,
    long PatientId,
    short BatteryLevel,
    DateTime ReportedAtUtc) : INotification;
