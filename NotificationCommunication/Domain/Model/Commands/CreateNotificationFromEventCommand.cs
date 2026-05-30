using foll_backend.NotificationCommunication.Domain.Model.Enums;

namespace foll_backend.NotificationCommunication.Domain.Model.Commands;

public record CreateNotificationFromEventCommand(
    long PatientId,
    long? DeviceId,
    long? DeviceEventId,
    NotificationType NotificationType,
    string Title,
    string Body,
    string? DataJson);
