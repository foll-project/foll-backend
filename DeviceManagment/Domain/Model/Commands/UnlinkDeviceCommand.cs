namespace foll_backend.DeviceManagment.Domain.Model.Commands;

public record UnlinkDeviceCommand(long ActorUserId, long DeviceId);
