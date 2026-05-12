namespace foll_backend.DeviceManagment.Domain.Model.Enums;

public enum ConnectivityChangeCause
{
    HeartbeatTimeout = 1,
    PowerOff = 2,
    HeartbeatReceived = 3,
    PowerOn = 4
}
