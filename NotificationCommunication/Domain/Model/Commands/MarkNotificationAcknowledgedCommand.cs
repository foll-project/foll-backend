namespace foll_backend.NotificationCommunication.Domain.Model.Commands;

public record MarkNotificationAcknowledgedCommand(long UserId, long NotificationLogId);
