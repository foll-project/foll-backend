using foll_backend.NotificationCommunication.Application.OutboundServices;
using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;
using Microsoft.AspNetCore.SignalR;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime;

public class SignalRNotificationRealtimePublisher : INotificationRealtimePublisher
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly ILogger<SignalRNotificationRealtimePublisher> _logger;

    public SignalRNotificationRealtimePublisher(
        IHubContext<NotificationsHub> hubContext,
        ILogger<SignalRNotificationRealtimePublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishCreatedAsync(NotificationLog notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new NotificationCreatedRealtimeMessage(
                notification.NotificationLogId,
                notification.UserId,
                notification.NotificationType.ToString(),
                notification.NotificationChannel.ToString(),
                notification.NotificationStatus.ToString(),
                notification.Title,
                notification.Body,
                notification.DataJson,
                notification.PatientId,
                notification.DeviceId,
                notification.CreatedAt);

            await _hubContext.Clients
                .Group(NotificationsHub.GetUserGroupName(notification.UserId))
                .SendAsync("notification.created", payload, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error publicando notificacion realtime {NotificationLogId} para UserId={UserId}.",
                notification.NotificationLogId,
                notification.UserId);
        }
    }
}
