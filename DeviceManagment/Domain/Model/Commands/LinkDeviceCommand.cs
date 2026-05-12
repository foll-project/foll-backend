namespace foll_backend.DeviceManagment.Domain.Model.Commands;

public record LinkDeviceCommand(long ActorUserId, long DeviceId, long PatientId);
