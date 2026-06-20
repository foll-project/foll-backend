using System.Text.Json.Serialization;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime.Resources;

public record NotificationCreatedRealtimeMessage(
    [property: JsonPropertyName("notificationLogId")] long NotificationLogId,
    [property: JsonPropertyName("userId")] long UserId,
    [property: JsonPropertyName("notificationType")] string NotificationType,
    [property: JsonPropertyName("notificationChannel")] string NotificationChannel,
    [property: JsonPropertyName("notificationStatus")] string NotificationStatus,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("dataJson")] string? DataJson,
    [property: JsonPropertyName("patientId")] long? PatientId,
    [property: JsonPropertyName("deviceId")] long? DeviceId,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt);
