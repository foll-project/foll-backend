namespace foll_backend.DeviceManagment.Application.ACL;

public record DeviceStatusDto(
    long DeviceId,
    long? AssignedPatientId,
    string Status,
    string? ConnectivityStatus,
    short? CurrentBatteryLevel,
    bool? IsCharging,
    DateTime? LastHeartbeatAt,
    DateTime? MonitoringStartedAt,
    DateTime? LastConnectivityChangeAt,
    bool IsOnline,
    bool IsLowBattery,
    string FirmwareVersion);
