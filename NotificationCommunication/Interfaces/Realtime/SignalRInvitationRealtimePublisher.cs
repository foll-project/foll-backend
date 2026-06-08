using foll_backend.Care.Application.OutboundServices;
using foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;
using Microsoft.AspNetCore.SignalR;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime;

// Implementa el puerto de Care empujando el evento por el Hub SignalR existente,
// dirigido SOLO al grupo del usuario destinatario (user:{TargetUserId}).
public class SignalRInvitationRealtimePublisher : IInvitationRealtimePublisher
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly ILogger<SignalRInvitationRealtimePublisher> _logger;

    public SignalRInvitationRealtimePublisher(
        IHubContext<NotificationsHub> hubContext,
        ILogger<SignalRInvitationRealtimePublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(InvitationRealtimeEvent realtimeEvent, CancellationToken cancellationToken = default)
    {
        if (realtimeEvent.TargetUserId <= 0) return;

        try
        {
            var payload = new InvitationChangedRealtimeMessage(
                realtimeEvent.Kind,
                realtimeEvent.InvitationId,
                realtimeEvent.PatientId,
                realtimeEvent.PatientName,
                realtimeEvent.RequesterUserId,
                realtimeEvent.RequesterName,
                realtimeEvent.RelationshipTypeId,
                realtimeEvent.RelationshipName,
                realtimeEvent.Status,
                realtimeEvent.Title,
                realtimeEvent.Message,
                realtimeEvent.OccurredAt);

            await _hubContext.Clients
                .Group(NotificationsHub.GetUserGroupName(realtimeEvent.TargetUserId))
                .SendAsync("invitation.changed", payload, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error publicando invitacion realtime {InvitationId} para UserId={UserId}.",
                realtimeEvent.InvitationId,
                realtimeEvent.TargetUserId);
        }
    }
}
