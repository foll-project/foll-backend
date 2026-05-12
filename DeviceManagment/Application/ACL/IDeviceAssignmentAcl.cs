namespace foll_backend.DeviceManagment.Application.ACL;

public interface IDeviceAssignmentAcl
{
    Task<DeviceAssignmentDto?> GetAssignmentByDeviceIdAsync(long deviceId);
}
