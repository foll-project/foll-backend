using MediatR;
using Microsoft.Extensions.Logging;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public class LowBatteryResolvedIntegrationEventHandler : INotificationHandler<LowBatteryResolvedIntegrationEvent>
{
    private readonly ILogger<LowBatteryResolvedIntegrationEventHandler> _logger;

    public LowBatteryResolvedIntegrationEventHandler(ILogger<LowBatteryResolvedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LowBatteryResolvedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "LowBatteryResolvedEvent | DeviceId={DeviceId} | PatientId={PatientId} | BatteryLevel={BatteryLevel} | ReportedAt={ReportedAtUtc}",
            notification.DeviceId,
            notification.PatientId,
            notification.BatteryLevel,
            notification.ReportedAtUtc);

        return Task.CompletedTask;
    }
}
