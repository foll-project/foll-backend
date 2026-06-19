using foll_backend.EmergencyAnalytics.Application.Internal.Notifications;
using foll_backend.IAM.Application.ACL;
using foll_backend.NotificationCommunication.Application.OutboundServices;
using foll_backend.NotificationCommunication.Interfaces.Realtime;
using foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace foll_backend.NotificationCommunication.Application.Internal.EventHandlers;

/// <summary>
/// Cuando un incidente se cierra (atendido/resuelto o cancelado), avisa EN TIEMPO REAL
/// a todos los cuidadores del paciente (principal + secundarios) quién lo atendió, para
/// que en cualquier vista desaparezca la alerta activa y se muestre "Atendida por X".
/// </summary>
public class EmergencyIncidentClosedNotificationHandler : INotificationHandler<EmergencyIncidentClosedIntegrationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly IPatientNotificationAccessService _patientNotificationAccessService;
    private readonly IUserInfoAcl _userInfoAcl;
    private readonly ILogger<EmergencyIncidentClosedNotificationHandler> _logger;

    public EmergencyIncidentClosedNotificationHandler(
        IHubContext<NotificationsHub> hubContext,
        IPatientNotificationAccessService patientNotificationAccessService,
        IUserInfoAcl userInfoAcl,
        ILogger<EmergencyIncidentClosedNotificationHandler> logger)
    {
        _hubContext = hubContext;
        _patientNotificationAccessService = patientNotificationAccessService;
        _userInfoAcl = userInfoAcl;
        _logger = logger;
    }

    public async Task Handle(EmergencyIncidentClosedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var recipients = await _patientNotificationAccessService.GetRecipientsForPatientAsync(notification.PatientId);
            if (recipients.Count == 0) return;

            string? closedByName = null;
            if (notification.ClosedByUserId is > 0)
            {
                var userInfo = await _userInfoAcl.GetUserInfoByIdAsync(notification.ClosedByUserId.Value);
                if (userInfo is not null)
                    closedByName = $"{userInfo.FirstName} {userInfo.LastName}".Trim();
            }

            var payload = new IncidentResolvedRealtimeMessage(
                notification.IncidentKey,
                notification.DeviceId,
                notification.PatientId,
                notification.Status.ToString(),
                notification.CancellationReason?.ToString(),
                notification.ClosedByUserId,
                closedByName,
                notification.ClosedAtUtc,
                notification.Observation,
                notification.FallTypeName);

            var groups = recipients
                .Select(recipient => NotificationsHub.GetUserGroupName(recipient.UserId))
                .Distinct()
                .ToList();

            await _hubContext.Clients
                .Groups(groups)
                .SendAsync("incident.resolved", payload, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error publicando incident.resolved en tiempo real para IncidentKey={IncidentKey} PatientId={PatientId}.",
                notification.IncidentKey,
                notification.PatientId);
        }
    }
}
