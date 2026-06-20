namespace foll_backend.NotificationCommunication.Interfaces.REST.Resources;

public record UserPushTokenResponse(
    long UserPushTokenId,
    long UserId,
    string Token,
    string Platform,
    string? DeviceName,
    bool IsActive,
    DateTime? LastUsedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
