using System.Text.Json;
using foll_backend.DeviceManagment.Application.Internal.Notifications;
using foll_backend.NotificationCommunication.Domain.Model.Commands;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using foll_backend.NotificationCommunication.Domain.Services;
using MediatR;

namespace foll_backend.NotificationCommunication.Application.Internal.EventHandlers;

public class LowBatteryResolvedNotificationHandler : INotificationHandler<LowBatteryResolvedIntegrationEvent>
{
    private readonly INotificationCommandService _notificationCommandService;

    public LowBatteryResolvedNotificationHandler(INotificationCommandService notificationCommandService)
    {
        _notificationCommandService = notificationCommandService;
    }

    public async Task Handle(LowBatteryResolvedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var dataJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["eventType"] = "LowBatteryResolved",
            ["deviceId"] = notification.DeviceId.ToString(),
            ["patientId"] = notification.PatientId.ToString(),
            ["batteryLevel"] = notification.BatteryLevel.ToString(),
            ["reportedAtUtc"] = notification.ReportedAtUtc.ToString("O")
        });

        await _notificationCommandService.Handle(new CreateNotificationFromEventCommand(
            notification.PatientId,
            notification.DeviceId,
            null,
            NotificationType.BatteryRecovered,
            "Bateria recuperada",
            $"La bateria del dispositivo {notification.DeviceId} volvio a estar sobre el umbral.",
            dataJson));
    }
}
