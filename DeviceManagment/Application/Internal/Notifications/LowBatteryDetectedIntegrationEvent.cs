using MediatR;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public record LowBatteryDetectedIntegrationEvent(
    long DeviceId,
    long PatientId,
    short BatteryLevel,
    DateTime ReportedAtUtc) : INotification;
