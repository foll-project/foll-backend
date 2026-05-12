namespace foll_backend.DeviceManagment.Domain.Model.Commands;

public record RegisterDeviceTelemetryCommand(
    long DeviceId,
    short BatteryLevel,
    bool IsCharging,
    DateTime ReportedAtUtc,
    string? FirmwareVersion = null);
