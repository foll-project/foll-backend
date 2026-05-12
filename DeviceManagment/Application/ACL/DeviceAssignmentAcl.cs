using foll_backend.DeviceManagment.Domain.Repositories;

namespace foll_backend.DeviceManagment.Application.ACL;

public class DeviceAssignmentAcl : IDeviceAssignmentAcl
{
    private readonly IDeviceRepository _deviceRepository;

    public DeviceAssignmentAcl(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<DeviceAssignmentDto?> GetAssignmentByDeviceIdAsync(long deviceId)
    {
        if (deviceId <= 0) return null;

        var device = await _deviceRepository.FindByIdAsync(deviceId);
        if (device is null) return null;

        return new DeviceAssignmentDto(
            device.DeviceId,
            device.AssignedPatientId,
            device.Status.ToString(),
            device.ConnectivityStatus?.ToString());
    }
}
