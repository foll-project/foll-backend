using MediatR;

namespace foll_backend.EmergencyAnalytics.Application.Internal.Notifications;

public class EmergencyIncidentClosedIntegrationEventHandler : INotificationHandler<EmergencyIncidentClosedIntegrationEvent>
{
    private readonly ILogger<EmergencyIncidentClosedIntegrationEventHandler> _logger;

    public EmergencyIncidentClosedIntegrationEventHandler(ILogger<EmergencyIncidentClosedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EmergencyIncidentClosedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EmergencyIncidentClosed | IncidentKey={IncidentKey} | DeviceId={DeviceId} | PatientId={PatientId} | FallType={FallTypeName} | Severity={SeverityLevel} | Status={Status} | CancellationReason={CancellationReason}",
            notification.IncidentKey,
            notification.DeviceId,
            notification.PatientId,
            notification.FallTypeName,
            notification.SeverityLevel,
            notification.Status,
            notification.CancellationReason);

        return Task.CompletedTask;
    }
}
