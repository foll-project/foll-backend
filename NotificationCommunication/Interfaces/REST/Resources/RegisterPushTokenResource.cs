namespace foll_backend.NotificationCommunication.Interfaces.REST.Resources;

public record RegisterPushTokenResource(
    string Token,
    string? Platform,
    string? DeviceName);
