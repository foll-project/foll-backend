namespace foll_backend.DeviceManagment.Domain.Model.Commands;

public record UpdateDevicePowerStateCommand(long DeviceId, bool IsActive, DateTime ReportedAtUtc);
