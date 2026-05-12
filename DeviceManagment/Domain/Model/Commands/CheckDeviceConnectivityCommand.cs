namespace foll_backend.DeviceManagment.Domain.Model.Commands;

public record CheckDeviceConnectivityCommand(long DeviceId, DateTime DetectedAtUtc, TimeSpan DisconnectThreshold);
