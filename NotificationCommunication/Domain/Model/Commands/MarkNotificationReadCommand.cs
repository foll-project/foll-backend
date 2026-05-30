namespace foll_backend.NotificationCommunication.Domain.Model.Commands;

public record MarkNotificationReadCommand(long UserId, long NotificationLogId);
