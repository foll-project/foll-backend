using foll_backend.NotificationCommunication.Domain.Model.Enums;

namespace foll_backend.NotificationCommunication.Domain.Model.Commands;

public record RegisterUserPushTokenCommand(
    long UserId,
    string Token,
    PushPlatform Platform,
    string? DeviceName);
