using MediatR;
using Microsoft.Extensions.Logging;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public class DeviceReconnectedIntegrationEventHandler : INotificationHandler<DeviceReconnectedIntegrationEvent>
{
    private readonly ILogger<DeviceReconnectedIntegrationEventHandler> _logger;

    public DeviceReconnectedIntegrationEventHandler(ILogger<DeviceReconnectedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DeviceReconnectedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeviceReconnectedEvent | DeviceId={DeviceId} | PatientId={PatientId} | ReportedAt={ReportedAtUtc} | Cause={Cause}",
            notification.DeviceId,
            notification.PatientId,
            notification.ReportedAtUtc,
            notification.Cause);

        return Task.CompletedTask;
    }
}
