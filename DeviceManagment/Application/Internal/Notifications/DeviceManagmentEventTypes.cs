namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public static class DeviceManagmentEventTypes
{
    public const string LowBatteryDetectedV1 = "device-managment.low-battery-detected.v1";
    public const string LowBatteryResolvedV1 = "device-managment.low-battery-resolved.v1";
    public const string DeviceDisconnectedV1 = "device-managment.device-disconnected.v1";
    public const string DeviceReconnectedV1 = "device-managment.device-reconnected.v1";
}
