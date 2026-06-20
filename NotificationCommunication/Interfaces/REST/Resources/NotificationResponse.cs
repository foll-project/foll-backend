namespace foll_backend.NotificationCommunication.Interfaces.REST.Resources;

public record NotificationResponse(
    long NotificationLogId,
    long UserId,
    string NotificationType,
    string NotificationChannel,
    string NotificationStatus,
    string Title,
    string Body,
    string? DataJson,
    string? ProviderMessageId,
    string? ErrorMessage,
    long? DeviceEventId,
    long? PatientId,
    long? DeviceId,
    DateTime? SentAt,
    DateTime? ReadAt,
    DateTime? AcknowledgedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
