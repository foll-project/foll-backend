using MediatR;

namespace foll_backend.EmergencyAnalytics.Application.Internal.Notifications;

public class EmergencyIncidentOpenedIntegrationEventHandler : INotificationHandler<EmergencyIncidentOpenedIntegrationEvent>
{
    private readonly ILogger<EmergencyIncidentOpenedIntegrationEventHandler> _logger;

    public EmergencyIncidentOpenedIntegrationEventHandler(ILogger<EmergencyIncidentOpenedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EmergencyIncidentOpenedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "EmergencyIncidentOpened | IncidentKey={IncidentKey} | DeviceId={DeviceId} | PatientId={PatientId} | FallType={FallTypeName} | Severity={SeverityLevel} | OpenedAt={OpenedAtUtc}",
            notification.IncidentKey,
            notification.DeviceId,
            notification.PatientId,
            notification.FallTypeName,
            notification.SeverityLevel,
            notification.OpenedAtUtc);

        return Task.CompletedTask;
    }
}
