using foll_backend.DeviceManagment.Application.ACL;
using foll_backend.EmergencyAnalytics.Application.OutboundServices;

namespace foll_backend.EmergencyAnalytics.Application.ACL;

public class DeviceIncidentAssignmentService : IDeviceIncidentAssignmentService
{
    private readonly IDeviceAssignmentAcl _deviceAssignmentAcl;

    public DeviceIncidentAssignmentService(IDeviceAssignmentAcl deviceAssignmentAcl)
    {
        _deviceAssignmentAcl = deviceAssignmentAcl;
    }

    public async Task<long?> FindAssignedPatientIdAsync(long deviceId)
    {
        var assignment = await _deviceAssignmentAcl.GetAssignmentByDeviceIdAsync(deviceId);
        return assignment?.AssignedPatientId;
    }
}
