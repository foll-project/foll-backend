using System.Text.Json.Serialization;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;

public record InvitationChangedRealtimeMessage(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("invitationId")] long InvitationId,
    [property: JsonPropertyName("patientId")] long PatientId,
    [property: JsonPropertyName("patientName")] string PatientName,
    [property: JsonPropertyName("requesterUserId")] long RequesterUserId,
    [property: JsonPropertyName("requesterName")] string RequesterName,
    [property: JsonPropertyName("relationshipTypeId")] short RelationshipTypeId,
    [property: JsonPropertyName("relationshipName")] string RelationshipName,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("occurredAt")] DateTime OccurredAt);
