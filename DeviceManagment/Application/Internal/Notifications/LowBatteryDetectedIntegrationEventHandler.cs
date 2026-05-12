using MediatR;
using Microsoft.Extensions.Logging;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public class LowBatteryDetectedIntegrationEventHandler : INotificationHandler<LowBatteryDetectedIntegrationEvent>
{
    private readonly ILogger<LowBatteryDetectedIntegrationEventHandler> _logger;

    public LowBatteryDetectedIntegrationEventHandler(ILogger<LowBatteryDetectedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LowBatteryDetectedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "LowBatteryDetectedEvent | DeviceId={DeviceId} | PatientId={PatientId} | BatteryLevel={BatteryLevel} | ReportedAt={ReportedAtUtc}",
            notification.DeviceId,
            notification.PatientId,
            notification.BatteryLevel,
            notification.ReportedAtUtc);

        return Task.CompletedTask;
    }
}
