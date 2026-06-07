namespace foll_backend.NotificationCommunication.Domain.Model.Enums;

public enum NotificationType
{
    LowBattery = 1,
    BatteryRecovered = 2,
    DeviceDisconnected = 3,
    DeviceReconnected = 4,
    FallDetected = 5,
    Generic = 6
}
