using MediatR;
using Microsoft.Extensions.Logging;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public class DeviceDisconnectedIntegrationEventHandler : INotificationHandler<DeviceDisconnectedIntegrationEvent>
{
    private readonly ILogger<DeviceDisconnectedIntegrationEventHandler> _logger;

    public DeviceDisconnectedIntegrationEventHandler(ILogger<DeviceDisconnectedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DeviceDisconnectedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "DeviceDisconnectedEvent | DeviceId={DeviceId} | PatientId={PatientId} | DetectedAt={DetectedAtUtc} | LastSeenAt={LastSeenAtUtc} | Cause={Cause}",
            notification.DeviceId,
            notification.PatientId,
            notification.DetectedAtUtc,
            notification.LastSeenAtUtc,
            notification.Cause);

        return Task.CompletedTask;
    }
}
