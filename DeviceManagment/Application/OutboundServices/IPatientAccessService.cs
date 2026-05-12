namespace foll_backend.DeviceManagment.Application.OutboundServices;

public interface IPatientAccessService
{
    Task<bool> CanManageDevicesAsync(long actorUserId, long patientId);
}
