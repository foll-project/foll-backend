using System.Text.Json.Serialization;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;

/// <summary>
/// Mensaje en tiempo real que avisa a TODOS los cuidadores (principal + secundarios)
/// que una caída fue atendida/cerrada y POR QUIÉN. Es un evento de coordinación
/// efímero (no se persiste como notificación): el historial del incidente ya guarda
/// el estado final y el ClosedByUserId.
/// </summary>
public record IncidentResolvedRealtimeMessage(
    [property: JsonPropertyName("incidentKey")] Guid IncidentKey,
    [property: JsonPropertyName("deviceId")] long DeviceId,
    [property: JsonPropertyName("patientId")] long PatientId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("cancellationReason")] string? CancellationReason,
    [property: JsonPropertyName("closedByUserId")] long? ClosedByUserId,
    [property: JsonPropertyName("closedByName")] string? ClosedByName,
    [property: JsonPropertyName("closedAt")] DateTime ClosedAtUtc,
    [property: JsonPropertyName("observation")] string? Observation,
    [property: JsonPropertyName("fallTypeName")] string FallTypeName);
