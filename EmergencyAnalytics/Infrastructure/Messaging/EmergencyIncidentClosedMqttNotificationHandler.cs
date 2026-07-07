using foll_backend.EmergencyAnalytics.Application.Internal.Notifications;
using foll_backend.EmergencyAnalytics.Application.OutboundServices;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using MediatR;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Messaging;

/// <summary>
/// Cuando un cuidador cierra un incidente desde la web, avisa a la capa Edge vía MQTT
/// para resetear is_falling y apagar el buzzer del ESP32.
/// </summary>
public class EmergencyIncidentClosedMqttNotificationHandler : INotificationHandler<EmergencyIncidentClosedIntegrationEvent>
{
    private readonly IEmergencyAnalyticsMqttPublisher _mqttPublisher;
    private readonly ILogger<EmergencyIncidentClosedMqttNotificationHandler> _logger;

    public EmergencyIncidentClosedMqttNotificationHandler(
        IEmergencyAnalyticsMqttPublisher mqttPublisher,
        ILogger<EmergencyIncidentClosedMqttNotificationHandler> logger)
    {
        _mqttPublisher = mqttPublisher;
        _logger = logger;
    }

    public async Task Handle(EmergencyIncidentClosedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (notification.CancellationReason == EmergencyCancellationReason.UserButtonPressed)
            return;

        var closedByCaregiver =
            notification.Status == EmergencyIncidentStatus.Resolved
            || notification.CancellationReason == EmergencyCancellationReason.FalsePositive;

        if (!closedByCaregiver)
            return;

        try
        {
            await _mqttPublisher.PublishIncidentClosedAsync(
                notification.DeviceId,
                notification.Status.ToString(),
                notification.CancellationReason?.ToString(),
                notification.ClosedAtUtc,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error publicando incident-closed por MQTT. DeviceId={DeviceId} PatientId={PatientId}",
                notification.DeviceId,
                notification.PatientId);
        }
    }
}
