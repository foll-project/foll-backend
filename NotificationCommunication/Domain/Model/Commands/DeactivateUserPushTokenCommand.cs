namespace foll_backend.NotificationCommunication.Domain.Model.Commands;

public record DeactivateUserPushTokenCommand(long UserId, long UserPushTokenId);
