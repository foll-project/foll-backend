namespace foll_backend.Care.Application.OutboundServices;

// Evento de invitación que se empuja en tiempo real a un usuario concreto.
public record InvitationRealtimeEvent(
    string Kind,            // "created" | "accepted" | "rejected"
    long TargetUserId,      // destinatario del push
    long InvitationId,
    long PatientId,
    string PatientName,
    long RequesterUserId,
    string RequesterName,
    short RelationshipTypeId,
    string RelationshipName,
    string Status,          // Pending | Accepted | Rejected
    string Title,
    string Message,
    DateTime OccurredAt);

// Puerto de salida de Care: lo implementa NotificationCommunication (dueño del Hub SignalR).
public interface IInvitationRealtimePublisher
{
    Task PublishAsync(InvitationRealtimeEvent realtimeEvent, CancellationToken cancellationToken = default);
}
