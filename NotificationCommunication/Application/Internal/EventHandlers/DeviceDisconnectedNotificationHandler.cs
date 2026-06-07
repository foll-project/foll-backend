using System.Text.Json;
using foll_backend.DeviceManagment.Application.Internal.Notifications;
using foll_backend.NotificationCommunication.Domain.Model.Commands;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using foll_backend.NotificationCommunication.Domain.Services;
using MediatR;

namespace foll_backend.NotificationCommunication.Application.Internal.EventHandlers;

public class DeviceDisconnectedNotificationHandler : INotificationHandler<DeviceDisconnectedIntegrationEvent>
{
    private readonly INotificationCommandService _notificationCommandService;

    public DeviceDisconnectedNotificationHandler(INotificationCommandService notificationCommandService)
    {
        _notificationCommandService = notificationCommandService;
    }

    public async Task Handle(DeviceDisconnectedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var dataJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["eventType"] = "DeviceDisconnected",
            ["deviceId"] = notification.DeviceId.ToString(),
            ["patientId"] = notification.PatientId.ToString(),
            ["detectedAtUtc"] = notification.DetectedAtUtc.ToString("O"),
            ["lastSeenAtUtc"] = notification.LastSeenAtUtc.ToString("O"),
            ["cause"] = notification.Cause.ToString()
        });

        await _notificationCommandService.Handle(new CreateNotificationFromEventCommand(
            notification.PatientId,
            notification.DeviceId,
            null,
            NotificationType.DeviceDisconnected,
            "Dispositivo desconectado",
            $"El dispositivo {notification.DeviceId} dejo de reportar senal.",
            dataJson));
    }
}
