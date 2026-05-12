namespace foll_backend.DeviceManagment.Infrastructure.Configuration;

public class DeviceMonitoringOptions
{
    public short LowBatteryThreshold { get; set; } = 15;
    public int ExpectedHeartbeatIntervalSeconds { get; set; } = 10;
    public int MissedHeartbeatsBeforeDisconnect { get; set; } = 3;
    public int ConnectivityCheckIntervalSeconds { get; set; } = 5;
}
