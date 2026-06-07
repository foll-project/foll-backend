namespace foll_backend.NotificationCommunication.Domain.Model.Queries;

public record GetNotificationDeliveryStatusQuery(long UserId, long NotificationLogId);
