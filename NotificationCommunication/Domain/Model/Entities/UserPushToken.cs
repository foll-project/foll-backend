using foll_backend.NotificationCommunication.Domain.Model.Enums;

namespace foll_backend.NotificationCommunication.Domain.Model.Entities;

public class UserPushToken
{
    public long UserPushTokenId { get; private set; }
    public long UserId { get; private set; }
    public string Token { get; private set; }
    public PushPlatform Platform { get; private set; }
    public string? DeviceName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected UserPushToken()
    {
        Token = string.Empty;
    }

    public UserPushToken(long userId, string token, PushPlatform platform, string? deviceName, DateTime createdAt)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("El token push es obligatorio.", nameof(token));

        UserId = userId;
        Token = token.Trim();
        Platform = platform;
        DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim();
        IsActive = true;
        CreatedAt = NormalizeTimestamp(createdAt);
        UpdatedAt = CreatedAt;
    }

    public void Refresh(PushPlatform platform, string? deviceName, DateTime updatedAt)
    {
        Platform = platform;
        DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim();
        IsActive = true;
        UpdatedAt = NormalizeTimestamp(updatedAt);
    }

    public void MarkUsed(DateTime usedAt)
    {
        LastUsedAt = NormalizeTimestamp(usedAt);
        UpdatedAt = LastUsedAt.Value;
    }

    public void Deactivate(DateTime updatedAt)
    {
        IsActive = false;
        UpdatedAt = NormalizeTimestamp(updatedAt);
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
