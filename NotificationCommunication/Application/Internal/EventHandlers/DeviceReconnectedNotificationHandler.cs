using System.Text.Json;
using foll_backend.DeviceManagment.Application.Internal.Notifications;
using foll_backend.NotificationCommunication.Domain.Model.Commands;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using foll_backend.NotificationCommunication.Domain.Services;
using MediatR;

namespace foll_backend.NotificationCommunication.Application.Internal.EventHandlers;

public class DeviceReconnectedNotificationHandler : INotificationHandler<DeviceReconnectedIntegrationEvent>
{
    private readonly INotificationCommandService _notificationCommandService;

    public DeviceReconnectedNotificationHandler(INotificationCommandService notificationCommandService)
    {
        _notificationCommandService = notificationCommandService;
    }

    public async Task Handle(DeviceReconnectedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var dataJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["eventType"] = "DeviceReconnected",
            ["deviceId"] = notification.DeviceId.ToString(),
            ["patientId"] = notification.PatientId.ToString(),
            ["reportedAtUtc"] = notification.ReportedAtUtc.ToString("O"),
            ["cause"] = notification.Cause.ToString()
        });

        await _notificationCommandService.Handle(new CreateNotificationFromEventCommand(
            notification.PatientId,
            notification.DeviceId,
            null,
            NotificationType.DeviceReconnected,
            "Dispositivo reconectado",
            $"El dispositivo {notification.DeviceId} volvio a reportar senal.",
            dataJson));
    }
}
