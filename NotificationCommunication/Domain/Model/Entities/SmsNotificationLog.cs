using foll_backend.NotificationCommunication.Domain.Model.Enums;

namespace foll_backend.NotificationCommunication.Domain.Model.Entities;

public class SmsNotificationLog
{
    public long SmsNotificationLogId { get; private set; }
    public Guid IncidentKey { get; private set; }
    public long? UserId { get; private set; }
    public long? EmergencyContactId { get; private set; }
    public long PatientId { get; private set; }
    public long? DeviceId { get; private set; }
    public string RecipientName { get; private set; }
    public string PhoneNumber { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public NotificationStatus NotificationStatus { get; private set; }
    public string Message { get; private set; }
    public string LocationAccessUrl { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected SmsNotificationLog()
    {
        RecipientName = string.Empty;
        PhoneNumber = string.Empty;
        Message = string.Empty;
        LocationAccessUrl = string.Empty;
    }

    public SmsNotificationLog(
        Guid incidentKey,
        long? userId,
        long? emergencyContactId,
        long patientId,
        long? deviceId,
        string recipientName,
        string phoneNumber,
        NotificationType notificationType,
        string message,
        string locationAccessUrl,
        DateTime createdAt)
    {
        if (incidentKey == Guid.Empty) throw new ArgumentException("La clave del incidente es obligatoria.", nameof(incidentKey));
        if (patientId <= 0) throw new ArgumentOutOfRangeException(nameof(patientId));
        if (userId is <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (emergencyContactId is <= 0) throw new ArgumentOutOfRangeException(nameof(emergencyContactId));
        if (string.IsNullOrWhiteSpace(recipientName)) throw new ArgumentException("El nombre del destinatario es obligatorio.", nameof(recipientName));
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("El telefono del destinatario es obligatorio.", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("El mensaje SMS es obligatorio.", nameof(message));
        if (string.IsNullOrWhiteSpace(locationAccessUrl)) throw new ArgumentException("El enlace de ubicacion es obligatorio.", nameof(locationAccessUrl));

        IncidentKey = incidentKey;
        UserId = userId;
        EmergencyContactId = emergencyContactId;
        PatientId = patientId;
        DeviceId = deviceId;
        RecipientName = recipientName.Trim();
        PhoneNumber = phoneNumber.Trim();
        NotificationType = notificationType;
        NotificationStatus = NotificationStatus.Pending;
        Message = message.Trim();
        LocationAccessUrl = locationAccessUrl.Trim();
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
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Error desconocido enviando SMS." : errorMessage.Trim();
        UpdatedAt = NormalizeTimestamp(failedAt);
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
