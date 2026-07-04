namespace foll_backend.DeviceManagment.Domain.Model.Commands;

public record UnlinkDevicesByAccountCommand(IEnumerable<long> PatientIds, long ActorUserId);
