using foll_backend.NotificationCommunication.Domain.Model.Enums;

namespace foll_backend.NotificationCommunication.Domain.Model.Entities;

public class NotificationLog
{
    public long NotificationLogId { get; private set; }
    public long UserId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public NotificationChannel NotificationChannel { get; private set; }
    public NotificationStatus NotificationStatus { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string? DataJson { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public long? DeviceEventId { get; private set; }
    public long? PatientId { get; private set; }
    public long? DeviceId { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected NotificationLog()
    {
        Title = string.Empty;
        Body = string.Empty;
    }

    public NotificationLog(
        long userId,
        NotificationType notificationType,
        NotificationChannel notificationChannel,
        string title,
        string body,
        string? dataJson,
        long? deviceEventId,
        long? patientId,
        long? deviceId,
        DateTime createdAt)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El titulo de la notificacion es obligatorio.", nameof(title));
        if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("El cuerpo de la notificacion es obligatorio.", nameof(body));

        UserId = userId;
        NotificationType = notificationType;
        NotificationChannel = notificationChannel;
        NotificationStatus = NotificationStatus.Pending;
        Title = title.Trim();
        Body = body.Trim();
        DataJson = string.IsNullOrWhiteSpace(dataJson) ? null : dataJson.Trim();
        DeviceEventId = deviceEventId;
        PatientId = patientId;
        DeviceId = deviceId;
        CreatedAt = NormalizeTimestamp(createdAt);
        UpdatedAt = CreatedAt;
    }

    public void MarkSent(string? providerMessageId, DateTime sentAt)
    {
        NotificationStatus = NotificationStatus.Sent;
        ProviderMessageId = string.IsNullOrWhiteSpace(providerMessageId) ? null : providerMessageId.Trim();
        ErrorMessage = null;
        SentAt = NormalizeTimestamp(sentAt);
        UpdatedAt = SentAt.Value;
    }

    public void MarkFailed(string errorMessage, DateTime failedAt)
    {
        NotificationStatus = NotificationStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Error desconocido enviando la notificacion." : errorMessage.Trim();
        UpdatedAt = NormalizeTimestamp(failedAt);
    }

    public void MarkRead(DateTime readAt)
    {
        if (ReadAt.HasValue) return;

        ReadAt = NormalizeTimestamp(readAt);
        if (NotificationStatus is NotificationStatus.Pending or NotificationStatus.Sent or NotificationStatus.Failed)
            NotificationStatus = NotificationStatus.Read;

        UpdatedAt = ReadAt.Value;
    }

    public void MarkAcknowledged(DateTime acknowledgedAt)
    {
        if (AcknowledgedAt.HasValue) return;

        AcknowledgedAt = NormalizeTimestamp(acknowledgedAt);
        NotificationStatus = NotificationStatus.Acknowledged;
        UpdatedAt = AcknowledgedAt.Value;
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }
}
