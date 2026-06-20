using foll_backend.DeviceManagment.Application.OutboundServices;
using foll_backend.NotificationCommunication.Application.OutboundServices;
using foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;
using Microsoft.AspNetCore.SignalR;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime;

/// <summary>
/// Implementación SignalR del canal de telemetría en tiempo real. Resuelve a los
/// cuidadores (principal + secundarios) del paciente dueño del dispositivo y les
/// envía el evento "device.telemetry" únicamente a sus grupos personales.
/// </summary>
public class SignalRDeviceTelemetryRealtimePublisher : IDeviceTelemetryRealtimePublisher
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly IPatientNotificationAccessService _patientNotificationAccessService;
    private readonly ILogger<SignalRDeviceTelemetryRealtimePublisher> _logger;

    public SignalRDeviceTelemetryRealtimePublisher(
        IHubContext<NotificationsHub> hubContext,
        IPatientNotificationAccessService patientNotificationAccessService,
        ILogger<SignalRDeviceTelemetryRealtimePublisher> logger)
    {
        _hubContext = hubContext;
        _patientNotificationAccessService = patientNotificationAccessService;
        _logger = logger;
    }

    public async Task PublishTelemetryAsync(DeviceTelemetrySnapshot snapshot, CancellationToken cancellationToken = default)
    {
        try
        {
            var recipients = await _patientNotificationAccessService.GetRecipientsForPatientAsync(snapshot.PatientId);
            if (recipients.Count == 0) return;

            var payload = new DeviceTelemetryRealtimeMessage(
                snapshot.DeviceId,
                snapshot.PatientId,
                snapshot.BatteryLevel,
                snapshot.IsCharging,
                snapshot.IsOnline,
                snapshot.LastHeartbeatAtUtc);

            var groups = recipients
                .Select(recipient => NotificationsHub.GetUserGroupName(recipient.UserId))
                .Distinct()
                .ToList();

            await _hubContext.Clients
                .Groups(groups)
                .SendAsync("device.telemetry", payload, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error publicando telemetria realtime del dispositivo {DeviceId} para PatientId={PatientId}.",
                snapshot.DeviceId,
                snapshot.PatientId);
        }
    }
}
