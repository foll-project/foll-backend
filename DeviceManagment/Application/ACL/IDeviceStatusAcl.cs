namespace foll_backend.DeviceManagment.Application.ACL;

public interface IDeviceStatusAcl
{
    Task<DeviceStatusDto?> GetStatusByPatientIdAsync(long patientId);
}
