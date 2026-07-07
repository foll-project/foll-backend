namespace foll_backend.EmergencyAnalytics.Application.OutboundServices;

public interface IEmergencyAnalyticsMqttPublisher
{
    Task PublishIncidentClosedAsync(
        long deviceId,
        string status,
        string? cancellationReason,
        DateTime closedAtUtc,
        CancellationToken cancellationToken = default);
}
